using System.Collections.Generic;
using Events;
using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using ValueSO.Core;

namespace GameJuice.VFX
{
    /// <summary>
    ///     Plays a particle system(s) in response to events. Switches between two sets of particle systems based on a ValueSO.
    /// </summary>
    public class ParrySymbolsInvertedParticlePlayer : MonoBehaviour
    {
        [Header("ValueSO (Read)")]

        [SerializeField]
        private BoolValueSO _invertParrySymbols;

        [SerializeField]
        private SwordHandednessValueSO _currentHandednessValueSO;

        [Header("Event")]

        [SerializeField]
        private SOEvent _event;

        [SerializeField]
        private List<ParticleSystem> _defaultParticleSystems;

        [SerializeField]
        private List<ParticleSystem> _parryInvertedParticleSystems;

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
            bool flip = _invertParrySymbols.Value ^
                        (_currentHandednessValueSO.Value == SwordHandedness.LeftHandedSword);
            List<ParticleSystem> particleSystems =
                flip ? _parryInvertedParticleSystems : _defaultParticleSystems;
            if (particleSystems.Count == 0)
            {
                Debug.LogWarning("No particle systems selected!");
                return;
            }

            foreach (ParticleSystem particle in particleSystems)
            {
                if (particle != null)
                {
                    particle.Play();
                }
            }
        }
    }
}