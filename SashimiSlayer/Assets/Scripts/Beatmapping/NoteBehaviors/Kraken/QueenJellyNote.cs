using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.NoteBehaviors.Visuals;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.NoteBehaviors.Kraken
{
    /// <summary>
    ///     Note behavior that moves to slice position in fixed time, with visual variants for each slice landed
    /// </summary>
    public class QueenJellyNote : BeatNoteModule
    {
        [BoldHeader("Queen Jelly Note")]
        [InfoBox(
            "Note behavior that moves to slice position in fixed time, with visual variants for each slice landed")]
        [Header("Depends")]

        [SerializeField]
        private List<NoteVisualHandler> _damageVariantVisuals;

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private ParticleSystem[] _hitParticles;

        [SerializeField]
        private ParticleSystem[] _slainParticles;

        [SerializeField]
        private Transform _bodyTransform;

        [Header("Movement")]

        [SerializeField]
        private AnimationCurve _enterCurve;

        [SerializeField]
        private AnimationCurve _exitCurve;

        [SerializeField]
        private float _entranceDuration;

        [Header("Sliced Effects")]

        [SerializeField]
        private UnityEvent _onSliced;

        private Vector2 _startPos;
        private Vector2 _targetPos;
        private Vector2 _endPos;

        private bool _landedHit;

        private int _visualIndex;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (segment.Type == BeatNote.TimeSegmentType.PreEnding)
            {
                PreEndingOnTick((float)tickinfo.NormalizedSegmentTime);
            }

            if (segment.Type == BeatNote.TimeSegmentType.Interaction)
            {
                InteractionOnTick((float)tickinfo.NoteTime);
            }
        }

        /// <summary>
        ///     Note exiting the screen
        /// </summary>
        /// <param name="normalizedTime"></param>
        private void PreEndingOnTick(float normalizedTime)
        {
            if (_landedHit)
            {
                return;
            }

            float t = _exitCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_targetPos.x, _endPos.x, t),
                Mathf.Lerp(_targetPos.y, _endPos.y, t)
            );
        }

        private void InteractionOnTick(float rawTime)
        {
            float t = _enterCurve.Evaluate(rawTime / _entranceDuration);

            // prevents locking movement after entrance animation
            if (t <= 1.1f)
            {
                // X velocity is constant, y uses curve
                _bodyTransform.position = new Vector2(
                    Mathf.Lerp(_startPos.x, _targetPos.x, t),
                    Mathf.Lerp(_startPos.y, _targetPos.y, t)
                );
            }
        }

        private void BeatNote_SlicedByProtag(int interactionIndex,
            NoteInteraction.AttemptResult result)
        {
            _visualIndex++;

            if (_visualIndex >= _damageVariantVisuals.Count)
            {
                foreach (ParticleSystem particle in _slainParticles)
                {
                    particle.Play();
                }
            }
            else
            {
                foreach (ParticleSystem particle in _hitParticles)
                {
                    particle.Play();
                }
            }

            for (var i = 0; i < _damageVariantVisuals.Count; i++)
            {
                _damageVariantVisuals[i].SetVisible(i == _visualIndex);
            }

            _onSliced.Invoke();
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return new List<IInteractionUser.InteractionUsage>
            {
                new(NoteInteraction.InteractionType.Slice, 0, 1)
            };
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            // Lerp from start to target position with fixed time, then to end
            _startPos = _beatNote.StartPosition;
            _bodyTransform.position = _startPos;
            _targetPos = _beatNote.GetInteractionPosition(0, 0);
            _endPos = _beatNote.EndPosition;
            _visualIndex = 0;

            for (var i = 0; i < _damageVariantVisuals.Count; i++)
            {
                _damageVariantVisuals[i].SetVisible(i == _visualIndex);
            }

            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnSlicedByProtag += BeatNote_SlicedByProtag;
            _beatNote.OnNoteEnd += HandleNoteEnd;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnSlicedByProtag -= BeatNote_SlicedByProtag;
            _beatNote.OnNoteEnd -= HandleNoteEnd;
        }

        private void HandleNoteEnd(BeatNote.NoteTickInfo tickInfo)
        {
            foreach (NoteVisualHandler visual in _damageVariantVisuals)
            {
                visual.SetVisible(false);
            }
        }
    }
}