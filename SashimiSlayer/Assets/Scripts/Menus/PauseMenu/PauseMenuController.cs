using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using EditorUtils.BoldHeader;
using Events;
using GameInput;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utility;

namespace Menus.PauseMenu
{
    public class PauseMenuController : MonoBehaviour
    {
        public enum PauseMenuState
        {
            Hidden,
            NavigatingSidebar,
            InsideView
        }

        [Serializable]
        public struct ViewSelection
        {
            public PauseMenuView View;
            public Button SelectionButton;

            public void Show()
            {
                if (View)
                {
                    View.Show();
                }
            }

            public void Hide()
            {
                if (View)
                {
                    View.Hide();
                }
            }
        }

        [BoldHeader("Pause Menu Controller")]
        [InfoBox("Controls pause menu views. First view in list is the default when opened")]
        [Header("Depend")]

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private List<ViewSelection> _pauseMenuViews;

        [Header("Events (Out)")]

        [SerializeField]
        private BoolEvent _menuToggleEvent;

        private bool _menuOpen = true;

        private ViewSelection _currentView;
        private PauseMenuState _state;

        private void Awake()
        {
            ToggleMenu(false);

            foreach (ViewSelection view in _pauseMenuViews)
            {
                view.View.Hide();
                view.View.ViewAwake();
                view.SelectionButton.onClick.AddListener(() => EnterView(view));
            }
        }

        private void Start()
        {
            InputService.Instance.OnToggleMenuInput += HandleOnToggleMenuInput;
            foreach (ViewSelection view in _pauseMenuViews)
            {
                view.View.ViewStart();
            }
        }

        private void HandleOnToggleMenuInput()
        {
            ToggleMenu(!_menuOpen);
        }

        private void OnDestroy()
        {
            InputService.Instance.OnToggleMenuInput -= HandleOnToggleMenuInput;
            foreach (ViewSelection view in _pauseMenuViews)
            {
                view.View.ViewDestroy();
                view.SelectionButton.onClick.RemoveAllListeners();
            }
        }

        public void ToggleMenu(bool open)
        {
            if (_menuOpen == open)
            {
                return;
            }

            _canvasGroup.SetEnabled(open);
            _menuOpen = open;

            if (open)
            {
                _menuToggleEvent.Raise(open);
                EnterView(_pauseMenuViews.First());
                SetPauseMenuState(PauseMenuState.InsideView);
            }
            else
            {
                _currentView.Hide();
                SetPauseMenuState(PauseMenuState.Hidden);
                MenuToggleEventDelayed().Forget();
            }
        }

        /// <summary>
        ///     Shitty hack; otherwise, the menu closing would also trigger a slice
        ///     This way the input occurs first, then the menu closes on the next frame, blocking the input
        /// </summary>
        private async UniTaskVoid MenuToggleEventDelayed()
        {
            await UniTask.DelayFrame(1);
            _menuToggleEvent.Raise(false);
        }

        public void EnterView(ViewSelection view)
        {
            _currentView.Hide();
            view.Show();
            _currentView = view;
            SetPauseMenuState(PauseMenuState.InsideView);
        }

        [Button("Next View")]
        public void NextView()
        {
            int index = _pauseMenuViews.IndexOf(_currentView);
            if (index == -1)
            {
                index = 0;
            }

            index = (index + 1) % _pauseMenuViews.Count;

            EnterView(_pauseMenuViews[index]);
        }

        [Button("Find Child Views")]
        public void FindChildViews()
        {
            _pauseMenuViews = GetComponentsInChildren<PauseMenuView>().Select(a => new ViewSelection
            {
                View = a,
                SelectionButton = null
            }).ToList();
        }

        public void SetPauseMenuState(PauseMenuState state)
        {
            Debug.Log($"Setting pause menu state to {state}");
            if (_state == state)
            {
                return;
            }

            _state = state;

            if (_state == PauseMenuState.NavigatingSidebar)
            {
                foreach (ViewSelection view in _pauseMenuViews)
                {
                    view.SelectionButton.interactable = true;
                }

                EventSystem.current.SetSelectedGameObject(_currentView.SelectionButton.gameObject);

                _currentView.View.SetInteractable(false);
            }
            else
            {
                _currentView.View.EnterSelection();

                foreach (ViewSelection view in _pauseMenuViews)
                {
                    view.SelectionButton.interactable = true;
                }

                _currentView.SelectionButton.interactable = false;

                _currentView.View.SetInteractable(true);
            }
        }

        public void EnterSidebar()
        {
            SetPauseMenuState(PauseMenuState.NavigatingSidebar);
        }
    }
}