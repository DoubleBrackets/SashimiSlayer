using System.Collections.Generic;
using Beatmapping.NoteBehaviors.Visuals;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.NoteBehaviors
{
    /// <summary>
    ///     Module where the note spins to a vulnerable position
    /// </summary>
    public class SpinToVulnerable : BeatNoteModule
    {
        [BoldHeader("Spin To Vulnerable Behavior")]
        [InfoBox("Behavior that moves from impact point to vulnerable point while spinning")]
        [Header("Depends")]

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private Transform _bodyTransform;

        [SerializeField]
        private NoteVisualHandler _visuals;

        [Header("Config")]

        [SerializeField]
        private int _interactionIndex;

        [SerializeField]
        private ParticleSystem[] _dieParticles;

        [SerializeField]
        private AnimationCurve _moveCurve;

        [Header("Events")]

        public UnityEvent OnEnterDazed;

        private Vector2 _startPos;
        private Vector2 _endPos;
        private Vector2 _vulnerablePos;

        private bool _enteredDazed;

        private void BeatNote_OnSlicedByProtag(int interactionIndex,
            NoteInteraction.NoteInteraction.AttemptResult result)
        {
            if (interactionIndex != _interactionIndex)
            {
                return;
            }

            _visuals.SetVisible(false);
            foreach (ParticleSystem particle in _dieParticles)
            {
                particle.Play();
            }
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (tickinfo.InteractionIndex != _interactionIndex)
            {
                _enteredDazed = false;
                return;
            }

            NoteInteraction.NoteInteraction interaction = segment.Interaction;
            _visuals.SetSpriteAlpha(1f);

            if (interaction.Type == NoteInteractionType.Slice)
            {
                TargetToHitVisuals((float)tickinfo.NormalizedSegmentTime, (float)tickinfo.BeatmapTime);
            }
        }

        private void TargetToHitVisuals(float normalizedTime, float beatmapTime)
        {
            if (!_enteredDazed)
            {
                _enteredDazed = true;
                OnEnterDazed.Invoke();
            }

            // Lerp from target pos to vulnerable pos

            float t = _moveCurve.Evaluate(normalizedTime);

            // X velocity is constant, y uses curve
            _bodyTransform.position = new Vector2(
                Mathf.Lerp(_startPos.x, _vulnerablePos.x, normalizedTime),
                Mathf.Lerp(_startPos.y, _vulnerablePos.y, t)
            );

            _visuals.SetRotation(360f * beatmapTime);
        }

        public override IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return new List<INoteInteractionUser.InteractionUsage>
            {
                new(NoteInteractionType.Slice, _interactionIndex, 1)
            };
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _vulnerablePos = _beatNote.GetInteractionPosition(_interactionIndex, 0);
            _startPos = _beatNote.GetPreviousPosition(_interactionIndex);
            _endPos = _beatNote.EndPosition;

            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnSlicedByProtag += BeatNote_OnSlicedByProtag;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnSlicedByProtag -= BeatNote_OnSlicedByProtag;
        }
    }
}