using System;
using System.Linq;
using CommonTypes;
using Events.Basic;
using GameInput.Haptics;
using GameInput.InputSources;
using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using UnityEngine.InputSystem;
using ValueSO.Core;

namespace GameInput
{
    public class InputService : MonoBehaviour, IUserInput
    {
        [Header("Event (In)")]

        [SerializeField]
        private VoidEvent _onDrawDebugGUI;

        [Header("Channel (In)")]

        [SerializeField]
        private BoolEvent _setUseSerialInput;

        [Header("ValueSO (Write)")]

        [SerializeField]
        private ControlSchemeValueSO _controlSchemeValueSO;

        [Header("ValueSO (Read)")]

        [SerializeField]
        private FloatValueSO _aimMultiplierValueSO;

        [SerializeField]
        private FloatValueSO _aimOffsetValueSO;

        [SerializeField]
        private BoolValueSO _invertParryDirectionValueSO;

        [SerializeField]
        private BoolValueSO _controllerRumbleEnabledValueSO;

        [Header("Depends")]

        [SerializeField]
        private HidInputSource _hidInputSource;

        [SerializeField]
        private SerialSwordInputSource _serialInputSource;

        [SerializeField]
        private bool _useSerialController;

        [Header("Config")]

        [Tooltip("Debounce time between sheathing and unsheathing slice to prevent bouncing")]
        [SerializeField]
        private float _sliceDebounce;

        private IUserInputSource InputProvider => _useSerialController ? _serialInputSource : _hidInputSource;

        public ControlSchemes ControlScheme { get; private set; }

        public bool FlipParryDirection { get; private set; }

        public event Action<BlockPoses> OnBlockPoseChanged;
        public event Action<SheathState> OnSheathStateChanged;
        public event Action OnToggleMenuInput;
        public event Action OnToggleSkipLooping;

        private float _lastSheathedTime;
        private bool IsInputBlocked => _inputBlockerCount > 0;

        /// <summary>
        ///     Counter for input blockers
        /// </summary>
        private int _inputBlockerCount;

        private void Awake()
        {
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

        public void AddInputBlocker()
        {
            _inputBlockerCount++;
        }

        public void RemoveInputBlocker()
        {
            _inputBlockerCount = Math.Max(0, _inputBlockerCount - 1);
        }

        public void AddRumble(RumbleFeedbackSO rumbleFeedback)
        {
            if (!_controllerRumbleEnabledValueSO.Value)
            {
                return;
            }

            InputProvider.AddRumble(rumbleFeedback);
        }

        private void SetInvertDirectionalBlockInputs(bool invert)
        {
            FlipParryDirection = invert;

            _hidInputSource.SetInvertParryDirection(invert);
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
            ControlSchemes existingControlScheme = ControlScheme;

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

            if (existingControlScheme != ControlScheme)
            {
                Debug.Log($"Control scheme changed from {existingControlScheme} to {ControlScheme}");
                _controlSchemeValueSO.SetValue(ControlScheme);
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
            if (IsInputBlocked)
            {
                return;
            }

            OnBlockPoseChanged?.Invoke(state);
        }

        private void HandleSheatheStateChanged(SheathState sheathState)
        {
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

            if (IsInputBlocked)
            {
                return;
            }

            OnSheathStateChanged?.Invoke(sheathState);
        }

        private void HandleToggleSkipLooping()
        {
            OnToggleSkipLooping?.Invoke();
        }

        public float GetSwordAngle()
        {
            if (IsInputBlocked)
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
            return (rawSwordAngled + _aimOffsetValueSO.Value) * _aimMultiplierValueSO.Value;
        }

        public SheathState GetSheathState()
        {
            return InputProvider.GetSheathState();
        }

        public BlockPoses GetBlockPose()
        {
            return InputProvider.GetBlockPose();
        }
    }
}