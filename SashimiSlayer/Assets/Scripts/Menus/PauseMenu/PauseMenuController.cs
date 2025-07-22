using System;
using System.Collections.Generic;
using System.Linq;
using EditorUtils.BoldHeader;
using Events;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace Menus.PauseMenu
{
    public class PauseMenuController : MonoBehaviour
    {
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

                if (SelectionButton)
                {
                    SelectionButton.interactable = false;
                }
            }

            public void Hide()
            {
                if (View)
                {
                    View.Hide();
                }

                if (SelectionButton)
                {
                    SelectionButton.interactable = true;
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

        private bool _menuOpen;

        private ViewSelection _currentView;

        private void Awake()
        {
            ToggleMenu(false);

            foreach (ViewSelection view in _pauseMenuViews)
            {
                view.View.Hide();
                view.View.ViewAwake();
                view.SelectionButton.onClick.AddListener(() => SwitchView(view));
            }
        }

        private void Start()
        {
            foreach (ViewSelection view in _pauseMenuViews)
            {
                view.View.ViewStart();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Tab))
            {
                ToggleMenu(!_menuOpen);
            }
        }

        private void OnDestroy()
        {
            foreach (ViewSelection view in _pauseMenuViews)
            {
                view.View.ViewDestroy();
                view.SelectionButton.onClick.RemoveAllListeners();
            }
        }

        public void ToggleMenu(bool state)
        {
            _canvasGroup.SetEnabled(state);
            _menuOpen = state;
            _menuToggleEvent.Raise(state);

            if (_menuOpen)
            {
                SwitchView(_pauseMenuViews.First());
            }
            else
            {
                _currentView.Hide();
            }
        }

        public void SwitchView(ViewSelection view)
        {
            _currentView.Hide();
            view.Show();
            _currentView = view;
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

            SwitchView(_pauseMenuViews[index]);
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
    }
}