using Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.PauseMenu.Views
{
    public class MiscSettingsView : PauseMenuView
    {
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
            FindDependencies();

            _screenShakeSlider.onValueChanged.AddListener(UpdateScreenShake);
            _screenShakeScale = SaveService.ScreenShakeRatio;
            _screenShakeSlider.value = _screenShakeScale;
        }

        public override void ViewStart()
        {
            UpdateScreenShake(_screenShakeScale);
        }

        private void UpdateScreenShake(float value)
        {
            _screenShakeScale = value;
            SaveService.ScreenShakeRatio = value;
            ScreenShakeService.Instance.ForceScale = value;
        }

        public void WipeHighScores()
        {
            SaveService.WipeHighScore();
        }
    }
}