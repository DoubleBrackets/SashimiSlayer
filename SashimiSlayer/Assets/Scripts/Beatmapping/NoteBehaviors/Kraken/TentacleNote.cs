using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.NoteBehaviors.Visuals;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using UnityEngine;

namespace Beatmapping.NoteBehaviors.Kraken
{
    public class TentacleNote : BeatNoteModule
    {
        [Header("Depends")]

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private NoteVisualHandler _visual;

        [SerializeField]
        private Transform _bodyTransform;

        [Header("Animations")]

        [SerializeField]
        private int _spawnAnimation;

        [SerializeField]
        private int _attackAnimation;

        [SerializeField]
        private int _leaveAnimation;

        [SerializeField]
        private int _idleAnimation;

        [SerializeField]
        private int _attackAnimationWindup;

        [SerializeField]
        private ParticleSystem[] _damagedParticles;

        /// <summary>
        ///     Used to prevent the attack animation from playing multiple times
        /// </summary>
        private NoteInteraction _attackAnimationPlayedInteraction;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;
            if (segment.Type != BeatNote.TimeSegmentType.Interaction)
            {
                return;
            }

            NoteInteraction interaction = segment.Interaction;

            if (interaction.Type == NoteInteraction.InteractionType.Block)
            {
                if (interaction != _attackAnimationPlayedInteraction)
                {
                    IncomingAttackVisuals(tickinfo, interaction);
                }
            }
        }

        private void BeatNote_OnEnd(BeatNote.NoteTickInfo tickinfo)
        {
            _visual.AnimationForceTransition(_idleAnimation, _leaveAnimation, tickinfo.SubdivisionIndex);
            _visual.SetSpriteAlpha(0.7f);
        }

        /// <summary>
        ///     Play the attack animation some number of subdivisions before the target time
        /// </summary>
        /// <param name="noteTickInfo"></param>
        /// <param name="noteInteraction"></param>
        private void IncomingAttackVisuals(BeatNote.NoteTickInfo noteTickInfo, NoteInteraction noteInteraction)
        {
            int targetSubdivIndex = noteTickInfo.BeatmapTickInfo.GetClosestSubdivisionIndex(noteInteraction.TargetTime);
            int currentSubdiv = noteTickInfo.SubdivisionIndex;

            if (currentSubdiv + _attackAnimationWindup >= targetSubdivIndex)
            {
                _visual.AnimationForceTransition(_idleAnimation, _attackAnimation, currentSubdiv);
                _visual.AnimationForceTransition(_spawnAnimation, _attackAnimation, currentSubdiv);
                _visual.SetupAnimationTransitionOnEnd(_attackAnimation, _idleAnimation);
                _attackAnimationPlayedInteraction = noteInteraction;
            }
        }

        private void HandleOnSliced(int interactionIndex, NoteInteraction.AttemptResult result)
        {
            foreach (ParticleSystem particle in _damagedParticles)
            {
                particle.Play();
            }

            _visual.SetVisible(false);
        }

        private void HandleSpawned(BeatNote.NoteTickInfo tickinfo)
        {
            _visual.PlayAnimation(_spawnAnimation, tickinfo.SubdivisionIndex);
            _visual.SetupAnimationTransitionOnEnd(_spawnAnimation, _idleAnimation);
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _beatNote.OnNoteStart += HandleSpawned;
            _beatNote.OnSlicedByProtag += HandleOnSliced;
            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnNoteEnd += BeatNote_OnEnd;

            _bodyTransform.position = beatNote.StartPosition;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnNoteStart -= HandleSpawned;
            _beatNote.OnSlicedByProtag -= HandleOnSliced;
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnNoteEnd -= BeatNote_OnEnd;
        }
    }
}