using Beatmapping.NoteInteraction.DataTypes;
using CommonTypes;
using Interactions.DataTypes;
using UnityEngine;

namespace Beatmapping.Notes
{
    public partial class BeatNote
    {
        /// <summary>
        ///     Evaluate the given interaction attempt. Interactions can be invalid if the interaction window has passed
        /// </summary>
        /// <param name="attempt"></param>
        /// <param name="result"></param>
        /// <returns>true of interaction is valid, false otherwise</returns>
        public bool EvaluateInteraction(BeatmapInteractionAttempt attempt, out NoteInteractionFinalResult result)
        {
            result = default;
            if (attempt.NoteInteractionType == NoteInteractionType.Block)
            {
                return EvaluateBlockInteraction(out result, attempt.BlockPose);
            }

            if (attempt.NoteInteractionType == NoteInteractionType.Slice)
            {
                return EvaluateSliceInteraction(out result);
            }

            return false;
        }

        private bool EvaluateBlockInteraction(out NoteInteractionFinalResult result, BlockPoses blockPose)
        {
            double currentBeatmapTime = _noteTickInfo.BeatmapTime;
            NoteInteraction.NoteInteraction interaction = _noteTickInfo.InsideInteractionWindow;

            if (interaction == null)
            {
                result = default;
                return false;
            }

            NoteInteraction.NoteInteraction.AttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteractionType.Block,
                blockPose);

            // Hitting in the fail window means instant failure
            if (interactionAttemptResult.TimingResult.Score == TimingWindow.Score.Miss)
            {
                result = new NoteInteractionFinalResult(
                    this,
                    interactionAttemptResult.TimingResult,
                    NoteInteractionType.Block,
                    false,
                    blockPose
                );

                OnInteractionFinalResult?.Invoke(_noteTickInfo, result);
                OnProtagFailBlock?.Invoke(_noteTickInfo, result);

                return true;
            }

            // No pass means do nothing
            if (!interactionAttemptResult.Passed)
            {
                result = default;
                return false;
            }

            result = new NoteInteractionFinalResult(
                this,
                interactionAttemptResult.TimingResult,
                NoteInteractionType.Block,
                true,
                blockPose
            );

            OnInteractionFinalResult?.Invoke(_noteTickInfo, result);
            OnBlockedByProtag?.Invoke(GetInteractionIndex(interaction), interactionAttemptResult);

            return true;
        }

        private bool EvaluateSliceInteraction(out NoteInteractionFinalResult result)
        {
            // Call interaction logic
            double currentBeatmapTime = _noteTickInfo.BeatmapTime;
            NoteInteraction.NoteInteraction interaction = _noteTickInfo.InsideInteractionWindow;

            if (interaction == null)
            {
                result = default;
                return false;
            }

            NoteInteraction.NoteInteraction.AttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteractionType.Slice);

            // Hitting in the early lockout window fails immediately
            if (interactionAttemptResult.TimingResult.Score == TimingWindow.Score.Miss)
            {
                result = new NoteInteractionFinalResult(
                    this,
                    interactionAttemptResult.TimingResult,
                    NoteInteractionType.Slice,
                    false);

                OnInteractionFinalResult?.Invoke(_noteTickInfo, result);
                OnProtagMissedHit?.Invoke(_noteTickInfo, result);

                return true;
            }

            // Other fails count as invalid, allowing for retries
            if (!interactionAttemptResult.Passed)
            {
                result = default;
                return false;
            }

            result = new NoteInteractionFinalResult(
                this,
                interactionAttemptResult.TimingResult,
                NoteInteractionType.Slice,
                true);

            OnInteractionFinalResult?.Invoke(_noteTickInfo, result);
            OnSlicedByProtag?.Invoke(GetInteractionIndex(interaction), interactionAttemptResult);

            return true;
        }

        public InteractionResult AttemptInteraction(InteractionAttempt attempt)
        {
            // Blocks are always successful; the actual logic is handled through note interactions
            if (attempt.Type == InteractionType.Block)
            {
                return InteractionResult.Success(this);
            }

            // Slices use hitbox radius to determine success
            if (attempt.Type == InteractionType.Slice)
            {
                return _circleSliceable.AttemptInteraction(attempt);
            }

            return InteractionResult.Fail(this);
        }

        public Vector2 Position => _hitboxTransform.position;
    }
}