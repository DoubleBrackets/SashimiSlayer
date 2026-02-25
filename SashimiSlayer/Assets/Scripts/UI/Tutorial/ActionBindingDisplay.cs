using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.Tutorial
{
    /// <summary>
    ///     Displays the current binding name for a specified input action and binding index.
    ///     Automatically updates when bindings change.
    /// </summary>
    public class ActionBindingDisplay : MonoBehaviour
    {
        [Header("Config")]

        [SerializeField]
        private InputActionReference _actionRef;

        [SerializeField]
        private int _bindingIndex;

        [SerializeField]
        private InputBinding.DisplayStringOptions _displayStringOptions;

        [Header("UI References")]

        [SerializeField]
        private TMP_Text _bindingText;

        private void Awake()
        {
            InputSystem.onActionChange += OnActionChange;
            UpdateBindingText();
        }

        private void OnDestroy()
        {
            InputSystem.onActionChange -= OnActionChange;
        }

        private void OnValidate()
        {
            UpdateBindingText();
        }

        private void UpdateBindingText()
        {
            var displayString = string.Empty;
            var withoutDevice = string.Empty;

            InputAction action = _actionRef?.action;
            if (action != null && _bindingIndex >= 0 && _bindingIndex < action.bindings.Count)
            {
                displayString = action.GetBindingDisplayString(
                    _bindingIndex,
                    out string deviceLayoutName,
                    out string controlPath,
                    _displayStringOptions);

                withoutDevice = action.GetBindingDisplayString(
                    _bindingIndex,
                    out deviceLayoutName,
                    out controlPath,
                    _displayStringOptions & ~InputBinding.DisplayStringOptions.DontOmitDevice);
            }

            if (_bindingText != null)
            {
                if (_displayStringOptions.HasFlag(InputBinding.DisplayStringOptions.DontOmitDevice))
                {
                    displayString = displayString.Replace(withoutDevice, withoutDevice + " /");
                }

                _bindingText.text = displayString;
            }
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
            {
                return;
            }

            UpdateBindingText();
        }
    }
}