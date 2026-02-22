using System;
using FMOD;
using FMODUnity;
using UnityEngine;
using ADVANCEDSETTINGS = FMOD.Studio.ADVANCEDSETTINGS;
using Debug = UnityEngine.Debug;

namespace Core.Audio
{
    /// <summary>
    ///     Callback handler for FMOD to set up the FMOD system with the correct settings.
    /// </summary>
    [CreateAssetMenu(menuName = "FMOD Callback Handler")]
    public class FmodCallbackHandler : PlatformCallbackHandler
    {
        [SerializeField]
        private FmodConfigSo _standaloneConfig;

        [SerializeField]
        private FmodConfigSo _webGLConfig;

        public override void PreInitialize(FMOD.Studio.System studioSystem, Action<RESULT, string> reportResult)
        {
            FmodConfigSo usedConfig = _standaloneConfig;

            if (Application.platform == RuntimePlatform.WebGLPlayer && !Application.isEditor)
            {
                usedConfig = _webGLConfig;
            }

            FMOD.System coreSystem;
            RESULT result = studioSystem.getCoreSystem(out coreSystem);
            reportResult(result, "studioSystem.getCoreSystem");

            // Sample rate
            coreSystem.getSoftwareFormat(out int sampleRate, out _, out _);

            Debug.Log($"FMOD sample rate: {sampleRate}");

            // Buffer size
            coreSystem.getDSPBufferSize(out uint bufferLength, out int numBuffers);
            coreSystem.setDSPBufferSize(usedConfig.DspBufferLength, numBuffers);

            Debug.Log($"FMOD DSP buffer length set to {usedConfig.DspBufferLength}");

            // Update period
            studioSystem.getAdvancedSettings(out ADVANCEDSETTINGS settings);
            settings.studioupdateperiod = usedConfig.UpdatePeriodMs;
            studioSystem.setAdvancedSettings(settings);

            Debug.Log($"FMOD update period set to {usedConfig.UpdatePeriodMs}");
        }
    }
}