using System.Collections.Generic;
using System.Linq;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace UI.MenuViews
{
    /// <summary>
    ///     UI Menu layout where a sequence of "buttons" each mapped to a "views",
    ///     where the user can see a view by selecting the button, and enter the view by submitting
    /// </summary>
    public class MenuViewController : MonoBehaviour
    {
        private enum PauseMenuState
        {
            /// <summary>
            ///     Navigating through the button sequence
            /// </summary>
            NavigatingSidebar,
            InsideView
        }

        [BoldHeader("Menu Controller")]
        [InfoBox("Controls a menu view. First view in list is the default when opened")]
        [Header("Depend")]

        [SerializeField]
        private List<MenuView> _menuViews;

        [Header("Selection Buttons")]

        [SerializeField]
        private MenuViewButton _buttonPrefab;

        [SerializeField]
        private Transform _buttonParent;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onInitialized;

        private MenuView _currentView;

        private MenuViewButton _currentSelectedButton;

        private PauseMenuState _state = PauseMenuState.NavigatingSidebar;
        private List<MenuViewButton> _buttons = new();
        public IList<MenuView> Views => _menuViews;

        public void Initialize()
        {
            foreach (MenuView view in _menuViews)
            {
                view.Hide();
                view.ViewInitialized();

                // Instantiate button for the view
                MenuViewButton button = Instantiate(_buttonPrefab, _buttonParent);
                button.Initialize(view);
                _buttons.Add(button);

                button.OnMenuViewEntered += HandleOnViewEntered;
                button.OnMenuViewSelected += HandleOnViewSelected;
            }

            ResetState();

            _onInitialized?.Invoke();
        }

        private void Start()
        {
            foreach (MenuView view in _menuViews)
            {
                view.ViewStart();
            }
        }

        private void OnDestroy()
        {
            foreach (MenuView view in _menuViews)
            {
                view.ViewDestroy();
            }

            foreach (MenuViewButton button in _buttons)
            {
                button.OnMenuViewEntered -= HandleOnViewEntered;
                button.OnMenuViewSelected -= HandleOnViewSelected;
            }
        }

        private void HandleOnViewSelected(MenuViewButton menuViewButton, MenuView menuView)
        {
            if (_currentSelectedButton != null)
            {
                _currentSelectedButton.IsSelected = false;
            }

            _currentSelectedButton = menuViewButton;
            _currentSelectedButton.IsSelected = true;

            SelectView(menuView);
        }

        private void HandleOnViewEntered(MenuViewButton menuViewButton, MenuView menuView)
        {
            // the current view should always match with the selecting button
            EnterCurrentView();
        }

        /// <summary>
        ///     Reset to default state, where the first view is entered
        /// </summary>
        public void ResetState()
        {
            _state = PauseMenuState.NavigatingSidebar;
            _currentSelectedButton = _buttons.First();
            SelectView(_menuViews.First());
            EnterCurrentView();
        }

        /// <summary>
        ///     Select a view to show and hide others
        /// </summary>
        /// <param name="view"></param>
        private void SelectView(MenuView view)
        {
            if (_state != PauseMenuState.NavigatingSidebar)
            {
                return;
            }

            foreach (MenuView otherViews in _menuViews)
            {
                otherViews.Hide();
            }

            view.Show();
            _currentView = view;
        }

        /// <summary>
        ///     Enter the currently selected view and exit navigation mode
        /// </summary>
        private void EnterCurrentView()
        {
            if (_state != PauseMenuState.NavigatingSidebar)
            {
                return;
            }

            _currentView.GrabFocus();

            foreach (MenuViewButton button in _buttons)
            {
                button.IsInNavigationMode = false;
            }

            _state = PauseMenuState.InsideView;
        }

        /// <summary>
        ///     Exit view and return to navigation mode
        /// </summary>
        public void ReturnToNavigation()
        {
            if (_state != PauseMenuState.InsideView)
            {
                return;
            }

            foreach (MenuViewButton button in _buttons)
            {
                button.IsInNavigationMode = true;
            }

            _currentSelectedButton.GrabFocus();

            _state = PauseMenuState.NavigatingSidebar;
        }

        /// <summary>
        ///     Editor utility
        /// </summary>
        [Button("Next View")]
        public void NextView()
        {
            int index = _menuViews.IndexOf(_currentView);
            if (index == -1)
            {
                index = 0;
            }

            index = (index + 1) % _menuViews.Count;

            SelectView(_menuViews[index]);
        }

        [Button("Find Child Views")]
        public void FindChildViews()
        {
            _menuViews = GetComponentsInChildren<MenuView>().ToList();
        }
    }
}