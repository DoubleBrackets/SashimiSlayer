using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Slider
{
    public class LabeledSlider : MonoBehaviour, IMoveHandler
    {
        public enum Style
        {
            Value,
            Percentage,
            Degrees
        }

        [SerializeField]
        private TMP_Text _label;

        [SerializeField]
        private TMP_Text _editingLabel;

        [SerializeField]
        private UnityEngine.UI.Slider _slider;

        [SerializeField]
        private Style _style;

        [SerializeField]
        private UnityEvent _onFocused;

        [SerializeField]
        private UnityEvent _onUnfocused;

        [SerializeField]
        private UnityEvent _onValueMoved;

        [SerializeField]
        private int _increments;

        private bool _locked;
        private Navigation _defaultNav;

        private void Awake()
        {
            _slider.onValueChanged.AddListener(SetValue);
            SetValue(_slider.value);
            _defaultNav = _slider.navigation;
        }

        private void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(SetValue);
        }

        private void OnEnable()
        {
            _editingLabel.gameObject.SetActive(false);
        }

        public void SetValue(float value)
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
        }

        /// <summary>
        ///     Lock the slider in on submission to allow editing with only two navigation keys
        /// </summary>
        public void ToggleFocused()
        {
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