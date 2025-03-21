using Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.Options
{
    public class EffectsOption : MonoBehaviour
    {
        private const string ScreenShakeKey = "ScreenShake";

        [Header("Effects")]

        [SerializeField]
        private Slider _screenShakeSlider;

        private float _screenShakeScale;

        private void Awake()
        {
            _screenShakeSlider.onValueChanged.AddListener(UpdateScreenShake);
            _screenShakeScale = PlayerPrefs.GetFloat(ScreenShakeKey, 1);
            _screenShakeSlider.value = _screenShakeScale;
        }

        private void Start()
        {
            UpdateScreenShake(_screenShakeScale);
        }

        private void OnDestroy()
        {
            _screenShakeSlider.onValueChanged.RemoveListener(UpdateScreenShake);
        }

        private void UpdateScreenShake(float value)
        {
            _screenShakeScale = value;
            PlayerPrefs.SetFloat(ScreenShakeKey, value);
            ScreenShakeService.Instance.ForceScale = value;
        }
    }
}