using System;
using System.Collections.Generic;
using CommonTypes;
using Events;
using Framework;
using GameInput.Haptics;
using Protag.SharedData;
using Saving;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace GameInput.InputSources
{
    public class HidInputSource : MonoBehaviour, IUserInputSource
    {
        [Header("Channels (In)")]

        [SerializeField]
        private SOEvent _onInputBindingOverride;

        [Header("Gameplay Input Action References")]

        [SerializeField]
        private InputActionAsset _inputActionAsset;

        [SerializeField]
        private InputActionReference _blockButtonLeftAction;

        [SerializeField]
        private InputActionReference _blockButtonRightAction;

        [SerializeField]
        private InputActionReference _swordAngleAction;

        [SerializeField]
        private InputActionReference _swordSheatheAction;

        [SerializeField]
        private InputActionReference _mousePosAction;

        [SerializeField]
        private InputActionReference _swordControllerAngleAction;

        [SerializeField]
        private InputActionReference _toggleMenuAction;

        [Header("Custom Sword Input Action References")]

        [SerializeField]
        private InputActionReference _customSwordControllerIdentifyAction;

        [Header("Exhibit Hotkey Input Action References")]

        [SerializeField]
        private InputActionReference _exhibitionResetAction;

        [SerializeField]
        private InputActionReference _exhibitionInvertAimAction;

        [SerializeField]
        private InputActionReference _exhibitionSkipLoopAction;

        [Header("UI Action References")]

        [SerializeField]
        private InputActionReference _uiNavigateAction;

        [SerializeField]
        private InputActionReference _uiNavigateActionInverted;

        public event Action<BlockPoses> OnBlockPoseChanged;
        public event Action<SheathState> OnSheathStateChanged;
        public event Action OnToggleMenuInput;
        public event Action OnToggleSkipLooping;

        private Vector2 _mousePos;
        private BlockPoses _blockPoses;
        private SheathState _sheathState;

        private float _rawSwordAngle;

        // Tracks individual button state without dealing with menu overlays and such
        // Currently only used for menu toggling w/ button combo
        private bool _isLeftButtonDown;
        private bool _isRightButtonDown;

        private SaveService _saveService;

        private bool _leftHandleSwordIdentify;
        private bool _customSwordControllerIdentify;

        private struct RumbleInstance
        {
            public RumbleProfile Profile;
            public float StartTime;
            public float EndTime;
        }

        private List<RumbleInstance> _lowFreqRumbleInstances = new();
        private List<RumbleInstance> _highFreqRumbleInstances = new();

        private float _currentLowFreqRumbleAmount;
        private float _currentHighFreqRumbleAmount;

        private void Start()
        {
            _saveService = ServiceLocator.GetService<SaveService>();

            LoadInputBindings();

            _blockPoses = 0;
            _inputActionAsset.Enable();
            SubscribeToInputActions();

            _onInputBindingOverride.AddListener(SaveInputBindings);
        }

        private void OnDestroy()
        {
            UnsubscribeFromInputActions();
            _onInputBindingOverride.RemoveListener(SaveInputBindings);

            Gamepad controller = Gamepad.current;

            if (controller != null)
            {
                controller.SetMotorSpeeds(0, 0);
            }
        }

        private void Update()
        {
            _currentLowFreqRumbleAmount = EvaluateRumble(_lowFreqRumbleInstances);
            _currentHighFreqRumbleAmount = EvaluateRumble(_highFreqRumbleInstances);

            Gamepad controller = Gamepad.current;

            if (controller != null)
            {
                controller.SetMotorSpeeds(_currentLowFreqRumbleAmount, _currentHighFreqRumbleAmount);
            }
        }

        private float EvaluateRumble(List<RumbleInstance> instances)
        {
            float totalRumble = 0;

            for (var i = 0; i < instances.Count; i++)
            {
                RumbleInstance instance = instances[i];
                if (Time.time >= instance.EndTime)
                {
                    instances.RemoveAt(i);
                    i--;
                }
                else
                {
                    totalRumble += instance.Profile.Evaluate(Time.time - instance.StartTime);
                }
            }

            return totalRumble;
        }

        private void SubscribeToInputActions()
        {
            // Gameplay Input Actions
            _swordAngleAction.action.performed += OnSwordAngle;
            _swordSheatheAction.action.performed += OnSheatheButton;
            _blockButtonLeftAction.action.performed += OnBlockButtonLeft;
            _blockButtonRightAction.action.performed += OnBlockButtonRight;
            _mousePosAction.action.performed += OnMousePos;
            _swordControllerAngleAction.action.performed += OnSwordControllerAngle;

            _toggleMenuAction.action.performed += OnToggleMenu;

            _customSwordControllerIdentifyAction.action.performed += OnCustomSwordControllerIdentify;
            _customSwordControllerIdentifyAction.action.canceled += OnCustomSwordControllerIdentify;

            // Exhibit Hotkey Input Actions
            _exhibitionResetAction.action.performed += OnExhibitionReset;
            _exhibitionInvertAimAction.action.performed += OnExhibitionInvertAim;
            _exhibitionSkipLoopAction.action.performed += OnExhibitionSkipLoop;
        }

        private void UnsubscribeFromInputActions()
        {
            // Gameplay Input Actions
            _swordAngleAction.action.performed -= OnSwordAngle;
            _swordSheatheAction.action.performed -= OnSheatheButton;
            _blockButtonLeftAction.action.performed -= OnBlockButtonLeft;
            _blockButtonRightAction.action.performed -= OnBlockButtonRight;
            _mousePosAction.action.performed -= OnMousePos;
            _swordControllerAngleAction.action.performed -= OnSwordControllerAngle;

            _toggleMenuAction.action.performed -= OnToggleMenu;

            _customSwordControllerIdentifyAction.action.performed -= OnCustomSwordControllerIdentify;
            _customSwordControllerIdentifyAction.action.canceled -= OnCustomSwordControllerIdentify;

            // Exhibit Hotkey Input Actions
            _exhibitionResetAction.action.performed -= OnExhibitionReset;
            _exhibitionInvertAimAction.action.performed -= OnExhibitionInvertAim;
            _exhibitionSkipLoopAction.action.performed -= OnExhibitionSkipLoop;
        }

        public void OnSwordAngle(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _rawSwordAngle = JoystickVectorToAngle(context.ReadValue<Vector2>());
            }
        }

        /// <summary>
        ///     Loads the input bindings from the save file
        /// </summary>
        /// <returns></returns>
        private void LoadInputBindings()
        {
            try
            {
                _inputActionAsset.LoadBindingOverridesFromJson(_saveService.InputBindingOverrides);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SaveInputBindings()
        {
            // awkward prettify
            _saveService.InputBindingOverrides =
                JsonUtility.ToJson(JsonUtility.FromJson<string>(_inputActionAsset.SaveBindingOverridesAsJson()), true);
        }

        private void OnSheatheButton(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();

            SheathState newSheathState = isPressed
                ? SheathState.Unsheathed
                : SheathState.Sheathed;

            // Toggle menu by pressing both buttons at the same time and slicing
            if (_isLeftButtonDown && _isRightButtonDown && newSheathState == SheathState.Unsheathed)
            {
                OnToggleMenuInput?.Invoke();
                return;
            }

            if (_sheathState != newSheathState)
            {
                _sheathState = newSheathState;
                OnSheathStateChanged?.Invoke(_sheathState);
            }
        }

        public void OnBlockButtonLeft(InputAction.CallbackContext context)
        {
            _isLeftButtonDown = context.ReadValueAsButton();

            if (context.ReadValueAsButton())
            {
                OnBlockPoseChanged?.Invoke(BlockPoses.BlockLeft);
                _blockPoses = BlockPoses.BlockLeft;
            }
        }

        public void OnBlockButtonRight(InputAction.CallbackContext context)
        {
            _isRightButtonDown = context.ReadValueAsButton();

            if (context.ReadValueAsButton())
            {
                OnBlockPoseChanged?.Invoke(BlockPoses.BlockRight);
                _blockPoses = BlockPoses.BlockRight;
            }
        }

        public void OnMousePos(InputAction.CallbackContext context)
        {
            var newMousePos = context.ReadValue<Vector2>();
            if (newMousePos != _mousePos && Camera.main != null)
            {
                Vector2 screenCenter =
                    Camera.main.WorldToScreenPoint(ProtagGlobalData.ProtagSwordPivot);
                Vector2 mouseDelta = newMousePos - screenCenter;
                _rawSwordAngle = JoystickVectorToAngle(mouseDelta);
            }

            _mousePos = newMousePos;
        }

        /// <summary>
        ///     Handles the angle from the hardware sword controller
        /// </summary>
        /// <param name="context"></param>
        public void OnSwordControllerAngle(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _rawSwordAngle = JoystickVectorToAngle(context.ReadValue<Vector2>().normalized);
            }
        }

        public void OnToggleMenu(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnToggleMenuInput?.Invoke();
            }
        }

        private void OnCustomSwordControllerIdentify(InputAction.CallbackContext context)
        {
            Debug.Log(context.ReadValueAsButton());
            _customSwordControllerIdentify = context.ReadValueAsButton();
        }

        public void OnExhibitionReset(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // TODO
            }
        }

        public void OnExhibitionInvertAim(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                // TODO
            }
        }

        public void OnExhibitionSkipLoop(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                OnToggleSkipLooping?.Invoke();
            }
        }

        public float GetSwordAngle()
        {
            return _rawSwordAngle;
        }

        /// <summary>
        ///     Converts a joystick vector to an angle
        /// </summary>
        /// <param name="joyVector"></param>
        /// <returns>angle in degrees</returns>
        private float JoystickVectorToAngle(Vector2 joyVector)
        {
            return Vector2.SignedAngle(Vector2.right, joyVector.normalized);
        }

        public SheathState GetSheathState()
        {
            return _sheathState;
        }

        public BlockPoses GetBlockPose()
        {
            return _blockPoses;
        }

        public void AddRumble(RumbleFeedbackSO rumbleFeedback)
        {
            RumbleProfile lowFreqRumble = rumbleFeedback.RumbleProfileLowFreq;
            RumbleProfile highFreqRumble = rumbleFeedback.RumbleProfileHighFreq;

            _lowFreqRumbleInstances.Add(new RumbleInstance
            {
                EndTime = Time.time + lowFreqRumble.Duration,
                Profile = lowFreqRumble,
                StartTime = Time.time
            });

            _highFreqRumbleInstances.Add(new RumbleInstance
            {
                EndTime = Time.time + highFreqRumble.Duration,
                Profile = highFreqRumble,
                StartTime = Time.time
            });
        }

        public bool GetCustomSwordControllerIdentify()
        {
            return _customSwordControllerIdentify;
        }

        /// <summary>
        ///     We handle flipping for gameplay elsewhere, so this is only needed for UI navigation since there's no processing
        ///     since there's no layers between the action and the UI input module
        /// </summary>
        /// <param name="invert"></param>
        public void SetInvertParryDirection(bool invert)
        {
            var uiModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();

            uiModule.move = invert ? _uiNavigateActionInverted : _uiNavigateAction;
        }
    }
}