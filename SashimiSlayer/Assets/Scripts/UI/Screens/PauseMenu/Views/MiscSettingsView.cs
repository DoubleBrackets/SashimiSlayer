using UnityEngine;

namespace UI.Screens.PauseMenu.Views
{
    public class MiscSettingsView : PauseMenuView
    {
        [Header("Depends")]

        [SerializeField]
        private UnityEngine.UI.Slider _screenShakeSlider;

        private float _screenShakeScale;

        public override void ViewDestroy()
        {
            _screenShakeSlider.onValueChanged.RemoveListener(UpdateScreenShake);
        }

        public override void ViewInitialized()
        {
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
            ScreenShakeService.ForceScale = value;
        }

        public void WipeHighScores()
        {
            SaveService.WipeHighScore();
        }
    }
}