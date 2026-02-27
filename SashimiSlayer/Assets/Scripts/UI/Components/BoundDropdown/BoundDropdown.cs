using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using ValueSO;

namespace UI.Components.BoundDropdown
{
    public abstract class BoundDropdown<T> : MonoBehaviour, IValueSOObserver
    {
        [Header("ValueSO (Read/Write)")]

        [SerializeField]
        private ValueSO<T> _valueSO;

        [Header("UI")]

        [SerializeField]
        private TMP_Dropdown _dropdown;

        private void Awake()
        {
            _dropdown.options =
                new List<TMP_Dropdown.OptionData>(GetDropdownOptions().Select(x => new TMP_Dropdown.OptionData(x)));
            _valueSO.AddListener(this, HandleValueSOChanged, true);
            _dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private void OnDestroy()
        {
            _valueSO.RemoveListener(this);
            _dropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
        }

        private void OnDropdownValueChanged(int index)
        {
            _valueSO.SetValue(IndexToType(index), this);
        }

        private void HandleValueSOChanged(T value)
        {
            _dropdown.SetValueWithoutNotify(TypeToIndex(value));
        }

        protected abstract IList<string> GetDropdownOptions();

        protected abstract T IndexToType(int index);
        protected abstract int TypeToIndex(T value);
    }
}