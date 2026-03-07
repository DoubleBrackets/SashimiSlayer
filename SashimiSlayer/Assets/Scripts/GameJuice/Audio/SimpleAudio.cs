using FMODUnity;
using UnityEngine;

namespace GameJuice.Audio
{
    /// <summary>
    ///     Exposes an audio clip to be played. Intended to be used with UnityEvents.
    /// </summary>
    public class SimpleAudio : MonoBehaviour
    {
        [SerializeField]
        private EventReference _clip;

        public void Play()
        {
            if (!_clip.IsNull)
            {
                RuntimeManager.PlayOneShot(_clip);
            }
        }
    }
}