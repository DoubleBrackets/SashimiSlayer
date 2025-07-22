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
        private PipTimingIndicator _currentActiveIndicator;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickInfo)
        {
            BeatNote.TimeSegmentType segmentType = tickInfo.NoteSegment.Type;

            if (segmentType == BeatNote.TimeSegmentType.Interaction)
            {
                NoteInteraction interaction = tickInfo.NoteSegment.Interaction;

                bool isNewInteraction = _lastInteraction != interaction;
                bool isFirstInteraction = _lastInteraction == null;

                if (isNewInteraction)
                {
                    // Flash the final beat of previous interaction
                    if (_lastInteraction != null)
                    {
                        _currentActiveIndicator.FlashFinalBeat();
                    }

                    SwitchIndicators(interaction, tickInfo.BeatmapTickInfo.CurrentBeatmap, isFirstInteraction);

                    _currentActiveIndicator.QueueFlashEntry();

                    _lastInteraction = interaction;
                }

                _currentActiveIndicator.Tick(tickInfo).Forget();
            }
            else
            {
                if (_lastInteraction == null)
                {
                    return;
                }

                // Not an interaction; hide all indicators
                _sliceTimingIndicators.SetVisible(false);
                foreach (PipTimingIndicator blockTimingIndicator in _blockTimingIndicators)
                {
                    blockTimingIndicator.SetVisible(false);
                }

                // Flash the final beat of the last interaction
                if (_lastInteraction != null)
                {
                    _currentActiveIndicator.FlashFinalBeat();
                }

                _currentActiveIndicator = null;
                _lastInteraction = null;
            }
        }

        /// <summary>
        ///     Switch to the matching indicator and initialize it. This should be called on new interaction
        /// </summary>
        private void SwitchIndicators(NoteInteraction interaction, BeatmapConfigSo currentBeatmap,
            bool firstInteraction)
        {
            bool hideIndicator = interaction.HideIndicator;

            if (interaction.Type == NoteInteraction.InteractionType.Block)
            {
                // Select matching block indicator
                var blockIndex = (int)interaction.BlockPose;
                _blockTimingIndicators[blockIndex].SetupNewInteraction(currentBeatmap, firstInteraction);
                for (var i = 0; i < _blockTimingIndicators.Count; i++)
                {
                    _blockTimingIndicators[i].SetVisible(i == blockIndex && !hideIndicator);
                }

                _currentActiveIndicator = _blockTimingIndicators[blockIndex];

                _sliceTimingIndicators.SetVisible(false);
            }
            else if (interaction.Type == NoteInteraction.InteractionType.Slice)
            {
                _sliceTimingIndicators.SetupNewInteraction(currentBeatmap, firstInteraction);
                _sliceTimingIndicators.SetVisible(!hideIndicator);
                _currentActiveIndicator = _sliceTimingIndicators;

                // Hide all block indicators
                foreach (PipTimingIndicator blockIndicator in _blockTimingIndicators)
                {
                    blockIndicator.SetVisible(false);
                }
            }
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