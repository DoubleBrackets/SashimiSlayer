using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ValueSO;
using ValueSO.Core;

namespace UI.Components.Slider
{
    /// <summary>
    ///     Labeled slider that allows editing of the value with only two navigation keys.
    ///     Can also load/save value from/to ValueSO.
    /// </summary>
    public class LabeledSlider : MonoBehaviour, IMoveHandler, IValueSOObserver
    {
        public enum Style
        {
            Value,
            Percentage,
            Degrees
        }

        [Header("UI")]

        [SerializeField]
        private TMP_Text _label;

        [SerializeField]
        private TMP_Text _editingLabel;

        [SerializeField]
        private UnityEngine.UI.Slider _slider;

        [SerializeField]
        private Style _style;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent _onFocused;

        [SerializeField]
        private UnityEvent _onUnfocused;

        [SerializeField]
        private UnityEvent _onValueMoved;

        [SerializeField]
        private int _increments;

        [Header("ValueSO (Read/Write)")]

        [SerializeField]
        private FloatValueSO _valueSO;

        private bool _locked;
        private Navigation _defaultNav;

        private void Awake()
        {
            if (_valueSO)
            {
                _valueSO.AddListener(this, OnValueSOChanged, true);
            }

            _slider.onValueChanged.AddListener(HandleValueChanged);
            HandleValueChanged(_slider.value);
        }

        private void OnDestroy()
        {
            _valueSO?.RemoveListener(this);
            _slider.onValueChanged.RemoveListener(HandleValueChanged);
        }

        private void OnEnable()
        {
            _editingLabel.gameObject.SetActive(false);
        }

        private void OnValueSOChanged(float value)
        {
            _slider.value = value;
        }

        public void HandleValueChanged(float value)
        {
            if (_style == Style.Percentage)
            {
                _label.text = $"{value * 100:F0}%";
                return;
            }

            if (_style == Style.Value)
            {
                _label.text = $"{value:F2}";
            }
            else if (_style == Style.Degrees)
            {
                _label.text = $"{value:F0}°";
            }

            if (_valueSO != null)
            {
                _valueSO.SetValue(value, this);
            }
        }

        /// <summary>
        ///     Lock the slider in on submission to allow editing with only two navigation keys
        /// </summary>
        public void ToggleFocused()
        {
            if (!_locked)
            {
                _defaultNav = _slider.navigation;
            }

            _locked = !_locked;

            _editingLabel.gameObject.SetActive(_locked);

            if (_locked)
            {
                _slider.navigation = new Navigation
                {
                    mode = Navigation.Mode.None
                };
            }
            else
            {
                _slider.navigation = _defaultNav;
            }
        }

        public void OnMove(AxisEventData eventData)
        {
            if (_locked)
            {
                float incrementValue = _slider.maxValue / _increments;
                _slider.value += incrementValue * Mathf.Sign(eventData.moveVector.x);
                _onValueMoved.Invoke();
            }
        }
    }
}