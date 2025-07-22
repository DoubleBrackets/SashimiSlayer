using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using EditorUtils.BoldHeader;
using Events;
using NaughtyAttributes;
using UnityEngine;

namespace Feel.Notes
{
    /// <summary>
    ///     Simple sprite flipbook that changes whenever a beat event is triggered
    /// </summary>
    public class BeatAnimatedSprite : MonoBehaviour
    {
        [BoldHeader("Beat Animated Sprite")]
        [InfoBox("Simple sprite flipbook that changes on a beat pattern")]
        [Header("Depends")]

        [SerializeField]
        private IntEvent _incrementEvent;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [Header("Config")]

        [SerializeField]
        private Sprite[] _sprites;

        [SerializeField]
        private int _incrementInterval;

        [SerializeField]
        private bool _loop;

        [SerializeField]
        private float _delay;

        [SerializeField]
        private bool _animationEnabled;

        public event Action<BeatAnimatedSprite> OnTransitionOut;

        private int _firstSubdivIndex;
        private int _currentSubdiv;

        private BeatAnimatedSprite _transitionTo;

        private CancellationToken _destroyCancellationToken;

        private void OnEnable()
        {
            _incrementEvent.AddListener(HandleEvent);
            _destroyCancellationToken = this.GetCancellationTokenOnDestroy();
        }

        private void OnDisable()
        {
            _incrementEvent.RemoveListener(HandleEvent);
        }

        private void HandleEvent(int currentSubdiv)
        {
            _currentSubdiv = currentSubdiv;

            if (!_animationEnabled)
            {
                return;
            }

            int relativeIndex = currentSubdiv - _firstSubdivIndex;
            if (relativeIndex % _incrementInterval == 0)
            {
                int spriteIndex = relativeIndex / _incrementInterval;
                if (spriteIndex > _sprites.Length - 1)
                {
                    if (_transitionTo != null)
                    {
                        ForceTransition(_transitionTo, currentSubdiv);
                        _transitionTo = null;
                        return;
                    }

                    if (!_loop)
                    {
                        Stop();
                        return;
                    }
                }

                SetSprite(spriteIndex).Forget();
            }
        }

        private async UniTaskVoid SetSprite(int newIndex)
        {
            await UniTask.Delay((int)(_delay * 1000));

            // Prevent changing after transitioning away
            if (!_animationEnabled)
            {
                return;
            }

            if (_destroyCancellationToken.IsCancellationRequested)
            {
                return;
            }

            _spriteRenderer.sprite = _sprites[newIndex % _sprites.Length];
        }

        /// <summary>
        ///     Setup a transition to another BeatAnimatedSprite when this one ends
        /// </summary>
        /// <param name="transitionTo"></param>
        public void SetupTransitionOnEnd(BeatAnimatedSprite transitionTo)
        {
            _transitionTo = transitionTo;
        }

        /// <summary>
        ///     Force transition to another BeatAnimatedSprite immediately
        /// </summary>
        /// <param name="to"></param>
        /// <param name="currentSubdiv"></param>
        public void ForceTransition(BeatAnimatedSprite to, int currentSubdiv = -1)
        {
            if (!_animationEnabled)
            {
                return;
            }

            OnTransitionOut?.Invoke(to);
            to.Play(currentSubdiv);
            Stop();
        }

        public void Play(int firstSubdiv = -1)
        {
            _animationEnabled = true;
            _firstSubdivIndex = firstSubdiv == -1 ? _currentSubdiv : firstSubdiv;
            _spriteRenderer.sprite = _sprites[0];
        }

        public void Stop()
        {
            _animationEnabled = false;
        }
    }
}