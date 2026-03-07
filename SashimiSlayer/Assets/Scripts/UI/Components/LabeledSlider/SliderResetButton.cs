using UnityEngine;

namespace UI.Components.Slider
{
    /// <summary>
    ///     Resets a slider to a given value when clicked
    /// </summary>
    public class SliderResetButton : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Slider _slider;

        [SerializeField]
        private float _resetValue;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private float _disabledAlpha;

        private void Awake()
        {
            _slider.onValueChanged.AddListener(OnSliderValueChanged);
            OnSliderValueChanged(_slider.value);
        }

        private void OnDestroy()
        {
            _slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }

        public void ResetSlider()
        {
            _slider.value = _resetValue;
        }

        private void OnSliderValueChanged(float value)
        {
            _canvasGroup.alpha = Mathf.Approximately(value, _resetValue) ? _disabledAlpha : 1;
        }
    }
}