using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.MenuViews
{
    public class MenuViewButton : MonoBehaviour
    {
        [SerializeField]
        private Button _button;

        [SerializeField]
        private TMP_Text _label;

        /// <summary>
        ///     The menu view this button is mapped to
        /// </summary>
        private MenuView _menuView;

        public event Action<MenuViewButton, MenuView> OnMenuViewSelected;
        public event Action<MenuViewButton, MenuView> OnMenuViewEntered;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                UpdateInteractable();
            }
        }

        public bool IsInNavigationMode
        {
            get => _isInNavigationMode;
            set
            {
                _isInNavigationMode = value;
                UpdateInteractable();
            }
        }

        private void UpdateInteractable()
        {
            _button.interactable = _isInNavigationMode || !IsSelected;
        }

        private bool _isSelected;
        private bool _isInNavigationMode;

        public void Initialize(MenuView menuView)
        {
            _menuView = menuView;
            _label.text = menuView.ViewName;
            gameObject.name = $"ViewButton {menuView.ViewName}";
            _button.onClick.AddListener(OnSubmit);
        }

        private void OnDestroy()
        {
            _button.onClick.RemoveListener(OnSubmit);
        }

        /// <summary>
        ///     Called by EventTrigger component, wired up in editor since button does not expose OnSelect event
        /// </summary>
        public void OnSelect()
        {
            OnMenuViewSelected?.Invoke(this, _menuView);
        }

        public void OnSubmit()
        {
            OnMenuViewEntered?.Invoke(this, _menuView);
        }

        public void GrabFocus()
        {
            EventSystem.current.SetSelectedGameObject(_button.gameObject);
        }
    }
}