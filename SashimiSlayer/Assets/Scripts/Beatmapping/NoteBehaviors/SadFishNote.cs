using System.Collections.Generic;
using Beatmapping.NoteBehaviors.Visuals;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.NoteBehaviors
{
    public class SadFishNote : BeatNoteModule
    {
        [BoldHeader("Sad Fish Note")]
        [InfoBox("Fish behavior that travels in an arc to the Slice point, then arcs down to the end point")]
        [Header("Depends")]

        [SerializeField]
        private NoteVisualHandler _visual;

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private ParticleSystem[] _dieParticles;

        [SerializeField]
        private AnimationCurve _moveCurve;

        [SerializeField]
        private Transform _bodyTransform;

        private Vector2 _startPos;
        private Vector2 _targetPos;
        private Vector2 _endPos;

        private bool _landedHit;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (segment.Type == BeatNote.TimeSegmentType.PreEnding)
            {
                PreEndingOnTick((float)tickinfo.NormalizedSegmentTime);
            }

            if (segment.Type == BeatNote.TimeSegmentType.Interaction)
            {
                InteractionOnTick((float)tickinfo.NormalizedSegmentTime);
            }
        }

        /// <summary>
        ///     Note exits the screen
        /// </summary>
        /// <param name="normalizedTime"></param>
        private void PreEndingOnTick(float normalizedTime)
        {
            if (_landedHit)
            {
                return;
            }

            float t = _moveCurve.Evaluate(1 - normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_targetPos.x, _endPos.x, normalizedTime),
                Mathf.Lerp(_targetPos.y, _endPos.y, 1 - t)
            );

            _visual.SetRotation(90 * (1 - t));
            _visual.SetSpriteAlpha(0.5f);
        }

        /// <summary>
        ///     Note enters the screen and moves to target position
        /// </summary>
        /// <param name="normalizedTime"></param>
        private void InteractionOnTick(float normalizedTime)
        {
            float t = _moveCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _targetPos.x, normalizedTime),
                Mathf.Lerp(_startPos.y, _targetPos.y, t)
            );

            _visual.SetRotation(-90 * (1 - t));
            _visual.SetSpriteAlpha(1);
        }

        private void BeatNote_SlicedByProtag(int interactionIndex,
            NoteInteraction.NoteInteraction.AttemptResult result)
        {
            _visual.SetVisible(false);
            foreach (ParticleSystem particle in _dieParticles)
            {
                particle.Play();
            }
        }

        public override IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return new List<INoteInteractionUser.InteractionUsage>
            {
                new(NoteInteractionType.Slice, 0, 1)
            };
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            // Form an arc from start, with the peak at the target position
            _startPos = _beatNote.StartPosition;
            _bodyTransform.position = _startPos;
            _targetPos = _beatNote.GetInteractionPosition(0, 0);
            _endPos = _beatNote.EndPosition;
            _visual.SetFlipX(_targetPos.x > _startPos.x);

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
            _visual.SetVisible(true);
        }
    }
}