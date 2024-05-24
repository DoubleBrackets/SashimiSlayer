using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class SerialReader : MonoBehaviour
{
    public struct SerialReadResult
    {
        public bool TopButton;
        public bool MiddleButton;
        public bool BottomButton;
        public bool LeftSheatheSwitch;
        public bool RightSheatheSwitch;
        public Quaternion SwordOrientation;
    }

    private const int RateCountWindow = 10;

    // 1 byte for buttons, 16 bytes for quaternion
    private const int PacketByteCount = 17;

    [SerializeField]
    private int _baudRate;

    [SerializeField]
    private string _portName;

    [Header("Config")]

    [SerializeField]
    private bool _logPackets;

    public event Action<SerialReadResult> OnSerialRead;

    private readonly Queue<float> _intervalQueue = new();
    private readonly byte[] _packetBuffer = new byte[PacketByteCount];

    private SerialPort _serialPort;
    private float _intervalSum;
    private float _prevPacketTime;
    private float _currentPacketRate;

    private int _currentPacketLength;

    private void Awake()
    {
        InitializeSerialPort();
        _serialPort.WriteTimeout = 1000;
        ReadLoop(this.GetCancellationTokenOnDestroy()).Forget();
    }

    private void OnDestroy()
    {
        try
        {
            _serialPort.Close();
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }

    private void OnGUI()
    {
        GUILayout.Space(150);
        GUILayout.Label("Serial Packet Rate: " + _currentPacketRate);
    }

    private void InitializeSerialPort()
    {
        _serialPort = new SerialPort(_portName, _baudRate);
        _serialPort.Open();
    }

    private async UniTaskVoid ReadLoop(CancellationToken cancellationToken)
    {
        try
        {
            // Discard any junk in the buffer
            _serialPort.ReadExisting();

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
            Debug.Log(e.Message);
        }
        finally
        {
            _serialPort.Close();
        }
    }

    private void ProcessAllFromPort()
    {
        while (_serialPort.BytesToRead > 0)
        {
            var readByte = (byte)_serialPort.ReadByte();
            _packetBuffer[_currentPacketLength] = readByte;
            _currentPacketLength++;

            if (_currentPacketLength == PacketByteCount)
            {
                SamplePacketInterval();

                SerialReadResult serialReadResult = ParsePacket(_packetBuffer);

                OnSerialRead?.Invoke(serialReadResult);

                if (_logPackets)
                {
                    var packet = string.Empty;
                    foreach (byte b in _packetBuffer)
                    {
                        // log byte as binary
                        packet += Convert.ToString(b, 2).PadLeft(8, '0') + " ";
                    }

                    Debug.Log(JsonUtility.ToJson(serialReadResult));
                    Debug.Log(packet);
                }

                _currentPacketLength = 0;

                _serialPort.ReadExisting();
                // Starting ack
                Write(new byte[] { 255 });

                break;
            }
        }
    }

    private SerialReadResult ParsePacket(byte[] packetBuffer)
    {
        var x = BitConverter.ToSingle(packetBuffer, 1);
        var y = BitConverter.ToSingle(packetBuffer, 5);
        var z = BitConverter.ToSingle(packetBuffer, 9);
        var w = BitConverter.ToSingle(packetBuffer, 13);

        var orientation = new Quaternion(x, -z, y, w);

        var serialReadResult = new SerialReadResult
        {
            TopButton = (_packetBuffer[0] & 1) == 1,
            MiddleButton = (_packetBuffer[0] & 2) == 2,
            BottomButton = (_packetBuffer[0] & 4) == 4,
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