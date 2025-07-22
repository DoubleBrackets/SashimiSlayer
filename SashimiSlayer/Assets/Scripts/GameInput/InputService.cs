using System;
using System.Linq;
using Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameInput
{
    public enum ControlSchemes
    {
        KeyboardMouse,
        Gamepad,
        SwordJoystick,
        SwordSerial
    }

    public class InputService : BaseUserInputProvider
    {
        [Header("Event (Out)")]

        [SerializeField]
        private IntEvent _onControlSchemeChanged;

        [Header("Event (In)")]

        [SerializeField]
        private BoolEvent _setUseSerialInput;

        [Header("Depends")]

        [SerializeField]
        private BaseUserInputProvider _hidInputProvider;

        [SerializeField]
        private SwordInputProvider _serialInputProvider;

        [SerializeField]
        private bool _useSerialController;

        [SerializeField]
        private VoidEvent _onDrawDebugGUI;

        public static InputService Instance { get; private set; }

        private BaseUserInputProvider InputProvider => _useSerialController ? _serialInputProvider : _hidInputProvider;

        public ControlSchemes ControlScheme { get; private set; }

        public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
        public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            EventPassthroughSub();

            _onDrawDebugGUI.AddListener(HandleDrawDebugGUI);
            _setUseSerialInput.AddListener(HandleSetUseSerialInput);

            InputSystem.onDeviceChange += (device, change) => { UpdateControlScheme(); };
        }

        private void OnDestroy()
        {
            EventPassthroughUnsub();

            _onDrawDebugGUI.RemoveListener(HandleDrawDebugGUI);
            _setUseSerialInput.RemoveListener(HandleSetUseSerialInput);
        }

        private void HandleSetUseSerialInput(bool useSerialInput)
        {
            Debug.Log($"Setting useHardwareController to {useSerialInput}");
            EventPassthroughUnsub();

            _useSerialController = useSerialInput;

            UpdateControlScheme();

            EventPassthroughSub();
        }

        private void UpdateControlScheme()
        {
            if (_useSerialController)
            {
                ControlScheme = ControlSchemes.SwordSerial;
            }
            else if (InputSystem.devices.Count(device => device is Joystick) > 0)
            {
                ControlScheme = ControlSchemes.SwordJoystick;
            }
            else if (InputSystem.devices.Count(device => device is Gamepad) > 0)
            {
                ControlScheme = ControlSchemes.Gamepad;
            }
            else
            {
                ControlScheme = ControlSchemes.KeyboardMouse;
            }

            _onControlSchemeChanged.Raise((int)ControlScheme);
        }

        private void HandleDrawDebugGUI()
        {
            GUILayout.Label($"control scheme: {ControlScheme}");
        }

        private void EventPassthroughSub()
        {
            InputProvider.OnBlockPoseChanged += HandleBlockPoseChanged;
            InputProvider.OnSheathStateChanged += HandleSheatheStateChanged;
        }

        private void EventPassthroughUnsub()
        {
            InputProvider.OnBlockPoseChanged -= HandleBlockPoseChanged;
            InputProvider.OnSheathStateChanged -= HandleSheatheStateChanged;
        }

        private void HandleBlockPoseChanged(SharedTypes.BlockPoseStates state)
        {
            OnBlockPoseChanged?.Invoke(state);
        }

        private void HandleSheatheStateChanged(SharedTypes.SheathState state)
        {
            OnSheathStateChanged?.Invoke(state);
        }

        public override float GetSwordAngle()
        {
            return InputProvider.GetSwordAngle();
        }

        public override SharedTypes.SheathState GetSheathState()
        {
            return InputProvider.GetSheathState();
        }

        public override SharedTypes.BlockPoseStates GetBlockPose()
        {
            return InputProvider.GetBlockPose();
        }
    }
}