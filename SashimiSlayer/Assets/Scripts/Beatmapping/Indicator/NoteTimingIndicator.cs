using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Script that handles the visual timing indicators for a beat note.
    /// </summary>
    public class NoteTimingIndicator : BeatNoteModule
    {
        [BoldHeader("Note Timing Indicator")]
        [InfoBox("Handles a note's timing indicators for slicing and blocking")]
        [Header("Visuals")]

        [SerializeField]
        private PipTimingIndicator _sliceTimingIndicators;

        [SerializeField]
        private List<PipTimingIndicator> _blockTimingIndicators;

        private BeatNote _beatNote;

        private NoteInteraction _lastInteraction;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickInfo)
        {
            BeatNote.TimeSegmentType segmentType = tickInfo.NoteSegment.Type;

            if (segmentType != BeatNote.TimeSegmentType.Interaction)
            {
                _sliceTimingIndicators.SetVisible(false);
                foreach (PipTimingIndicator blockTimingIndicator in _blockTimingIndicators)
                {
                    blockTimingIndicator.SetVisible(false);
                }

                if (_lastInteraction != null && _lastInteraction != null)
                {
                    FlashFinalBeat(_lastInteraction);
                }

                _lastInteraction = null;
                return;
            }

            NoteInteraction interaction = tickInfo.NoteSegment.Interaction;

            // Optimization that causes a delay for looping notes...
            /*if (!tickInfo.BeatmapTickInfo.CrossedSubdivThisTick)
            {
                return;
            }*/

            bool isNewInteraction = _lastInteraction != interaction;

            // Flash the final beat of previous interaction
            if (isNewInteraction && _lastInteraction != null)
            {
                FlashFinalBeat(_lastInteraction);
            }

            TickInteraction(interaction, tickInfo, isNewInteraction);
            _lastInteraction = interaction;
        }

        private void TickInteraction(NoteInteraction interaction, BeatNote.NoteTickInfo tickInfo, bool isNewInteraction)
        {
            if (interaction.Type == NoteInteraction.InteractionType.Block)
            {
                if (isNewInteraction)
                {
                    _sliceTimingIndicators.SetVisible(false);
                }

                TickBlockIndicators(tickInfo, interaction.BlockPose, isNewInteraction);
            }
            else if (interaction.Type == NoteInteraction.InteractionType.Slice)
            {
                if (isNewInteraction)
                {
                    _sliceTimingIndicators.SetVisible(true);
                    _sliceTimingIndicators.SetupNewInteraction();
                    foreach (PipTimingIndicator blockIndicator in _blockTimingIndicators)
                    {
                        blockIndicator.SetVisible(false);
                    }
                }

                _sliceTimingIndicators.Tick(tickInfo).Forget();
            }
        }

        private void FlashFinalBeat(NoteInteraction interaction)
        {
            if (interaction.Type == NoteInteraction.InteractionType.Block)
            {
                foreach (PipTimingIndicator blockTimingIndicator in _blockTimingIndicators)
                {
                    blockTimingIndicator.FlashFinalBeat();
                }
            }
            else if (interaction.Type == NoteInteraction.InteractionType.Slice)
            {
                _sliceTimingIndicators.FlashFinalBeat();
            }
        }

        private void TickBlockIndicators(BeatNote.NoteTickInfo tickInfo, SharedTypes.BlockPoseStates blockPose,
            bool isNewInteraction)
        {
            var blockIndex = (int)blockPose;

            for (var i = 0; i < _blockTimingIndicators.Count; i++)
            {
                if (isNewInteraction)
                {
                    _blockTimingIndicators[i].SetupNewInteraction();
                    _blockTimingIndicators[i].SetVisible(i == blockIndex);
                }
            }

            _blockTimingIndicators[blockIndex].Tick(tickInfo).Forget();
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _lastInteraction = null;
            _beatNote = GetComponentInParent<BeatNote>();
            _beatNote.OnTick += BeatNote_OnTick;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
        }
    }
}