using Events;
using FMODUnity;
using UnityEngine;

namespace GameJuice.Audio
{
    /// <summary>
    ///     Plays a oneshot audio clip in response to an event.
    /// </summary>
    public class EventAudioPlayer : MonoBehaviour
    {
        [SerializeField]
        private EventReference _sfx;

        [SerializeField]
        private SOEvent _event;

        private void Awake()
        {
            _event.AddListener(PlayAudio);
        }

        private void OnDestroy()
        {
            _event.RemoveListener(PlayAudio);
        }

        private void PlayAudio()
        {
            RuntimeManager.PlayOneShot(_sfx);
        }
    }
}