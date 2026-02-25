using UnityEngine;
using UnityEngine.UI;
using ValueSO;
using ValueSO.Core;

namespace UI.Components
{
    /// <summary>
    ///     Simple toggle binding to boolean valueSO
    /// </summary>
    public class BoundToggle : MonoBehaviour, IValueSOObserver
    {
        [Header("ValueSO (Read/Write)")]

        [SerializeField]
        private BoolValueSO _valueSO;

        [Header("UI")]

        [SerializeField]
        private Toggle _toggle;

        private void Awake()
        {
            _valueSO?.AddListener(this, OnValueSOChanged, true);
            _toggle.onValueChanged.AddListener(HandleToggleValueChanged);
        }

        private void OnValueSOChanged(bool value)
        {
            if (value == _toggle.isOn)
            {
                return;
            }

            _toggle.isOn = value;
        }

        private void OnDestroy()
        {
            _valueSO?.RemoveListener(this);
            _toggle.onValueChanged.RemoveListener(HandleToggleValueChanged);
        }

        private void HandleToggleValueChanged(bool value)
        {
            if (value == _valueSO.Value)
            {
                return;
            }

            _valueSO?.SetValue(value, this);
        }
    }
}