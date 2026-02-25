using System.Collections.Generic;
using UnityEngine;
using ValueSO;
using ValueSO.Core;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Utility component that flips the direction of parry symbols based on input settings.
    /// </summary>
    public class ParryInvertedFlipper : MonoBehaviour, IValueSOObserver
    {
        [Header("ValueSO (Read)")]

        [SerializeField]
        private BoolValueSO _invertParryDirections;

        [Header("Depends")]

        [SerializeField]
        private List<SpriteRenderer> _sprites;

        [SerializeField]
        private List<ParticleSystemRenderer> _particles;

        [SerializeField]
        private List<Transform> _transforms;

        private void Awake()
        {
            _invertParryDirections.AddListener(this, OnFlipParry, true);
        }

        private void OnDestroy()
        {
            _invertParryDirections.RemoveListener(this);
        }

        private void OnFlipParry(bool flip)
        {
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