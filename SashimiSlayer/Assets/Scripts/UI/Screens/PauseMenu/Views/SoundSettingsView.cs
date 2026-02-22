using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace UI.Screens.PauseMenu.Views
{
    public class SoundSettingsView : PauseMenuView
    {
        [Header("Volume")]

        [SerializeField]
        private UnityEngine.UI.Slider _sfxVolumeSlider;

        [SerializeField]
        private UnityEngine.UI.Slider _musicVolumeSlider;

        [SerializeField]
        private UnityEngine.UI.Slider _masterVolumeSlider;

        private Bus _musicBus;
        private Bus _sfxBus;
        private Bus _masterBus;

        private float _sfxVolume;
        private float _musicVolume;
        private float _masterVolume;

        public override void ViewDestroy()
        {
            _musicVolumeSlider.onValueChanged.RemoveListener(UpdateMusicVolume);
            _sfxVolumeSlider.onValueChanged.RemoveListener(UpdateSfxVolume);
            _masterVolumeSlider.onValueChanged.RemoveListener(UpdateMasterVolume);
        }

        public override void ViewInitialized()
        {
            _musicBus = RuntimeManager.GetBus("bus:/Music");
            _sfxBus = RuntimeManager.GetBus("bus:/Sfx");
            _masterBus = RuntimeManager.GetBus("bus:/");

            _musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
            _sfxVolumeSlider.onValueChanged.AddListener(UpdateSfxVolume);
            _masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);

            _sfxVolume = SaveService.SfxVolume;
            _musicVolume = SaveService.MusicVolume;
            _masterVolume = SaveService.MasterVolume;

            _sfxVolumeSlider.value = _sfxVolume;
            _musicVolumeSlider.value = _musicVolume;
            _masterVolumeSlider.value = _masterVolume;

            UpdateMusicVolume(_musicVolume);
            UpdateSfxVolume(_sfxVolume);
            UpdateMasterVolume(_masterVolume);
        }

        private void UpdateMusicVolume(float volume)
        {
            _sfxVolume = volume;
            SaveService.MusicVolume = volume;
            _musicBus.setVolume(volume);
        }

        private void UpdateSfxVolume(float volume)
        {
            _sfxVolume = volume;
            SaveService.SfxVolume = volume;
            _sfxBus.setVolume(volume);
        }

        private void UpdateMasterVolume(float volume)
        {
            _masterVolume = volume;
            SaveService.MasterVolume = volume;
            _masterBus.setVolume(volume);
        }
    }
}