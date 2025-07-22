using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.PauseMenu.Views
{
    public class SoundSettingsView : PauseMenuView
    {
        private const string SfxKey = "SfxVolume";
        private const string MusicKey = "MusicVolume";
        private const string MasterKey = "MasterVolume";

        [Header("Volume")]

        [SerializeField]
        private Slider _sfxVolumeSlider;

        [SerializeField]
        private Slider _musicVolumeSlider;

        [SerializeField]
        private Slider _masterVolumeSlider;

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

        public override void ViewAwake()
        {
            _musicBus = RuntimeManager.GetBus("bus:/Music");
            _sfxBus = RuntimeManager.GetBus("bus:/Sfx");
            _masterBus = RuntimeManager.GetBus("bus:/");

            _musicVolumeSlider.onValueChanged.AddListener(UpdateMusicVolume);
            _sfxVolumeSlider.onValueChanged.AddListener(UpdateSfxVolume);
            _masterVolumeSlider.onValueChanged.AddListener(UpdateMasterVolume);

            _sfxVolume = PlayerPrefs.GetFloat(SfxKey, 1);
            _musicVolume = PlayerPrefs.GetFloat(MusicKey, 1);
            _masterVolume = PlayerPrefs.GetFloat(MasterKey, 1);

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
            PlayerPrefs.SetFloat(MusicKey, volume);
            _musicBus.setVolume(volume);
        }

        private void UpdateSfxVolume(float volume)
        {
            _sfxVolume = volume;
            PlayerPrefs.SetFloat(SfxKey, volume);
            _sfxBus.setVolume(volume);
        }

        private void UpdateMasterVolume(float volume)
        {
            _masterVolume = volume;
            PlayerPrefs.SetFloat(MasterKey, volume);
            _masterBus.setVolume(volume);
        }
    }
}