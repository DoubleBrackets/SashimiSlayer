using System;
using Core.Protag;
using Events;
using GameInput.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GameInput.InputSources
{
    public class HidInputSource : MonoBehaviour, IUserInputSource, GameplayControls.IGameplayActions
    {
        [Header("Events (In)")]

        [SerializeField]
        private FloatEvent _angleMultiplierEvent;

        [SerializeField]
        private FloatEvent _swordAngleOffsetEvent;

        [Header("Exhibition Hotkey Events")]

        [SerializeField]
        private UnityEvent _onExhibitionResetEvent;

        [SerializeField]
        private UnityEvent _OnExhibitionInvertAimEvent;

        [SerializeField]
        private UnityEvent _onExhibitionSkipLoopEvent;

        [SerializeField]
        private UnityEvent<bool> _onLeftHandSwordIdentifyEvent;

        public event Action<BlockPoseStates> OnBlockPoseChanged;
        public event Action<SheathState> OnSheathStateChanged;
        public event Action OnToggleMenuInput;

        private GameplayControls _gameplayControls;

        private Vector2 _mousePos;
        private BlockPoseStates _blockPoseStates;
        private SheathState _sheathState;

        private float _rawSwordAngle;

        // Tracks individual button state without dealing with menu overlays and such
        // Currently only used for menu toggling w/ button combo
        private bool _isLeftButtonDown;
        private bool _isRightButtonDown;

        private void OnEnable()
        {
            _gameplayControls = new GameplayControls();
            _gameplayControls.Enable();
            _gameplayControls.Gameplay.SetCallbacks(this);
            _blockPoseStates = 0;
        }

        private void OnDisable()
        {
            _gameplayControls.Disable();
        }

        public void OnSwordAngle(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _rawSwordAngle = JoyToAngle(context.ReadValue<Vector2>());
            }
        }

        public void OnUnsheathe(InputAction.CallbackContext context)
        {
            bool isPressed = context.ReadValueAsButton();

            SheathState newState = isPressed
                ? SheathState.Unsheathed
                : SheathState.Sheathed;

            // Toggle menu by pressing both buttons at the same time and slicing
            if (_isLeftButtonDown && _isRightButtonDown && newState == SheathState.Unsheathed)
            {
                OnToggleMenuInput?.Invoke();
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
                _rawSwordAngle = JoyToAngle(mouseDelta);
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
                _rawSwordAngle = JoyToAngle(context.ReadValue<Vector2>().normalized);
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
                _OnExhibitionInvertAimEvent?.Invoke();
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

        private float JoyToAngle(Vector2 joyVector)
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