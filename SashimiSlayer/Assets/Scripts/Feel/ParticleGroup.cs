using System.Collections.Generic;
using UnityEngine;

namespace Feel
{
    /// <summary>
    ///     Utility class for grouping particle systems together
    /// </summary>
    public class ParticleGroup : MonoBehaviour
    {
        [SerializeField]
        private List<ParticleSystem> _particleSystems;

        public IReadOnlyList<ParticleSystem> ParticleSystems => _particleSystems;

        public void PlayAll()
        {
            foreach (ParticleSystem particle in _particleSystems)
            {
                if (particle != null)
                {
                    particle.Play();
                }
            }
        }

        public void StopAll()
        {
            foreach (ParticleSystem particle in _particleSystems)
            {
                if (particle != null)
                {
                    particle.Stop();
                }
            }
        }
    }
}