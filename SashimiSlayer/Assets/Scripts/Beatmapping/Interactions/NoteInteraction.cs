using System;
using System.Collections.Generic;
using UnityEngine;

namespace Beatmapping.Interactions
{
    /// <summary>
    ///     An instantiated note interaction
    /// </summary>
    public class NoteInteraction
    {
        public enum InteractionType
        {
            /// <summary>
            ///     Interaction where the note will try to hit the protag, and the protag must block
            /// </summary>
            Block,

            /// <summary>
            ///     Interaction where the note is vulnerable, and the protag must hit it
            /// </summary>
            Slice
        }

        [Flags]
        public enum InteractionFlags
        {
            /// <summary>
            ///     Do not end the note on a successful hit. NOTE: THIS IS CURRENTLY UNUSED AND HAS NO DEFINED BEHAVIOR
            /// </summary>
            DoNotEndOnHit = 1 << 1
        }

        public enum NoteInteractionState
        {
            Default,
            Fail,
            Success
        }

        public InteractionType Type { get; }
        public List<Vector2> Positions { get; }

        public double TargetTime => _timingWindow.TargetTime;

        public NoteInteractionState State { get; private set; }

        public InteractionFlags Flags { get; }

        public SharedTypes.BlockPoseStates BlockPose { get; }

        public bool HideIndicator { get; private set; }

        private readonly TimingWindow _timingWindow;

        public NoteInteraction(InteractionType typeType,
            InteractionFlags interactionFlags,
            SharedTypes.BlockPoseStates blockPose,
            List<Vector2> positions,
            TimingWindow window,
            bool hideIndicator)
        {
            Type = typeType;
            Flags = interactionFlags;
            BlockPose = blockPose;
            _timingWindow = window;
            Positions = positions == null ? null : new List<Vector2>(positions);
            State = NoteInteractionState.Default;
            HideIndicator = hideIndicator;
        }

        public void ResetState()
        {
            State = NoteInteractionState.Default;
        }

        /// <summary>
        ///     Attempt an interaction
        /// </summary>
        /// <param name="attemptTime"></param>
        /// <param name="attemptedInteraction">the interaction attempted by the protag</param>
        /// <param name="blockPose">the block pose protag is using, if relevant</param>
        /// <returns></returns>
        public AttemptResult TryInteraction(
            double attemptTime,
            InteractionType attemptedInteraction,
            SharedTypes.BlockPoseStates blockPose = default)
        {
            // Interaction already succeeded or failed
            if (State != NoteInteractionState.Default)
            {
                return new AttemptResult
                {
                    ValidInteraction = false
                };
            }

            // Interaction type doesn't match
            if (attemptedInteraction != Type)
            {
                return new AttemptResult
                {
                    ValidInteraction = false
                };
            }

            TimingWindow.TimingResult timingResult = _timingWindow.CalculateTimingResult(attemptTime);

            // Outside timing window completely, counted as invalid
            if (timingResult.Score == TimingWindow.Score.Invalid)
            {
                return new AttemptResult
                {
                    ValidInteraction = false
                };
            }

            InteractionFlags flags = Flags;

            var result = new AttemptResult
            {
                ValidInteraction = true,
                TimingResult = timingResult,
                Flags = flags
            };

            // Block pose doesn't match, it's a miss
            if (Type == InteractionType.Block &&
                blockPose != BlockPose)
            {
                result.TimingResult.Score = TimingWindow.Score.Miss;
            }

            if (result.Passed)
            {
                State = NoteInteractionState.Success;
            }
            else if (result.TimingResult.Score == TimingWindow.Score.Miss)
            {
                State = NoteInteractionState.Fail;
            }

            return result;
        }

        /// <summary>
        ///     Is the given time within the interaction window for this interaction? Includes anti-spam lockout window
        /// </summary>
        /// <param name="time">time in beatmap timespace</param>
        /// <returns></returns>
        public bool IsInInteractTimingWindow(double time)
        {
            return _timingWindow.CalculateTimingResult(time).Score is TimingWindow.Score.Pass
                or TimingWindow.Score.Perfect or TimingWindow.Score.Miss;
        }

        /// <summary>
        ///     Is the given time within the passing window for this interaction?
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool IsInPassTimingWindow(double time)
        {
            return _timingWindow.CalculateTimingResult(time).Score is TimingWindow.Score.Pass
                or TimingWindow.Score.Perfect;
        }

        /// <summary>
        ///     Set the interaction as failed
        /// </summary>
        public void SetFailed()
        {
            State = NoteInteractionState.Fail;
        }

        /// <summary>
        ///     The result of an interaction attempt (i.e an attempted block or slice)
        /// </summary>
        public struct AttemptResult
        {
            /// <summary>
            ///     Was the interaction attempt even valid? (matching type, correct block pose, not locked out)
            /// </summary>
            public bool ValidInteraction;

            /// <summary>
            ///     The result of the timing window check for valid interactions
            /// </summary>
            public TimingWindow.TimingResult TimingResult;

            public InteractionFlags Flags;

            public bool Passed => ValidInteraction &&
                                  TimingResult.Score is TimingWindow.Score.Pass or TimingWindow.Score.Perfect;
        }

        /// <summary>
        ///     The final result of a note interaction; either a success when it occurs, or a failure after the timing window ends
        /// </summary>
        public struct FinalResult
        {
            public TimingWindow.TimingResult TimingResult;
            public InteractionType InteractionType;
            public SharedTypes.BlockPoseStates Pose;
            public bool Successful;

            public FinalResult(TimingWindow.TimingResult timingResult, InteractionType interactionType, bool successful)
            {
                TimingResult = timingResult;
                InteractionType = interactionType;
                Successful = successful;
                Pose = default;
            }
        }
    }
}