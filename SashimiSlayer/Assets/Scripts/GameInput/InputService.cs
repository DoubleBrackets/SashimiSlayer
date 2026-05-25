using System;
using CommonTypes;
using Events.Basic;
using GameInput.Extra;
using GameInput.Haptics;
using GameInput.InputSources;
using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using UnityEngine.InputSystem;
using ValueSO;
using ValueSO.Core;

namespace GameInput
{
    public class InputService : MonoBehaviour, IUserInput, IValueSOObserver, IObserver<InputControl>
    {
        [Flags]
        private enum InputFlags
        {
            None = 0,
            MenuOpen = 1 << 0,
            CalibratingSword = 1 << 1
        }

        [Header("Event (In)")]

        [SerializeField]
        private VoidEvent _onDrawDebugGUI;

        [Header("Channel (In)")]

        [SerializeField]
        private BoolEvent _setUseSerialInput;

        [Header("ValueSO (Write)")]

        [SerializeField]
        private ControlSchemeValueSO _controlSchemeValueSO;

        [SerializeField]
        private SwordHandednessValueSO _currentHandednessValueSO;

        [Header("ValueSO (Read)")]

        [SerializeField]
        private FloatValueSO _aimMultiplierValueSO;

        [SerializeField]
        private FloatValueSO _aimOffsetValueSO;

        [SerializeField]
        private BoolValueSO _invertParryInputValueSO;

        [SerializeField]
        private SwordHandednessValueSO _swordHandednessValueSO;

        [SerializeField]
        private BoolValueSO _invertAimValueSO;

        [SerializeField]
        private BoolValueSO _controllerRumbleEnabledValueSO;

        [Header("Depends")]

        [SerializeField]
        private HidInputSource _hidInputSource;

        [SerializeField]
        private SerialSwordInputSource _serialInputSource;

        [SerializeField]
        private bool _useSerialController;

        [SerializeField]
        private SwordCalibrationPopup _swordCalibrationPopupPrefab;

        [Header("Config")]

        [Tooltip("Debounce time between sheathing and unsheathing slice to prevent bouncing")]
        [SerializeField]
        private float _sliceDebounce;

        private IUserInputSource InputProvider => _useSerialController ? _serialInputSource : _hidInputSource;

        public ControlSchemes ControlScheme => _controlSchemeValueSO.Value;

        public event Action<BlockPoses> OnBlockPoseChanged;
        public event Action<SheathState> OnSheathStateChanged;
        public event Action OnToggleMenuInput;
        public event Action OnToggleSkipLooping;

        private float _lastSheathedTime;

        private InputFlags _inputFlags;

        private SwordCalibrationPopup _swordCalibrationPopup;

        private void Awake()
        {
            EventPassthroughSub();

            _invertParryInputValueSO.AddListener(this, _ => SetInvertUINavigation());
            _swordHandednessValueSO.AddListener(this, _ => SetInvertUINavigation());

            _onDrawDebugGUI.AddListener(HandleDrawDebugGUI);
            _setUseSerialInput.AddListener(HandleSetUseSerialInput);

            InputSystem.onDeviceChange += HandleDeviceChange;

            InitSwordCalibrationPopup();

            _currentHandednessValueSO.SetValue(SwordHandedness.RightHandedSword);

            InputSystem.onAnyButtonPress.Subscribe(this);
        }

        private void Update()
        {
            // Not efficient but
            UpdateControlScheme();
        }

        private void OnDestroy()
        {
            EventPassthroughUnsub();

            _invertParryInputValueSO.RemoveListener(this);
            _swordHandednessValueSO.RemoveListener(this);

            _onDrawDebugGUI.RemoveListener(HandleDrawDebugGUI);
            _setUseSerialInput.RemoveListener(HandleSetUseSerialInput);

            InputSystem.onDeviceChange -= HandleDeviceChange;

            _controlSchemeValueSO.SetValue(ControlSchemes.KeyboardMouse);
            _currentHandednessValueSO.SetValue(SwordHandedness.RightHandedSword);
        }

        private void InitSwordCalibrationPopup()
        {
            _swordCalibrationPopup = Instantiate(_swordCalibrationPopupPrefab, transform);
            _swordCalibrationPopup.SetVisible(false);
        }

        public void SetMenuOpenFlag()
        {
            _inputFlags |= InputFlags.MenuOpen;
        }

        public void RemoveInputBlocker()
        {
            _inputFlags &= ~InputFlags.MenuOpen;
        }

        public void AddRumble(RumbleFeedbackSO rumbleFeedback)
        {
            if (!_controllerRumbleEnabledValueSO.Value)
            {
                return;
            }

            InputProvider.AddRumble(rumbleFeedback);
        }

        public bool GetCustomSwordControllerIdentify()
        {
            return InputProvider.GetCustomSwordControllerIdentify();
        }

        private void SetInvertUINavigation()
        {
            // invert on exclusive or
            bool invert = (_swordHandednessValueSO.Value == SwordHandedness.LeftHandedSword) ^
                          _invertParryInputValueSO.Value;
            _hidInputSource.SetInvertUINavigation(invert);
        }

        private void HandleSetUseSerialInput(bool useSerialInput)
        {
            Debug.Log($"Setting useHardwareController to {useSerialInput}");
            EventPassthroughUnsub();

            _useSerialController = useSerialInput;

            EventPassthroughSub();
        }

        private void HandleDeviceChange(InputDevice inputDevice, InputDeviceChange inputDeviceChange)
        {
            UpdateControlScheme();
        }

        private void UpdateControlScheme(InputDevice pressedInputDevice = null)
        {
            ControlSchemes existingControlScheme = ControlScheme;
            ControlSchemes newControlScheme = existingControlScheme;

            if (_useSerialController)
            {
                newControlScheme = ControlSchemes.SwordSerial;
            }
            else if (InputProvider.GetCustomSwordControllerIdentify())
            {
                newControlScheme = ControlSchemes.SwordJoystick;
            }
            else if (pressedInputDevice != null)
            {
                if (pressedInputDevice is Gamepad)
                {
                    newControlScheme = ControlSchemes.Gamepad;
                }
                else if (pressedInputDevice is Keyboard or Mouse)
                {
                    newControlScheme = ControlSchemes.KeyboardMouse;
                }
            }

            if (existingControlScheme != newControlScheme)
            {
                Debug.Log($"Control scheme changed from {existingControlScheme} to {newControlScheme}");
                _controlSchemeValueSO.SetValue(newControlScheme);

                if (newControlScheme == ControlSchemes.SwordJoystick)
                {
                    // Begin sword calibration popup
                    _swordCalibrationPopup.SetVisible(true);
                    _inputFlags |= InputFlags.CalibratingSword;
                }
                else
                {
                    _swordCalibrationPopup.SetVisible(false);
                    _inputFlags &= ~InputFlags.CalibratingSword;
                    _currentHandednessValueSO.SetValue(SwordHandedness.RightHandedSword);
                }
            }
        }

        private void HandleDrawDebugGUI()
        {
            GUILayout.Label($"control scheme: {ControlScheme}");
        }

        private void EventPassthroughSub()
        {
            InputProvider.OnBlockPoseChanged += HandleBlockPoseChanged;
            InputProvider.OnSheathStateChanged += HandleSheatheStateChanged;
            InputProvider.OnToggleMenuInput += HandleOnToggleMenuInput;
            InputProvider.OnToggleSkipLooping += HandleToggleSkipLooping;
        }

        private void EventPassthroughUnsub()
        {
            InputProvider.OnBlockPoseChanged -= HandleBlockPoseChanged;
            InputProvider.OnSheathStateChanged -= HandleSheatheStateChanged;
            InputProvider.OnToggleMenuInput -= HandleOnToggleMenuInput;
            InputProvider.OnToggleSkipLooping -= HandleToggleSkipLooping;
        }

        private void HandleOnToggleMenuInput()
        {
            OnToggleMenuInput?.Invoke();
        }

        private void HandleBlockPoseChanged(BlockPoses state)
        {
            if (BlockInput())
            {
                return;
            }

            if (_invertParryInputValueSO.Value)
            {
                state = state == BlockPoses.BlockShell ? BlockPoses.BlockStar : BlockPoses.BlockShell;
            }

            OnBlockPoseChanged?.Invoke(state);
        }

        private void HandleSheatheStateChanged(SheathState sheathState)
        {
            // Slice to confirm calibration results
            if (_inputFlags.HasFlag(InputFlags.CalibratingSword) && sheathState != SheathState.Sheathed)
            {
                _currentHandednessValueSO.SetValue(_swordCalibrationPopup.GetHandedness());
                _swordCalibrationPopup.SetVisible(false);
                _inputFlags &= ~InputFlags.CalibratingSword;
                return;
            }

            if (BlockInput())
            {
                return;
            }

            if (sheathState == SheathState.Unsheathed)
            {
                if (Time.time < _lastSheathedTime + _sliceDebounce)
                {
                    float timeSinceSheathed = Time.time - _lastSheathedTime;
                    Debug.LogWarning(
                        $"Unsheathing too soon after slicing, ignoring unsheathe request. {timeSinceSheathed:F2}s since last sheathed.");
                    return;
                }
            }

            if (sheathState == SheathState.Sheathed)
            {
                _lastSheathedTime = Time.time;
            }

            OnSheathStateChanged?.Invoke(sheathState);
        }

        private void HandleToggleSkipLooping()
        {
            OnToggleSkipLooping?.Invoke();
        }

        public float GetSwordAngle()
        {
            if (_inputFlags.HasFlag(InputFlags.CalibratingSword))
            {
                _swordCalibrationPopup.SetCurrentRawAngle(InputProvider.GetSwordAngle());
            }

            if (BlockInput())
            {
                // Ignore mouse aim when menus are open
                return 0;
            }

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

            int flippingFromInvertAim = _invertAimValueSO.Value ? -1 : 1;
            int flippingFromHandedness = _currentHandednessValueSO.Value == SwordHandedness.LeftHandedSword ? -1 : 1;

            return (rawSwordAngled + _aimOffsetValueSO.Value) * _aimMultiplierValueSO.Value *
                   flippingFromInvertAim *
                   flippingFromHandedness;
        }

        public SheathState GetSheathState()
        {
            return InputProvider.GetSheathState();
        }

        public BlockPoses GetBlockPose()
        {
            return InputProvider.GetBlockPose();
        }

        private bool BlockInput()
        {
            return _inputFlags != 0;
        }

        public void OnCompleted()
        {
        }

        public void OnError(Exception error)
        {
        }

        /// <summary>
        ///     Switch control schemes based on any pressed input's device
        /// </summary>
        /// <param name="value"></param>
        public void OnNext(InputControl value)
        {
            UpdateControlScheme(value.device);
        }
    }
}