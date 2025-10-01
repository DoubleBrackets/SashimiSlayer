using System.Collections.Generic;
using Events;
using GameInput;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Utility component that flips the direction of parry symbols based on input settings.
    /// </summary>
    public class ParrySymbolFlipper : MonoBehaviour
    {
        [Header("Events (In)")]

        [SerializeField]
        private BoolEvent _flipParryDirectionEvent;

        [Header("Depends")]

        [SerializeField]
        private List<SpriteRenderer> _sprites;

        [SerializeField]
        private List<ParticleSystemRenderer> _particles;

        [SerializeField]
        private List<Transform> _transforms;

        private void Awake()
        {
            if (InputService.Instance != null && InputService.Instance.FlipParryDirection)
            {
                OnFlipParry(true);
            }

            if (_flipParryDirectionEvent)
            {
                _flipParryDirectionEvent.AddListener(OnFlipParry);
            }
        }

        private void OnDestroy()
        {
            if (_flipParryDirectionEvent)
            {
                _flipParryDirectionEvent.RemoveListener(OnFlipParry);
            }
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