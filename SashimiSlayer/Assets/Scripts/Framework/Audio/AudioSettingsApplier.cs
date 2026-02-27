using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using ValueSO;
using ValueSO.Core;

namespace Framework.Audio
{
    /// <summary>
    ///     Handles reading audio settings from ValueSOs and applying them to FMOD
    /// </summary>
    public class AudioSettingsApplier : MonoBehaviour, IValueSOObserver
    {
        [InfoBox("Handles reading audio settings from ValueSOs and applying them to FMOD")]
        [Header("ValueSO (Read)")]

        [SerializeField]
        private FloatValueSO _sfxVolume;

        [SerializeField]
        private FloatValueSO _musicVolume;

        [SerializeField]
        private FloatValueSO _masterVolume;

        private Bus _musicBus;
        private Bus _sfxBus;
        private Bus _masterBus;

        private void Awake()
        {
            _musicBus = RuntimeManager.GetBus("bus:/Music");
            _sfxBus = RuntimeManager.GetBus("bus:/Sfx");
            _masterBus = RuntimeManager.GetBus("bus:/");

            _sfxVolume.AddListener(this, UpdateSfxVolume, true);
            _musicVolume.AddListener(this, UpdateMusicVolume, true);
            _masterVolume.AddListener(this, UpdateMasterVolume, true);
        }

        private void OnDestroy()
        {
            _sfxVolume.RemoveListener(this);
            _musicVolume.RemoveListener(this);
            _masterVolume.RemoveListener(this);
        }

        private void UpdateMasterVolume(float value)
        {
            _masterBus.setVolume(value);
        }

        private void UpdateMusicVolume(float value)
        {
            _musicBus.setVolume(value);
        }

        private void UpdateSfxVolume(float value)
        {
            _sfxBus.setVolume(value);
        }
    }
}