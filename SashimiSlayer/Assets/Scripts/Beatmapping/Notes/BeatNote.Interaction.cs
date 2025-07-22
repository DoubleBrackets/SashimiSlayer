using Base;
using Beatmapping.Interactions;
using Core.Protag;
using Events.Core;
using UnityEngine;

namespace Beatmapping.Notes
{
    public partial class BeatNote : DescMono
    {
        /// <summary>
        ///     Handle protag's attempt to block
        /// </summary>
        public void AttemptPlayerBlock(Protaganist.ProtagSwordState protagSwordState)
        {
            double currentBeatmapTime = _noteTickInfo.BeatmapTime;
            NoteInteraction interaction = _noteTickInfo.InsideInteractionWindow;

            if (interaction == null)
            {
                return;
            }

            NoteInteraction.AttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.Block,
                protagSwordState.BlockPose);

            // Hitting in the fail window means instant failure
            if (interactionAttemptResult.TimingResult.Score == TimingWindow.Score.Miss)
            {
                var earlyFailResult = new NoteInteraction.FinalResult(
                    interactionAttemptResult.TimingResult,
                    NoteInteraction.InteractionType.Block,
                    false
                )
                {
                    Pose = protagSwordState.BlockPose
                };

                _noteInteractionFinalResultEvent.Raise(earlyFailResult);
                OnInteractionFinalResult?.Invoke(_noteTickInfo, earlyFailResult);
                OnProtagFailBlock?.Invoke(_noteTickInfo, earlyFailResult);

                return;
            }

            // No pass means do nothing
            if (!interactionAttemptResult.Passed)
            {
                return;
            }

            var finalResult = new NoteInteraction.FinalResult(
                interactionAttemptResult.TimingResult,
                NoteInteraction.InteractionType.Block,
                true
            )
            {
                Pose = protagSwordState.BlockPose
            };

            _noteInteractionFinalResultEvent.Raise(finalResult);
            OnInteractionFinalResult?.Invoke(_noteTickInfo, finalResult);

            OnBlockedByProtag?.Invoke(GetInteractionIndex(interaction), interactionAttemptResult);

            Protaganist.Instance.SuccessfulBlock(protagSwordState.BlockPose);
        }

        /// <summary>
        ///     Handle protag's attempt to attack
        /// </summary>
        /// <param name="protagSwordState"></param>
        public bool AttemptPlayerSlice(Protaganist.ProtagSwordState protagSwordState)
        {
            // Check if slice is in hitbox
            Vector3 pos = _hitboxTransform.transform.position;
            float dist = protagSwordState.DistanceToSwordPlane(pos);
            bool isAttackOnTarget = dist < _hitboxRadius;

            if (!isAttackOnTarget)
            {
                return false;
            }

            // Call interaction logic
            double currentBeatmapTime = _noteTickInfo.BeatmapTime;
            NoteInteraction interaction = _noteTickInfo.InsideInteractionWindow;

            if (interaction == null)
            {
                return false;
            }

            NoteInteraction.AttemptResult interactionAttemptResult = interaction.TryInteraction(
                currentBeatmapTime,
                NoteInteraction.InteractionType.Slice,
                protagSwordState.BlockPose);

            // Hitting in the early lockout window fails immediately
            if (interactionAttemptResult.TimingResult.Score == TimingWindow.Score.Miss)
            {
                var earlyFailResult = new NoteInteraction.FinalResult(interactionAttemptResult.TimingResult,
                    NoteInteraction.InteractionType.Slice,
                    false);

                _noteInteractionFinalResultEvent.Raise(earlyFailResult);
                OnInteractionFinalResult?.Invoke(_noteTickInfo, earlyFailResult);
                OnProtagMissedHit?.Invoke(_noteTickInfo, earlyFailResult);
                return false;
            }

            // Other fails simply do nothing
            if (!interactionAttemptResult.Passed)
            {
                return false;
            }

            var finalResult = new NoteInteraction.FinalResult(interactionAttemptResult.TimingResult,
                NoteInteraction.InteractionType.Slice,
                true);

            _noteInteractionFinalResultEvent.Raise(finalResult);
            _objectSlicedEvent.Raise(new ObjectSlicedData
            {
                Position = _hitboxTransform.position
            });
            OnInteractionFinalResult?.Invoke(_noteTickInfo, finalResult);
            OnSlicedByProtag?.Invoke(GetInteractionIndex(interaction), interactionAttemptResult);

            return true;
        }
    }
}