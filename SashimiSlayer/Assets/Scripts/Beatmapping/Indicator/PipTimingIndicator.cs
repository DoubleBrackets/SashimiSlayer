using System.Collections.Generic;
using Base;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Script that handles timing indicators using pips that count down
    /// </summary>
    public class PipTimingIndicator : DescMono
    {
        [BoldHeader("Pip Timing Indicator")]
        [InfoBox("Represents a single series of pips that indicate timing")]
        [SerializeField]
        private Transform _visualContainer;

        [Header("Layout")]

        [SerializeField]
        private int _pipCountOffset;

        [SerializeField]
        private float _centerAngle;

        [SerializeField]
        private float _pipIntervalAngle;

        [SerializeField]
        private float _pipRadius;

        [SerializeField]
        private int _pipDirection;

        [SerializeField]
        private Vector2 _centerOffset;

        [Header("Indicator")]

        [SerializeField]
        private IndicatorPip _beatPip;

        [SerializeField]
        private IndicatorPip _finalPip;

        [SerializeField]
        private float _delay;

        [SerializeField]
        private float _shakeStrength;

        [SerializeField]
        private int _shakeVibrato;

        private readonly List<IndicatorPip> _pips = new();

        [Header("Shake")]

        private float _shakeDuration;

        private bool _initialized;

        private int _prevBeatRemaining;

        private bool _flashOnNext;
        private bool _didShake;

        private void OnDrawGizmosSelected()
        {
            List<Vector2> pipPositions =
                CalculatePipLocalPositions(_centerAngle, _pipCountOffset + 4, _pipIntervalAngle, _pipDirection);

            Vector2 centerPosition = _visualContainer.position;
            for (var i = 0; i < pipPositions.Count; i++)
            {
                if (i == 0)
                {
                    Gizmos.color = Color.green;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                Vector2 position = pipPositions[i];
                Gizmos.DrawSphere(centerPosition + position, 0.1f);
            }
        }

        public void SetupNewInteraction()
        {
            _flashOnNext = true;
            _didShake = false;
        }

        private void Initialize(BeatmapConfigSo beatmapConfigSo)
        {
            int beatsPerMeasure = beatmapConfigSo.BeatsPerMeasure;

            int totalPips = beatsPerMeasure + _pipCountOffset;
            float startingAngle = _centerAngle + totalPips * _pipIntervalAngle / 2f * _pipDirection;

            List<Vector2> pipPositions =
                CalculatePipLocalPositions(_centerAngle, totalPips, _pipIntervalAngle, _pipDirection);

            for (var i = 0; i < totalPips; i++)
            {
                IndicatorPip prefab = i == 0 ? _finalPip : _beatPip;
                IndicatorPip pip = Instantiate(prefab, _visualContainer);
                pip.Setup();

                pip.transform.localPosition = pipPositions[i];

                pip.SetOn(false);
                _pips.Add(pip);
            }

            _shakeDuration = (float)(1 / beatmapConfigSo.Bpm * 60);
        }

        /// <summary>
        /// </summary>
        /// <param name="centerAngle">center angle in degrees</param>
        /// <param name="totalPips"></param>
        /// <param name="pipIntervalAngle">interval angle in degrees</param>
        /// <param name="direction">positive or negative 1</param>
        /// <returns></returns>
        private List<Vector2> CalculatePipLocalPositions(float centerAngle, int totalPips, float pipIntervalAngle,
            int direction)
        {
            float startingAngle = centerAngle - (totalPips - 1) * pipIntervalAngle / 2f * direction;

            var positions = new List<Vector2>(totalPips);
            for (var i = 0; i < totalPips; i++)
            {
                Vector2 dir = Quaternion.Euler(
                                  0,
                                  0,
                                  startingAngle + i * pipIntervalAngle * direction) *
                              Vector2.up;

                positions.Add(dir * _pipRadius + _centerOffset);
            }

            return positions;
        }

        public void SetVisible(bool visible)
        {
            foreach (IndicatorPip pip in _pips)
            {
                pip.SetVisible(visible);
            }
        }

        public void FlashFinalBeat()
        {
            if (_pips.Count == 0)
            {
                return;
            }

            _pips[0].Flash();
        }

        public async UniTaskVoid Tick(BeatNote.NoteTickInfo tickInfo)
        {
            await UniTask.Delay((int)_delay * 1000, cancellationToken: destroyCancellationToken);
            if (!_initialized)
            {
                Initialize(tickInfo.BeatmapTickInfo.CurrentBeatmap);
                _initialized = true;
            }

            NoteInteraction interaction = tickInfo.NoteSegment.Interaction;

            int beatsRemaining = CalculateBeatRemaining(interaction, tickInfo);
            float normalized = 1 - (float)beatsRemaining / (_pips.Count - 1);

            bool changed = _prevBeatRemaining != beatsRemaining;
            _prevBeatRemaining = beatsRemaining;

            // Don't show anything at all if the target subdiv is before any of the pips
            bool shouldShowPips = beatsRemaining < _pips.Count;

            for (var i = 0; i < _pips.Count; i++)
            {
                // Counting down; i.e pip index 0 is the final beat
                bool isVisible = shouldShowPips;
                _pips[i].SetVisible(isVisible);

                if (!isVisible)
                {
                    _pips[i].SetOn(false);
                    continue;
                }

                bool isOn = i <= beatsRemaining;
                bool wasOn = _pips[i].IsOn;
                _pips[i].SetOn(isOn);
                _pips[i].SetAlpha(normalized);

                if (i == 0 && beatsRemaining == 1 && !_didShake)
                {
                    _didShake = true;
                    _pips[i].transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, fadeOut: false);
                }

                if (i == 0 && !isOn && _flashOnNext)
                {
                    _flashOnNext = false;
                    _pips[i].Flash();
                }

                if (changed)
                {
                    _pips[i].DoSquish();
                }
            }
        }

        private int CalculateBeatRemaining(NoteInteraction interaction, BeatNote.NoteTickInfo tickInfo)
        {
            int subdivsPerBeat = tickInfo.BeatmapTickInfo.CurrentBeatmap.Subdivisions;
            int currentSubdivision = tickInfo.SubdivisionIndex;

            // We need to calculate in subdivisions for off beats
            double interactionTargetTime = interaction.TargetTime;
            int targetSubdiv = tickInfo.BeatmapTickInfo.GetClosestSubdivisionIndex(interactionTargetTime);

            int beatsRemaining = (targetSubdiv - currentSubdivision + 1) / subdivsPerBeat;

            return beatsRemaining;
        }
    }
}