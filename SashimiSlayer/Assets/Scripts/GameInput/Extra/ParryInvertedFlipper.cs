using System.Collections.Generic;
using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using UnityEngine.Serialization;
using ValueSO;
using ValueSO.Core;

namespace GameInput.Extra
{
    /// <summary>
    ///     Utility component that flips the direction of parry symbols based on input settings.
    /// </summary>
    public class ParryInvertedFlipper : MonoBehaviour, IValueSOObserver
    {
        [FormerlySerializedAs("_invertParryDirections")]
        [Header("ValueSO (Read)")]

        [SerializeField]
        private BoolValueSO _invertParrySymbols;

        [SerializeField]
        private SwordHandednessValueSO _currentHandednessValueSO;

        [Header("Depends")]

        [SerializeField]
        private List<SpriteRenderer> _sprites;

        [SerializeField]
        private List<ParticleSystemRenderer> _particles;

        [SerializeField]
        private List<Transform> _transforms;

        private void Awake()
        {
            _invertParrySymbols.AddListener(this, HandleInvertParry, true);
            _currentHandednessValueSO.AddListener(this, HandleHandednessChange, true);
        }

        private void OnDestroy()
        {
            _invertParrySymbols.RemoveListener(this);
            _currentHandednessValueSO.RemoveListener(this);
        }

        private void HandleInvertParry(bool b)
        {
            OnFlipParry();
        }

        private void HandleHandednessChange(SwordHandedness swordHandedness)
        {
            OnFlipParry();
        }

        private void OnFlipParry()
        {
            bool flip = _invertParrySymbols.Value ^
                        (_currentHandednessValueSO.Value == SwordHandedness.LeftHandedSword);

            foreach (SpriteRenderer sprite in _sprites)
            {
                if (sprite != null)
                {
                    sprite.flipX = flip;
                }
                else
                {
                    Debug.LogWarning("Unassigned SpriteRenderer in parry symbol flipper");
                }
            }

            int x = flip ? 1 : 0;

            foreach (ParticleSystemRenderer particle in _particles)
            {
                if (particle != null)
                {
                    Vector3 currentFlip = particle.flip;
                    currentFlip.x = x;
                    particle.flip = currentFlip;
                }
                else
                {
                    Debug.LogWarning("Unassigned ParticleSystem in parry symbol flipper");
                }
            }

            foreach (Transform tform in _transforms)
            {
                if (tform != null)
                {
                    Vector3 localScale = tform.localScale;
                    localScale.x = flip ? -1 : 1;
                    tform.localScale = localScale;
                }
                else
                {
                    Debug.LogWarning("Unassigned Transform in parry symbol flipper");
                }
            }
        }
    }
}