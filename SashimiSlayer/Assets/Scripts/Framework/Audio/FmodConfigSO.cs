using UnityEngine;

namespace Core.Audio
{
    [CreateAssetMenu(fileName = "FmodConfig")]
    public class FmodConfigSo : ScriptableObject
    {
        [Tooltip("FMOD default is 1024")]
        public uint DspBufferLength;

        [Tooltip("FMOD default is 20")]
        public int UpdatePeriodMs;
    }
}