using System;
using Core.Protag;
using Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace GameInput
{
    public class HidInputProvider : BaseUserInputProvider, GameplayControls.IGameplayActions
    {
        [Header("Events (In)")]

        [SerializeField]
        private FloatEvent _angleMultiplierEvent;

        [SerializeField]
        private FloatEvent _swordAngleOffsetEvent;

        [SerializeField]
        private BoolEvent _onMenuToggled;

        [Header("Exhibition Hotkey Events")]

        [SerializeField]
        private UnityEvent _onExhibitionResetEvent;

        [SerializeField]
        private UnityEvent _OnExhibitionInvertAimEvent;

        [SerializeField]
        private UnityEvent _onExhibitionSkipLoopEvent;

        [SerializeField]
        private UnityEvent<bool> _onLeftHandSwordIdentifyEvent;

        private bool IsMenuOverlayed => _overlayMenus > 0;

        public override event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;
        public override event Action<SharedTypes.SheathState> OnSheathStateChanged;

        private GameplayControls _gameplayControls;

        private Vector2 _mousePos;
        private SharedTypes.BlockPoseStates _blockPoseStates;
        private SharedTypes.SheathState _sheathState;

        private float _rawSwordAngle;

        /// <summary>
        ///     Disables input when menus are open
        /// </summary>
        private int _overlayMenus;

        private void Awake()
        {
            _onMenuToggled.AddListener(HandleMenuToggled);
        }

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

        private void OnDestroy()
        {
            _onMenuToggled.RemoveListener(HandleMenuToggled);
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
            if (IsMenuOverlayed)
            {
                return;
            }

            bool isPressed = context.ReadValueAsButton();

            SharedTypes.SheathState newState = isPressed
                ? SharedTypes.SheathState.Unsheathed
                : SharedTypes.SheathState.Sheathed;

            if (_sheathState != newState)
            {
                _sheathState = newState;
                OnSheathStateChanged?.Invoke(_sheathState);
            }
        }

        public void OnBlockButtonLeft(InputAction.CallbackContext context)
        {
            if (IsMenuOverlayed)
            {
                return;
            }

            if (context.ReadValueAsButton())
            {
                OnBlockPoseChanged?.Invoke(SharedTypes.BlockPoseStates.BlockLeft);
                _blockPoseStates = SharedTypes.BlockPoseStates.BlockLeft;
            }
        }

        public void OnBlockButtonRight(InputAction.CallbackContext context)
        {
            if (IsMenuOverlayed)
            {
                return;
            }

            if (context.ReadValueAsButton())
            {
                OnBlockPoseChanged?.Invoke(SharedTypes.BlockPoseStates.BlockRight);
                _blockPoseStates = SharedTypes.BlockPoseStates.BlockRight;
            }
        }

        public void OnMousePos(InputAction.CallbackContext context)
        {
            if (IsMenuOverlayed)
            {
                // Ignore mouse aim when menus are open
                return;
            }

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

        private void HandleMenuToggled(bool isMenuOpen)
        {
            Debug.Log("Menu toggled: " + isMenuOpen);
            _overlayMenus += isMenuOpen ? 1 : -1;
        }

        public override float GetSwordAngle()
        {
            return _rawSwordAngle;
        }

        private float JoyToAngle(Vector2 joyVector)
        {
            return Vector2.SignedAngle(Vector2.right, joyVector.normalized);
        }

        public override SharedTypes.SheathState GetSheathState()
        {
            return _sheathState;
        }

        public override SharedTypes.BlockPoseStates GetBlockPose()
        {
            return _blockPoseStates;
        }
    }
}