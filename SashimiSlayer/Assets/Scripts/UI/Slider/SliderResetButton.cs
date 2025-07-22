using UnityEngine;
using Utility;

namespace UI.Slider
{
    public class SliderResetButton : MonoBehaviour
    {
        [SerializeField]
        private UnityEngine.UI.Slider _slider;

        [SerializeField]
        private float _resetValue;

        [SerializeField]
        private CanvasGroup _canvasGroup;

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
            _canvasGroup.SetEnabled(value != _resetValue);
        }
    }
}