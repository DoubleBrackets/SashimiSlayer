using System;
using CommonTypes;
using Events.Basic;
using GameInput.Haptics;
using GameInput.SerialComm;
using UnityEngine;

namespace GameInput.InputSources
{
    /// <summary>
    ///     Handles interpreting sword controller data into game inputs.
    /// </summary>
    public class SerialSwordInputSource : MonoBehaviour, IUserInputSource
    {
        private enum UpAxis
        {
            X,
            Y,
            Z
        }

        [Header("Depends")]

        [SerializeField]
        private SwordSerialReader _serialReader;

        [SerializeField]
        private Transform _quatDebugger;

        [Header("Channels (In)")]

        [SerializeField]
        private StringEvent _connectToSerialPort;

        [SerializeField]
        private IntEvent _upAxisChangedEvent;

        public event Action<BlockPoses> OnBlockPoseChanged;
        public event Action<SheathState> OnSheathStateChanged;
        public event Action OnToggleMenuInput;
        public event Action OnToggleSkipLooping;

        private UpAxis _upAxis = UpAxis.Y;

        private SheathState _sheathState = SheathState.Sheathed;
        private float _swordAngle = 90f;

        private bool _wasTopButtonPressed;
        private bool _wasMiddleButtonPressed;

        private BlockPoses _currentBlockPose;

        private void Awake()
        {
            _serialReader.OnSerialRead += HandleSerialRead;

            _connectToSerialPort.AddListener(ConnectToPort);
            _upAxisChangedEvent.AddListener(HandleUpAxisChanged);
        }

        private void OnDestroy()
        {
            _serialReader.OnSerialRead -= HandleSerialRead;

            _connectToSerialPort.RemoveListener(ConnectToPort);
            _upAxisChangedEvent.RemoveListener(HandleUpAxisChanged);
        }

        private void HandleUpAxisChanged(int axis)
        {
            _upAxis = (UpAxis)axis;
        }

        private void HandleSerialRead(SwordSerialReader.SerialReadResult data)
        {
            SheathState newSheathSheathState = data.LeftSheatheSwitch && data.RightSheatheSwitch
                ? SheathState.Unsheathed
                : SheathState.Sheathed;

            if (newSheathSheathState != _sheathState)
            {
                _sheathState = newSheathSheathState;
                OnSheathStateChanged?.Invoke(_sheathState);

                // Toggle menu by unsheathing when all buttons are pressed
                if (_sheathState == SheathState.Unsheathed && data.TopButton && data.MiddleButton)
                {
                    OnToggleMenuInput?.Invoke();
                }
            }

            if (data.TopButton && !_wasTopButtonPressed)
            {
                _currentBlockPose = BlockPoses.BlockRight;
                OnBlockPoseChanged?.Invoke(BlockPoses.BlockRight);
            }

            if (data.MiddleButton && !_wasMiddleButtonPressed)
            {
                _currentBlockPose = BlockPoses.BlockLeft;
                OnBlockPoseChanged?.Invoke(BlockPoses.BlockLeft);
            }

            _wasTopButtonPressed = data.TopButton;
            _wasMiddleButtonPressed = data.MiddleButton;

            _swordAngle = ProcessSwordOrientation(data.SwordOrientation);
            _quatDebugger.transform.rotation = data.SwordOrientation;
        }

        /// <summary>
        ///     Converts the quaternion to the in-game float angle
        /// </summary>
        /// <param name="quat"></param>
        /// <returns></returns>
        private float ProcessSwordOrientation(Quaternion quat)
        {
            Vector3 upAxis = Vector3.zero;

            switch (_upAxis)
            {
                case UpAxis.X:
                    upAxis = Vector3.right;
                    break;
                case UpAxis.Y:
                    upAxis = Vector3.up;
                    break;
                case UpAxis.Z:
                    upAxis = Vector3.forward;
                    break;
            }

            Vector3 up = quat * upAxis;
            float angle = -Vector3.Angle(up, Vector3.up) + 90f;
            return angle;
        }

        public float GetSwordAngle()
        {
            return _swordAngle;
        }

        public SheathState GetSheathState()
        {
            return _sheathState;
        }

        public BlockPoses GetBlockPose()
        {
            return _currentBlockPose;
        }

        public void AddRumble(RumbleFeedbackSO rumbleFeedback)
        {
            // Not supported
        }

        public bool GetLeftHandleSwordIdentify()
        {
            return false;
        }

        public bool GetCustomSwordControllerIdentify()
        {
            return true;
        }

        public void ConnectToPort(string portName)
        {
            _serialReader.TryConnectToPort(portName);
        }
    }
}