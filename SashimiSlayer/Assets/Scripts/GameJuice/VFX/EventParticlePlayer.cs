using System.Collections.Generic;
using Events;
using UnityEngine;

namespace GameJuice.VFX
{
    /// <summary>
    ///     Plays a particle system(s) in response to events.
    /// </summary>
    public class EventParticlePlayer : MonoBehaviour
    {
        [Header("Event")]

        [SerializeField]
        private SOEvent _event;

        [SerializeField]
        private List<ParticleSystem> _particleSystems;

        private void Awake()
        {
            _event.AddListener(PlayParticle);
        }

        private void OnDestroy()
        {
            _event.RemoveListener(PlayParticle);
        }

        private void PlayParticle()
        {
            if (_particleSystems.Count == 0)
            {
                Debug.LogWarning("No particle systems selected!");
                return;
            }

            foreach (ParticleSystem particle in _particleSystems)
            {
                if (particle != null)
                {
                    particle.Play();
                }
            }
        }

        [ContextMenu("Quickselect Child ParticleSystems")]
        private void AutodetectChildren()
        {
            _particleSystems.Clear();
            foreach (ParticleSystem particle in GetComponentsInChildren<ParticleSystem>())
            {
                _particleSystems.Add(particle);
            }
        }
    }
}