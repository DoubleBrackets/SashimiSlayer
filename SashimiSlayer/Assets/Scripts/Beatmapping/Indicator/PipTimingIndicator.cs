using System.Collections.Generic;
using Base;
using Beatmapping.Indicator.Positioners;
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
        private PipPositioner _pipPositioner;

        [SerializeField]
        private int _pipCountOffset;

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

        private bool _didShake;
        private bool _didStartInteractionVisible;
        private bool _isFirstInteraction;

        [ShowNonSerializedField]
        private bool _isVisible;

        /// <summary>
        ///     Hidden because too many beats before interaction time
        /// </summary>
        [ShowNonSerializedField]
        private bool _prematurelyHidden;

        /// <summary>
        ///     On the next visible beat, flash the entry pip
        /// </summary>
        private bool _flashEntryQueued;

        private void OnDrawGizmosSelected()
        {
            if (_pipPositioner == null)
            {
                return;
            }

            List<(Vector2, float)> pipPositions =
                _pipPositioner.CalculatePipLocalPositions(4 + _pipCountOffset);

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

                Vector2 position = pipPositions[i].Item1;
                Gizmos.DrawSphere(centerPosition + position, 0.1f);
            }
        }

        public void SetupNewInteraction(BeatmapConfigSo beatmapConfigSo, bool isFirstInteraction)
        {
            _didShake = false;
            _didStartInteractionVisible = true;
            _isFirstInteraction = isFirstInteraction;
            _isVisible = true;
            _prematurelyHidden = false;

            if (!_initialized)
            {
                Initialize(beatmapConfigSo);
                _initialized = true;
            }
        }

        /// <summary>
        ///     Initialize the pip indicator. Should only be called once during lifetime
        /// </summary>
        /// <param name="beatmapConfigSo"></param>
        private void Initialize(BeatmapConfigSo beatmapConfigSo)
        {
            int beatsPerMeasure = beatmapConfigSo.BeatsPerMeasure;
            int totalPips = beatsPerMeasure + _pipCountOffset;

            List<(Vector2, float)> pipPositions = _pipPositioner.CalculatePipLocalPositions(totalPips);

            for (var i = 0; i < totalPips; i++)
            {
                IndicatorPip prefab = i == 0 ? _finalPip : _beatPip;
                IndicatorPip pip = Instantiate(prefab, _visualContainer);

                pip.transform.localPosition = pipPositions[i].Item1;
                pip.transform.localRotation = Quaternion.Euler(0, 0, pipPositions[i].Item2);

                pip.SetOn(false);
                _pips.Add(pip);
            }

            _shakeDuration = (float)(1 / beatmapConfigSo.Bpm * 60);
        }

        public void SetVisible(bool visible)
        {
            _isVisible = visible;
            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            foreach (IndicatorPip pip in _pips)
            {
                pip.SetVisible(_isVisible && !_prematurelyHidden);
            }
        }

        public void FlashFinalBeat()
        {
            if (_pips.Count == 0)
            {
                return;
            }

            _pips[0].FlashTriggerBeat();
        }

        public async UniTaskVoid Tick(BeatNote.NoteTickInfo tickInfo)
        {
            await UniTask.Delay((int)_delay * 1000, cancellationToken: destroyCancellationToken);

            NoteInteraction interaction = tickInfo.NoteSegment.Interaction;

            int beatsRemaining = CalculateBeatRemaining(interaction, tickInfo);
            bool beatChanged = _prevBeatRemaining != beatsRemaining;

            _prevBeatRemaining = beatsRemaining;

            // Don't show anything at all if the target subdiv is before any of the pips
            _prematurelyHidden = beatsRemaining > _pips.Count;
            UpdateVisibility();

            for (var i = 0; i < _pips.Count; i++)
            {
                // Counting down; i.e pip index 0 is the final beat
                if (_prematurelyHidden || !_isVisible)
                {
                    _didStartInteractionVisible = false;
                    continue;
                }

                bool isOn = i < beatsRemaining;
                _pips[i].SetOn(isOn);

                if (i == 0 && beatsRemaining == 1 && !_didShake)
                {
                    _didShake = true;
                    _pips[i].transform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato, fadeOut: false);
                }

                if (beatChanged)
                {
                    _pips[i].DoSquish();
                }

                bool validToShowEntryFlash = !_didStartInteractionVisible || _isFirstInteraction;
                if (_flashEntryQueued && validToShowEntryFlash)
                {
                    _pips[i].FlashEntry();
                    _flashEntryQueued = false;
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

        public void QueueFlashEntry()
        {
            _flashEntryQueued = true;
        }
    }
}