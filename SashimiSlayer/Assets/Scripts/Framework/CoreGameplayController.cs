using System.Collections.Generic;
using System.Linq;
using Beatmapping;
using Beatmapping.Data;
using Beatmapping.Interaction.DataTypes;
using Beatmapping.Notes;
using Beatmapping.Service;
using CommonTypes;
using Core.Protag.Core;
using GameInput.Interface;
using Interactions.DataTypes;
using Interactions.Framework;
using Protag.Core;
using Unity.VisualScripting;

namespace Framework
{
    /// <summary>
    ///     Controls the top level gameplay elements, i.e player interactions and the beatmap
    /// </summary>
    public class CoreGameplayController
    {
        private Protaganist _protaganist;
        private InteractionService _interactionService;
        private IUserInput _userInputService;
        private IBeatmapService _beatmapService;

        public CoreGameplayController(
            Protaganist protaganist,
            InteractionService interactionService,
            IUserInput userInputService,
            IBeatmapService beatmapService)
        {
            _protaganist = protaganist;
            _interactionService = interactionService;
            _userInputService = userInputService;
            _beatmapService = beatmapService;

            _userInputService.OnBlockPoseChanged += HandleBlockPoseChanged;
            _userInputService.OnSheathStateChanged += HandleSheathStateChanged;
        }

        public void Update()
        {
            HandleSwordAngleChanged(_userInputService.GetSwordAngle());
            BeatmapTickFinalResults tickBeatmapResult = _beatmapService.TickForward();

            IList<NoteTickedResults> noteTickResults = tickBeatmapResult.NoteTickedResults;

            bool playerHit = noteTickResults.Any(a => a.DidHitPlayer);

            if (playerHit)
            {
                _protaganist.DoDamagedPresentation();
            }
        }

        private void DoInteractionFromProtag(InteractionAttempt attempt)
        {
            InteractionFinalResults interactionResult = _interactionService.AttemptInteraction(attempt);

            List<IInteractable> interactablesInteractedWith =
                interactionResult.SuccessfulInteractions.Select(a => a.Interactable).ToList();

            // We did not interact with anything, clean miss
            if (interactablesInteractedWith.Count == 0)
            {
                if (attempt.Type == InteractionType.Slice)
                {
                    _protaganist.DoSlicePresentation(interactablesInteractedWith, SlicePresentationTypes.Miss);
                }
                else if (attempt.Type == InteractionType.Block)
                {
                    _protaganist.DoBlockPresentation(BlockPresentationTypes.Miss, attempt.BlockPose);
                }

                return;
            }

            List<BeatNote> interactedNotes = interactablesInteractedWith
                .Select(a => a as BeatNote)
                .NotNull()
                .ToList();

            // This means we interacted with non-notes only (i.e menu UI)
            // For now, we treat it those a perfect hit for the purposes of VFX and so on
            if (interactedNotes.Count == 0)
            {
                if (attempt.Type == InteractionType.Slice)
                {
                    _protaganist.DoSlicePresentation(interactablesInteractedWith, SlicePresentationTypes.Perfect);
                }
                else if (attempt.Type == InteractionType.Block)
                {
                    _protaganist.DoBlockPresentation(BlockPresentationTypes.Perfect, attempt.BlockPose);
                }

                return;
            }

            var beatmapinteractionAttempt = new BeatmapInteractionAttempt
            {
                BlockPose = attempt.BlockPose,
                NoteInteractionType = attempt.Type == InteractionType.Slice
                    ? NoteInteractionType.Slice
                    : NoteInteractionType.Block,
                Notes = interactedNotes
            };

            IList<NoteInteractionFinalResult> beatmapInteractionResults =
                _beatmapService.GetNoteInteractionFinalResults(beatmapinteractionAttempt);

            List<NoteInteractionFinalResult> successfulInteractions =
                beatmapInteractionResults.Where(a => a.Successful).ToList();

            // Missed, i.e if we are outside the timing windows of any notes
            if (successfulInteractions.Count == 0)
            {
                if (attempt.Type == InteractionType.Slice)
                {
                    _protaganist.DoSlicePresentation(new List<IInteractable>(), SlicePresentationTypes.Miss);
                }
                else if (attempt.Type == InteractionType.Block)
                {
                    _protaganist.DoBlockPresentation(BlockPresentationTypes.Miss, attempt.BlockPose);
                }

                return;
            }

            // Hit, i.e if we are inside the timing windows of any notes
            // We make the assumption that the timings are uniform
            TimingWindow.TimingResult timingResult = beatmapInteractionResults.First().TimingResult;

            List<IInteractable> finalInteractedWith =
                successfulInteractions.Select(a => a.Note as IInteractable).ToList();

            if (attempt.Type == InteractionType.Slice)
            {
                _protaganist.DoSlicePresentation(finalInteractedWith, timingResult.ToSlicePresentationType());
            }
            else if (attempt.Type == InteractionType.Block)
            {
                _protaganist.DoBlockPresentation(timingResult.ToBlockPresentationType(), attempt.BlockPose);
            }
        }

        #region Input Handling

        private void HandleSheathStateChanged(SheathState sheathState)
        {
            _protaganist.SetSheatheState(sheathState, out bool doSlice);

            if (!doSlice)
            {
                return;
            }

            DoInteractionFromProtag(new InteractionAttempt
            {
                Type = InteractionType.Slice,
                Aim = new SwordAim
                {
                    Angle = _protaganist.SwordAngle,
                    Position = _protaganist.SwordPosition
                }
            });
        }

        private void HandleBlockPoseChanged(BlockPoses newState)
        {
            _protaganist.SetBlockPose(newState);
            DoInteractionFromProtag(new InteractionAttempt
            {
                Type = InteractionType.Block,
                BlockPose = newState
            });
        }

        private void HandleSwordAngleChanged(float angle)
        {
            _protaganist.SetSwordAngle(_userInputService.GetSwordAngle(), out bool changed);

            if (!changed)
            {
                return;
            }

            DoInteractionFromProtag(new InteractionAttempt
            {
                Type = InteractionType.Hover,
                Aim = new SwordAim
                {
                    Angle = _protaganist.SwordAngle,
                    Position = _protaganist.SwordPosition
                }
            });
        }

        #endregion
    }
}