using Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.PauseMenu.Views
{
    public class MiscSettingsView : PauseMenuView
    {
        private const string ScreenShakeKey = "ScreenShake";

        [Header("Depends")]

        [SerializeField]
        private Slider _screenShakeSlider;

        private float _screenShakeScale;

        public override void ViewDestroy()
        {
            _screenShakeSlider.onValueChanged.RemoveListener(UpdateScreenShake);
        }

        public override void ViewAwake()
        {
            _screenShakeSlider.onValueChanged.AddListener(UpdateScreenShake);
            _screenShakeScale = PlayerPrefs.GetFloat(ScreenShakeKey, 1);
            _screenShakeSlider.value = _screenShakeScale;
        }

        public override void ViewStart()
        {
            UpdateScreenShake(_screenShakeScale);
        }

        private void UpdateScreenShake(float value)
        {
            _screenShakeScale = value;
            PlayerPrefs.SetFloat(ScreenShakeKey, value);
            ScreenShakeService.Instance.ForceScale = value;
        }
    }
}