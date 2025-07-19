using System.Collections.Generic;
using Feel.Notes;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.NoteBehaviors.Visuals
{
    /// <summary>
    ///     Interface for a single instance of a note visual object
    /// </summary>
    public class NoteVisualObject : MonoBehaviour
    {
        [SerializeField]
        private ParticleSystem _hitParticle;

        [SerializeField]
        private ParticleSystem _trailParticle;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private List<BeatAnimatedSprite> _animatedSprites;

        [SerializeField]
        private bool _playFirstAnimationOnStart;

        private int _currentAnimationIndex = -1;

        private void Awake()
        {
            // Temp fix for visual bug where first frame of note flashes the static sprite renderer
            if (_playFirstAnimationOnStart && _animatedSprites.Count > 0)
            {
                _spriteRenderer.sprite = null;
            }
        }

        private void Start()
        {
            if (_playFirstAnimationOnStart)
            {
                PlayAnimation(0, -1);
            }

            foreach (BeatAnimatedSprite sprite in _animatedSprites)
            {
                sprite.OnTransitionOut += HandleAnimTransitionOut;
            }
        }

        private void HandleAnimTransitionOut(BeatAnimatedSprite obj)
        {
            _currentAnimationIndex = _animatedSprites.IndexOf(obj);
        }

        public void SetSpriteAlpha(float alpha)
        {
            Color color = _spriteRenderer.color;
            color.a = alpha;
            _spriteRenderer.color = color;
        }

        public void SetSpriteVisible(bool visible)
        {
            _spriteRenderer.enabled = visible;
        }

        public void SetVisualObjectVisible(bool active)
        {
            _spriteRenderer.gameObject.SetActive(active);
        }

        public void SetHitParticle(bool visible)
        {
            ToggleParticle(_hitParticle, visible);
        }

        public void SetTrailParticle(bool visible)
        {
            ToggleParticle(_trailParticle, visible);
        }

        public void SetRotation(float rot)
        {
            _spriteRenderer.transform.localRotation = Quaternion.Euler(0, 0, rot);
        }

        [Button("Detect Animations")]
        private void DetectAnimations()
        {
            _animatedSprites.Clear();
            foreach (Transform child in transform)
            {
                var sprite = child.GetComponent<BeatAnimatedSprite>();
                if (sprite != null)
                {
                    _animatedSprites.Add(sprite);
                }
            }
        }

        private void ToggleParticle(ParticleSystem particle, bool visible)
        {
            if (particle == null)
            {
                return;
            }

            if (visible)
            {
                particle.Play();
            }
            else
            {
                particle.Stop();
            }
        }

        public void SetFlipX(bool flip)
        {
            _spriteRenderer.flipX = flip;
        }

        public void PlayAnimation(int index, int firstSubdv)
        {
            if (index == _currentAnimationIndex)
            {
                return;
            }

            if (index >= _animatedSprites.Count)
            {
                return;
            }

            if (_currentAnimationIndex != -1)
            {
                AnimationForceTransition(_currentAnimationIndex, index, firstSubdv);
            }
            else
            {
                _animatedSprites[index].Play(firstSubdv);
                _currentAnimationIndex = index;
            }
        }

        public void SetAnimationTransitionToOnEnd(int fromIndex, int toIndex)
        {
            _animatedSprites[fromIndex].SetupTransitionOnEnd(_animatedSprites[toIndex]);
        }

        public void AnimationForceTransition(int fromIndex, int toIndex, int currentSubdiv)
        {
            _animatedSprites[fromIndex].ForceTransition(_animatedSprites[toIndex], currentSubdiv);
        }
    }
}