using System.Collections.Generic;
using Base;
using UnityEngine;

namespace Feel
{
    public class OneshotVFX : DescMono
    {
        [SerializeField]
        private List<ParticleSystem> _particleSystems;

        [SerializeField]
        private float _lifetime;

        public IReadOnlyList<ParticleSystem> ParticleSystems => _particleSystems;

        public static void PlayVFX(OneshotVFX vfxPrefab, Vector3 position, Quaternion rotation)
        {
            OneshotVFX vfx = Instantiate(vfxPrefab, position, rotation);
            vfx.Play();
        }

        public void Play()
        {
            foreach (ParticleSystem particle in _particleSystems)
            {
                particle.Play();
            }

            Destroy(gameObject, _lifetime);
        }
    }
}