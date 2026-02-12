using CommonTypes;
using Cysharp.Threading.Tasks;
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

        [Header("Events (Out)")]

        [SerializeField]
        private BoolEvent _menuToggleEvent;

        private bool _isMenuOpen;
        private IUserInput _userInput;

        public void FindDependencies()
        {
            _userInput = ServiceLocator.GetUserInput();
        }

        private void Start()
        {
            FindDependencies();
            SetMenuOpenDelayed(false, false).Forget();
            _userInput.OnToggleMenuInput += HandleOnToggleMenuInput;
        }

        private void HandleOnToggleMenuInput()
        {
            SetMenuOpenDelayed(!_isMenuOpen).Forget();
        }

        public void SetMenuOpen(bool isOpen)
        {
            SetMenuOpenDelayed(isOpen).Forget();
        }

        /// <summary>
        ///     Delay to avoid menu-opening inputs causing navigation on entering
        /// </summary>
        /// <param name="isOpen"></param>
        /// <param name="delay"></param>
        private async UniTaskVoid SetMenuOpenDelayed(bool isOpen, bool delay = true)
        {
            // Additional delay to prevent slicing immediately after closing
            if (delay && !isOpen)
            {
                await UniTask.Yield();
            }

            _isMenuOpen = isOpen;
            _menuToggleEvent.Raise(_isMenuOpen);

            if (delay)
            {
                await UniTask.Yield();
            }

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
        }

        private void OnDestroy()
        {
            _userInput.OnToggleMenuInput -= HandleOnToggleMenuInput;
        }
    }
}