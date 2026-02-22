using Events;
using UnityEngine;

namespace GameJuice.VFX
{
    /// <summary>
    ///     Plays or stops a particle system in response to events.
    /// </summary>
    public class EventParticleToggler : MonoBehaviour
    {
        [Header("Event")]

        [SerializeField]
        private SOEvent _startEvent;

        [SerializeField]
        private SOEvent _stopEvent;

        [SerializeField]
        private ParticleSystem _particleSystem;

        private void Awake()
        {
            _startEvent.AddListener(PlayParticle);
            _stopEvent.AddListener(StopParticle);
        }

        private void OnDestroy()
        {
            _startEvent.RemoveListener(PlayParticle);
            _stopEvent.RemoveListener(StopParticle);
        }

        private void PlayParticle()
        {
            _particleSystem.Play();
        }

        private void StopParticle()
        {
            _particleSystem.Stop();
        }
    }
}