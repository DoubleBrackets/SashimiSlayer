using System;
using Events;
using UnityEngine;

namespace GameInput
{
    /// <summary>
    ///     Handles interpreting sword controller data into game inputs
    /// </summary>
    public class SwordInputProvider : BaseUserInputProvider
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

        [Header("Events (In)")]

        [SerializeField]
        private FloatEvent _angleMultiplierEvent;

        [SerializeField]
        private FloatEvent _swordAngleOffsetEvent;

        [SerializeField]
        private StringEvent _connectToSerialPort;

        [SerializeField]
        private IntEvent _upAxisChangedEvent;

        public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
        public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

        private UpAxis _upAxis = UpAxis.Y;

        private SharedTypes.SheathState _sheathState = SharedTypes.SheathState.Sheathed;
        private float _swordAngle = 90f;

        private bool _wasTopButtonPressed;
        private bool _wasMiddleButtonPressed;

        private SharedTypes.BlockPoseStates _currentBlockPose;

        private float _angleMultiplier = 1f;
        private float _angleOffset;

        private void Awake()
        {
            _serialReader.OnSerialRead += HandleSerialRead;
            _angleMultiplierEvent.AddListener(SetAngleMultiplier);
            _swordAngleOffsetEvent.AddListener(SetAngleOffset);
            _connectToSerialPort.AddListener(ConnectToPort);
            _upAxisChangedEvent.AddListener(HandleUpAxisChanged);
        }

        private void OnDestroy()
        {
            _serialReader.OnSerialRead -= HandleSerialRead;
            _angleMultiplierEvent.RemoveListener(SetAngleMultiplier);
            _swordAngleOffsetEvent.RemoveListener(SetAngleOffset);
            _connectToSerialPort.RemoveListener(ConnectToPort);
            _upAxisChangedEvent.RemoveListener(HandleUpAxisChanged);
        }

        private void HandleUpAxisChanged(int axis)
        {
            _upAxis = (UpAxis)axis;
        }

        private void SetAngleMultiplier(float angleMultiplier)
        {
            _angleMultiplier = angleMultiplier;
        }

        private void SetAngleOffset(float angleOffset)
        {
            _angleOffset = angleOffset;
        }

        private void HandleSerialRead(SwordSerialReader.SerialReadResult data)
        {
            SharedTypes.SheathState newSheatheState = data.LeftSheatheSwitch && data.RightSheatheSwitch
                ? SharedTypes.SheathState.Unsheathed
                : SharedTypes.SheathState.Sheathed;

            if (newSheatheState != _sheathState)
            {
                _sheathState = newSheatheState;
                OnSheathStateChanged?.Invoke(_sheathState);
            }

            SharedTypes.BlockPoseStates newPose = 0;

            if (data.TopButton && !_wasTopButtonPressed)
            {
                _currentBlockPose = SharedTypes.BlockPoseStates.TopPose;
                OnBlockPoseChanged?.Invoke(SharedTypes.BlockPoseStates.TopPose);
            }

            if (data.MiddleButton && !_wasMiddleButtonPressed)
            {
                _currentBlockPose = SharedTypes.BlockPoseStates.MidPose;
                OnBlockPoseChanged?.Invoke(SharedTypes.BlockPoseStates.MidPose);
            }

            _wasTopButtonPressed = data.TopButton;
            _wasMiddleButtonPressed = data.MiddleButton;

            _swordAngle = ProcessSwordOrientation(data.SwordOrientation) + _angleOffset;
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
            angle *= _angleMultiplier;
            return angle;
        }

        public override float GetSwordAngle()
        {
            return _swordAngle;
        }

        public override SharedTypes.SheathState GetSheathState()
        {
            return _sheathState;
        }

        public override SharedTypes.BlockPoseStates GetBlockPose()
        {
            return _currentBlockPose;
        }

        public void ConnectToPort(string portName)
        {
            _serialReader.TryConnectToPort(portName);
        }
    }
}