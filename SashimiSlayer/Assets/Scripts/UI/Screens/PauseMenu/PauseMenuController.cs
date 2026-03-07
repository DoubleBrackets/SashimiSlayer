using GameInput.Interface;
using GameJuice.ScreenShake;
using Saving;
using UI.MenuViews;
using UnityEngine;
using UnityEngine.EventSystems;
using ValueSO.Core;

namespace UI.Screens.PauseMenu
{
    /// <summary>
    ///     Pause menu top level controller
    /// </summary>
    public class PauseMenuController : MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private MenuViewController _pauseMenu;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [Header("ValueSO (Write)")]

        [SerializeField]
        private BoolValueSO _pauseMenuOpen;

        private bool _isMenuOpen = true;

        private IUserInput _userInput;
        private SaveService _saveService;
        private ScreenShakeService _screenShakeService;

        public void Initialize(IUserInput userInput, SaveService saveService, ScreenShakeService screenShakeService)
        {
            _userInput = userInput;
            _saveService = saveService;
            _screenShakeService = screenShakeService;

            foreach (MenuView view in _pauseMenu.Views)
            {
                if (view is PauseMenuView pauseMenuView)
                {
                    pauseMenuView.Initialize(_saveService, _screenShakeService);
                }
            }

            _pauseMenu.Initialize();
        }

        private void Start()
        {
            SetMenuOpen(false);
            _userInput.OnToggleMenuInput += HandleOnToggleMenuInput;
        }

        private void HandleOnToggleMenuInput()
        {
            SetMenuOpen(!_isMenuOpen);
        }

        public void SetMenuOpen(bool isOpen)
        {
            if (_isMenuOpen == isOpen)
            {
                return;
            }

            _isMenuOpen = isOpen;

            _canvasGroup.alpha = isOpen ? 1 : 0;

            if (_isMenuOpen)
            {
                _pauseMenu.ResetState();
            }

            if (!_isMenuOpen)
            {
                // Drop selection
                EventSystem.current.SetSelectedGameObject(null);
            }

            _pauseMenuOpen.SetValue(isOpen);

            if (isOpen)
            {
                _userInput.SetMenuOpenFlag();
            }
            else
            {
                _userInput.RemoveInputBlocker();
            }
        }

        private void OnDestroy()
        {
            _userInput.OnToggleMenuInput -= HandleOnToggleMenuInput;
        }
    }
}