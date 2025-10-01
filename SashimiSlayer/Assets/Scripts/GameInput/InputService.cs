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

        [SerializeField]
        private FloatEvent _angleMultiplierEvent;

        [SerializeField]
        private FloatEvent _swordAngleOffsetEvent;

        [SerializeField]
        private BoolEvent _setFlipParryDirection;

        [Header("Depends")]

        [SerializeField]
        private BaseUserInputProvider _hidInputProvider;

        [SerializeField]
        private SwordInputProvider _serialInputProvider;

        [SerializeField]
        private bool _useSerialController;

        [SerializeField]
        private VoidEvent _onDrawDebugGUI;

        [Header("Config")]

        [Tooltip("Debounce time between sheathing and unsheathing slice to prevent bouncing")]
        [SerializeField]
        private float _sliceDebounce;

        public static InputService Instance { get; private set; }

        private BaseUserInputProvider InputProvider => _useSerialController ? _serialInputProvider : _hidInputProvider;

        public ControlSchemes ControlScheme { get; private set; }

        public bool FlipParryDirection { get; private set; }

        public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
        public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

        private float _angleMultiplier = 1f;
        private float _angleOffset;

        private float _lastSheathedTime;

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
            _angleMultiplierEvent.AddListener(SetAngleMultiplier);
            _swordAngleOffsetEvent.AddListener(SetAngleOffset);
            _setFlipParryDirection.AddListener(SetInvertDirectionalBlockInputs);

            InputSystem.onDeviceChange += (device, change) => { UpdateControlScheme(); };
        }

        private void OnDestroy()
        {
            EventPassthroughUnsub();

            _onDrawDebugGUI.RemoveListener(HandleDrawDebugGUI);
            _setUseSerialInput.RemoveListener(HandleSetUseSerialInput);
            _angleMultiplierEvent.RemoveListener(SetAngleMultiplier);
            _swordAngleOffsetEvent.RemoveListener(SetAngleOffset);
            _setFlipParryDirection.RemoveListener(SetInvertDirectionalBlockInputs);
        }

        private void SetInvertDirectionalBlockInputs(bool invert)
        {
            FlipParryDirection = invert;
        }

        private void SetAngleMultiplier(float angleMultiplier)
        {
            _angleMultiplier = angleMultiplier;
        }

        private void SetAngleOffset(float angleOffset)
        {
            _angleOffset = angleOffset;
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
            if (state == SharedTypes.SheathState.Unsheathed)
            {
                if (Time.time < _lastSheathedTime + _sliceDebounce)
                {
                    float timeSinceSheathed = Time.time - _lastSheathedTime;
                    Debug.LogWarning(
                        $"Unsheathing too soon after slicing, ignoring unsheathe request. {timeSinceSheathed:F2}s since last sheathed.");
                    return;
                }
            }

            if (state == SharedTypes.SheathState.Sheathed)
            {
                _lastSheathedTime = Time.time;
            }

            OnSheathStateChanged?.Invoke(state);
        }

        public override float GetSwordAngle()
        {
            return ConfiguredSwordAngle(InputProvider.GetSwordAngle());
        }

        /// <summary>
        ///     Process raw input angle with settings configuration
        /// </summary>
        /// <param name="rawSwordAngled"></param>
        /// <returns></returns>
        private float ConfiguredSwordAngle(float rawSwordAngled)
        {
            // We want to add multipler first so that the "horizontal" angle remains the same
            // i.e offset of -25 degrees means holding physical sword at 25 degrees hilt-up is horizontal
            // regardless of mult or flipping
            return (rawSwordAngled + _angleOffset) * _angleMultiplier;
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