using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using Cysharp.Threading.Tasks;
using Events;
using UnityEngine;

namespace GameInput
{
    /// <summary>
    ///     Reads serial data from the sword controller
    /// </summary>
    public class SwordSerialReader : MonoBehaviour
    {
        public struct SerialReadResult
        {
            public bool TopButton;
            public bool MiddleButton;
            public bool LeftSheatheSwitch;
            public bool RightSheatheSwitch;
            public Quaternion SwordOrientation;
        }

        private const int RateCountWindow = 10;

        /// <summary>
        ///     1 byte for buttons, 16 bytes for quaternion
        /// </summary>
        private const int PacketByteCount = 17;

        public static bool LogPackets;

        [SerializeField]
        private int _baudRate;

        [Header("Config")]

        [SerializeField]
        private VoidEvent _onDrawDebugGUI;

        [Header("Events (Out)")]

        [SerializeField]
        private StringEvent _serialPortConnectionStatus;

        public bool AbleToConnect { get; private set; }

        public event Action<SerialReadResult> OnSerialRead;

        private readonly Queue<float> _intervalQueue = new();
        private readonly byte[] _packetBuffer = new byte[PacketByteCount];

        private SerialPort _serialPort;
        private float _intervalSum;
        private float _prevPacketTime;
        private float _currentPacketRate;

        private int _toDiscard;

        private void Awake()
        {
            _onDrawDebugGUI.AddListener(DrawDebugGUI);
        }

        private void Start()
        {
            _serialPortConnectionStatus.Raise("Not connected to serial port");
        }

        private void OnDestroy()
        {
            try
            {
                CleanUp();
            }
            catch (Exception e)
            {
                ShowException(e);
            }

            _onDrawDebugGUI.RemoveListener(DrawDebugGUI);
        }

        public void TryConnectToPort(string portName)
        {
            try
            {
                CleanUp();
            }
            catch (Exception e)
            {
                ShowException(e);
            }

            try
            {
                _toDiscard = 10;
                _serialPortConnectionStatus.Raise("Connecting to serial port");

                InitializeSerialPort(portName);
                AbleToConnect = true;

                _serialPortConnectionStatus.Raise("Connected to serial port");

                _serialPort.WriteTimeout = 1000;
                ReadLoop(this.GetCancellationTokenOnDestroy()).Forget();
            }
            catch (Exception e)
            {
                ShowException(e);
                AbleToConnect = false;
            }
        }

        private void DrawDebugGUI()
        {
            GUILayout.Label("Connected to arduino: " + (_serialPort == null ? "No" : _serialPort.IsOpen));
            GUILayout.Label("Serial Packet Rate: " + _currentPacketRate);
        }

        private void InitializeSerialPort(string arduinoPort)
        {
            Debug.Log($"Connecting to {arduinoPort}");

            _serialPort = new SerialPort(arduinoPort, _baudRate);

            // Disable Rts since we don't use handshaking
            // Doesn't work on Mac unless we do this
            _serialPort.RtsEnable = true;

            // We don't need to enable Dtr to get it to work on mac, but leaving it here in case
            // _serialPort.DtrEnable = true;

            _serialPort.Open();
            _serialPort.ErrorReceived += HandleErrorReceived;
        }

        private void CleanUp()
        {
            if (_serialPort == null)
            {
                return;
            }

            _serialPort.ErrorReceived -= HandleErrorReceived;
            _serialPort.Close();
            _serialPort.Dispose();
        }

        private void HandleErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Debug.Log(e.ToString());
            _serialPortConnectionStatus.Raise(e.ToString());
        }

        private async UniTaskVoid ReadLoop(CancellationToken cancellationToken)
        {
            try
            {
                Debug.Log(_serialPort.BytesToRead);
                // Discard any junk in the buffer
                _serialPort.DiscardInBuffer();

                // Starting ack
                Write(new byte[] { 255 });

                while (true)
                {
                    await UniTask.Yield();
                    cancellationToken.ThrowIfCancellationRequested();
                    ProcessAllFromPort();
                }
            }
            catch (Exception e)
            {
                ShowException(e);
            }
            finally
            {
                CleanUp();
            }
        }

        private void ShowException(Exception e)
        {
            Debug.Log(e.Message);
            _serialPortConnectionStatus.Raise(e.GetType() + ":" + e.Message);
        }

        private void ProcessAllFromPort()
        {
            int bytesToRead = _serialPort.BytesToRead;
            int packetsToRead = bytesToRead / PacketByteCount;

            if (packetsToRead == 0)
            {
                return;
            }

            // Read one packet into buffer
            var byteIndex = 0;
            while (byteIndex < PacketByteCount)
            {
                var readByte = (byte)_serialPort.ReadByte();
                _packetBuffer[byteIndex] = readByte;
                byteIndex++;
            }

            // Calculate the packet rate
            SamplePacketInterval();

            // Parse the packet
            SerialReadResult serialReadResult = ParsePacket(_packetBuffer);

            // Discard some packets at the start
            if (_toDiscard > 0)
            {
                _toDiscard--;
            }
            else
            {
                OnSerialRead?.Invoke(serialReadResult);
            }

            if (LogPackets)
            {
                var packet = string.Empty;
                foreach (byte b in _packetBuffer)
                {
                    // log byte as binary
                    packet += Convert.ToString(b, 2).PadLeft(8, '0') + " ";
                }

                Debug.Log(packetsToRead);
                Debug.Log(JsonUtility.ToJson(serialReadResult));
                Debug.Log(packet);
            }

            // Only read one packet per frame (because of handshake there should only be one)
            _serialPort.DiscardInBuffer();

            // Write ack to arduino for next packet
            Write(new byte[] { 255 });
        }

        private SerialReadResult ParsePacket(byte[] packetBuffer)
        {
            var quatOffset = 1;
            var x = BitConverter.ToSingle(packetBuffer, quatOffset);
            var y = BitConverter.ToSingle(packetBuffer, quatOffset + 4);
            var z = BitConverter.ToSingle(packetBuffer, quatOffset + 8);
            var w = BitConverter.ToSingle(packetBuffer, quatOffset + 12);

            var orientation = new Quaternion(x, -z, y, w);

            var serialReadResult = new SerialReadResult
            {
                TopButton = (_packetBuffer[0] & 1) == 1,
                MiddleButton = (_packetBuffer[0] & 2) == 2,
                LeftSheatheSwitch = (_packetBuffer[0] & 8) == 8,
                RightSheatheSwitch = (_packetBuffer[0] & 16) == 16,
                SwordOrientation = orientation
            };

            return serialReadResult;
        }

        private void SamplePacketInterval()
        {
            float interval = Time.time - _prevPacketTime;

            _intervalSum += interval;
            _intervalQueue.Enqueue(interval);

            if (_intervalQueue.Count > RateCountWindow)
            {
                _intervalSum -= _intervalQueue.Dequeue();
            }

            _currentPacketRate = RateCountWindow / _intervalSum;

            _prevPacketTime = Time.time;
        }

        public void Write(string data)
        {
            _serialPort.Write(data);
        }

        private void Write(byte[] data)
        {
            try
            {
                _serialPort.Write(data, 0, data.Length);
            }
            catch (Exception e)
            {
                Debug.Log(e);
                throw;
            }
        }
    }
}