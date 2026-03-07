using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorUtils.BoldHeader;
using Events.Basic;
using NaughtyAttributes;
using UnityEngine;

namespace GameJuice.Notes
{
    /// <summary>
    ///     Squishes a transform on a beat.
    /// </summary>
    public class BeatSquish : MonoBehaviour
    {
        [Serializable]
        public struct BeatSquishable
        {
            public Transform transform;
            public int beatInterval;
            public int beatOffset;
        }

        [BoldHeader("Beat Squisher")]
        [InfoBox("Squishes some transforms on a beat pattern")]
        [Header("Targets")]

        [SerializeField]
        private List<BeatSquishable> _squishTransform;

        [Header("Config")]

        [SerializeField]
        private float _squishScale;

        [SerializeField]
        private float _squishDuration;

        [SerializeField]
        private float _delay;

        [Header("Event (In)")]

        [SerializeField]
        private IntEvent _beatPassedEvent;

        private void Awake()
        {
            _beatPassedEvent?.AddListener(HandleBeatPassed);
        }

        private void OnDestroy()
        {
            _beatPassedEvent?.RemoveListener(HandleBeatPassed);
        }

        private void HandleBeatPassed(int beatNumber)
        {
            OnBeatPassed(beatNumber).Forget();
        }

        private async UniTaskVoid OnBeatPassed(int beatNumber)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(_delay));

            foreach (BeatSquishable squishable in _squishTransform)
            {
                if ((beatNumber + squishable.beatOffset) % squishable.beatInterval != 0)
                {
                    continue;
                }

                if (squishable.transform == null)
                {
                    continue;
                }

                Squish(squishable);
            }
        }

        private void Squish(BeatSquishable squishable)
        {
            squishable.transform.localScale = new Vector3(1 / _squishScale, _squishScale, 1);
            squishable.transform.DOScaleY(1, _squishDuration);
            squishable.transform.DOScaleX(1, _squishDuration);
        }

        public void SquishAllImmediate()
        {
            foreach (BeatSquishable squishable in _squishTransform)
            {
                Squish(squishable);
            }
        }
    }
}