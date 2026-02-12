using System;
using Core.Protag;
using Events;
using GameInput.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GameInput.InputSources
{
    public class HidInputSource : MonoBehaviour, IUserInputSource
    {
        [Header("Events (In)")]

        [SerializeField]
        private FloatEvent _angleMultiplierEvent;

        [SerializeField]
        private FloatEvent _swordAngleOffsetEvent;

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

        [SerializeField]
        private InputActionReference _leftHandSwordIdentifyAction;

        [Header("Exhibit Hotkey Input Action References")]

        [SerializeField]
        private InputActionReference _exhibitionResetAction;

        [SerializeField]
        private InputActionReference _exhibitionInvertAimAction;

        [SerializeField]
        private InputActionReference _exhibitionSkipLoopAction;

        [Header("Exhibition Hotkey Events")]

        [SerializeField]
        private UnityEvent _onExhibitionResetEvent;

        [FormerlySerializedAs("_OnExhibitionInvertAimEvent")]
        [SerializeField]
        private UnityEvent _onExhibitionInvertAimEvent;

        [SerializeField]
        private UnityEvent _onExhibitionSkipLoopEvent;

        [SerializeField]
        private UnityEvent<bool> _onLeftHandSwordIdentifyEvent;

        public event Action<BlockPoseStates> OnBlockPoseChanged;
        public event Action<SheathState> OnSheathStateChanged;
        public event Action OnToggleMenuInput;

        private Vector2 _mousePos;
        private BlockPoseStates _blockPoseStates;
        private SheathState _sheathState;

        private float _rawSwordAngle;

        // Tracks individual button state without dealing with menu overlays and such
        // Currently only used for menu toggling w/ button combo
        private bool _isLeftButtonDown;
        private bool _isRightButtonDown;

        private void Awake()
        {
            _blockPoseStates = 0;
            _inputActionAsset.Enable();
            SubscribeToInputActions();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInputActions();
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
            _leftHandSwordIdentifyAction.action.performed += OnLeftHandSwordIdentify;

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
            _leftHandSwordIdentifyAction.action.performed -= OnLeftHandSwordIdentify;

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

        private void OnSheatheButton(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();

            SheathState newState = isPressed
                ? SheathState.Unsheathed
                : SheathState.Sheathed;

            // Toggle menu by pressing both buttons at the same time and slicing
            if (_isLeftButtonDown && _isRightButtonDown && newState == SheathState.Unsheathed)
            {
                OnToggleMenuInput?.Invoke();
                return;
            }

            if (_sheathState != newState)
            {
                _sheathState = newState;
                OnSheathStateChanged?.Invoke(_sheathState);
            }
        }

        public void OnBlockButtonLeft(InputAction.CallbackContext context)
        {
            _isLeftButtonDown = context.ReadValueAsButton();

            if (context.ReadValueAsButton())
            {
                OnBlockPoseChanged?.Invoke(BlockPoseStates.BlockLeft);
                _blockPoseStates = BlockPoseStates.BlockLeft;
            }
        }

        public void OnBlockButtonRight(InputAction.CallbackContext context)
        {
            _isRightButtonDown = context.ReadValueAsButton();

            if (context.ReadValueAsButton())
            {
                OnBlockPoseChanged?.Invoke(BlockPoseStates.BlockRight);
                _blockPoseStates = BlockPoseStates.BlockRight;
            }
        }

        public void OnMousePos(InputAction.CallbackContext context)
        {
            var newMousePos = context.ReadValue<Vector2>();
            if (newMousePos != _mousePos && Protaganist.Instance != null && Camera.main != null)
            {
                Vector2 screenCenter = Camera.main.WorldToScreenPoint(Protaganist.Instance.SwordPosition);
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

        public void OnLeftHandSwordIdentify(InputAction.CallbackContext context)
        {
            _onLeftHandSwordIdentifyEvent?.Invoke(context.ReadValueAsButton());
        }

        public void OnExhibitionReset(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _onExhibitionResetEvent?.Invoke();
            }
        }

        public void OnExhibitionInvertAim(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _onExhibitionInvertAimEvent?.Invoke();
            }
        }

        public void OnExhibitionSkipLoop(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _onExhibitionSkipLoopEvent?.Invoke();
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

        public BlockPoseStates GetBlockPose()
        {
            return _blockPoseStates;
        }
    }
}