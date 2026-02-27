using UnityEngine;
using ValueSO;
using ValueSO.Core;

namespace Saving
{
    /// <summary>
    ///     Handles initial loading data from save model into ValueSOs, and writing changes back to save model
    /// </summary>
    [CreateAssetMenu(fileName = "SaveDataValueSOLoader", menuName = "SaveDataValueSOLoader")]
    public class SaveDataValueSOLoader : ScriptableObject, IValueSOObserver
    {
        [Header("ValueSO (Read/Write)")]

        [Header("Audio Settings")]

        [SerializeField]
        private FloatValueSO _sfxVolume;

        [SerializeField]
        private FloatValueSO _musicVolume;

        [SerializeField]
        private FloatValueSO _masterVolume;

        [Header("Misc Settings")]

        [SerializeField]
        private FloatValueSO _screenShakeRatioValueSO;

        [Header("Input Settings")]

        [SerializeField]
        private FloatValueSO _aimMultiplierValueSO;

        [SerializeField]
        private FloatValueSO _aimOffsetValueSO;

        [SerializeField]
        private BoolValueSO _controllerRumbleEnabledValueSO;

        [SerializeField]
        private BoolValueSO _invertAimSO;

        [SerializeField]
        private BoolValueSO _invertParryInputSO;

        [SerializeField]
        private BoolValueSO _invertParrySymbolsSO;

        /// <summary>
        ///     Setup ValueSOs to write changes back to save model
        /// </summary>
        /// <param name="saveService"></param>
        public void InitializeSaveBack(SaveService saveService)
        {
            _sfxVolume.AddListener(this, value => saveService.SfxVolume = value);
            _musicVolume.AddListener(this, value => saveService.MusicVolume = value);
            _masterVolume.AddListener(this, value => saveService.MasterVolume = value);

            _screenShakeRatioValueSO.AddListener(this, value => saveService.ScreenShakeRatio = value);

            _aimMultiplierValueSO.AddListener(this, value => saveService.SwordAimMultiplier = value);
            _aimOffsetValueSO.AddListener(this, value => saveService.SwordAngleOffset = value);
            _controllerRumbleEnabledValueSO.AddListener(this, value => saveService.RumbleFeedbackEnabled = value);
            _invertAimSO.AddListener(this, value => saveService.InvertSwordAim = value);
            _invertParryInputSO.AddListener(this, value => saveService.InvertParryDirection = value);
            _invertParrySymbolsSO.AddListener(this, value => saveService.InvertParrySymbols = value);
        }

        public void Cleanup()
        {
            _sfxVolume.RemoveListener(this);
            _musicVolume.RemoveListener(this);
            _masterVolume.RemoveListener(this);

            _screenShakeRatioValueSO.RemoveListener(this);

            _aimMultiplierValueSO.RemoveListener(this);
            _aimOffsetValueSO.RemoveListener(this);
            _controllerRumbleEnabledValueSO.RemoveListener(this);
            _invertAimSO.RemoveListener(this);
            _invertParryInputSO.RemoveListener(this);
            _invertParrySymbolsSO.RemoveListener(this);
        }

        public void Load(SaveModel saveModel)
        {
            _sfxVolume.SetValue(saveModel.SfxVolume, this);
            _musicVolume.SetValue(saveModel.MusicVolume, this);
            _masterVolume.SetValue(saveModel.MasterVolume, this);

            _screenShakeRatioValueSO.SetValue(saveModel.ScreenShakeRatio, this);

            _aimMultiplierValueSO.SetValue(saveModel.SwordAimMultiplier, this);
            _aimOffsetValueSO.SetValue(saveModel.SwordAngleOffset, this);
            _controllerRumbleEnabledValueSO.SetValue(saveModel.RumbleFeedbackEnabled, this);
            _invertAimSO.SetValue(saveModel.InvertSwordAim, this);
            _invertParryInputSO.SetValue(saveModel.InvertParryDirection, this);
            _invertParrySymbolsSO.SetValue(saveModel.InvertParrySymbols, this);
        }
    }
}