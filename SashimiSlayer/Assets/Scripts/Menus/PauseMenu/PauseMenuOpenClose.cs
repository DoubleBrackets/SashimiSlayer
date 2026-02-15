using CommonTypes;
using Events;
using Framework;
using GameInput.Interface;
using UI.MenuViews;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Menus.PauseMenu
{
    /// <summary>
    ///     Handles opening and closing the pause menu
    /// </summary>
    public class PauseMenuOpenClose : MonoBehaviour, IFindsDependencies
    {
        [Header("Dependencies")]

        [SerializeField]
        private MenuViewController _pauseMenu;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [Header("Event (Out)")]

        [SerializeField]
        private BoolEvent _pauseMenuOpenEvent;

        private bool _isMenuOpen = true;
        private IUserInput _userInput;

        public void FindDependencies()
        {
            _userInput = ServiceLocator.GetUserInput();
        }

        private void Start()
        {
            FindDependencies();
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

            _pauseMenuOpenEvent?.Raise(isOpen);

            if (isOpen)
            {
                _userInput.AddInputBlocker();
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