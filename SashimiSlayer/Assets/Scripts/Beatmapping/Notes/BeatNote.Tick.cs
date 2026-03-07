using Beatmapping.Data;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.Timing;

namespace Beatmapping.Notes
{
    public partial class BeatNote
    {
        /// <summary>
        ///     Update timing and invoke tick events
        /// </summary>
        /// <param name="tickInfo"></param>
        /// <param name="tickFlags"></param>
        public NoteTickedResults Tick(BeatmapRunner.TickInfo tickInfo, TickFlags tickFlags)
        {
            var didHitTarget = false;
            NoteInteractionFinalResult interactionFinalResult = default;
            var interactionProduced = false;

            UpdateTiming(tickInfo, tickFlags);

            int currentSegmentIndex = _noteTickInfo.SegmentIndex;
            NoteTimeSegment currentSegment = _noteTimeSegments[currentSegmentIndex];
            TimeSegmentType currentSegmentType = currentSegment.Type;

            NoteTimeSegment prevSegment = _prevTickInfo.NoteSegment;
            TimeSegmentType prevSegmentType = prevSegment.Type;

            bool triggerInteractions = tickFlags.HasFlag(TickFlags.TriggerInteractions);

            // Only if this is NOT the first tick
            if (triggerInteractions && !_isFirstTick)
            {
                interactionProduced = HandleExitingInteractionWindow(
                    _noteTickInfo,
                    _prevTickInfo,
                    out interactionFinalResult,
                    out didHitTarget);
            }

            // We might've skipped the spawn segment (e.g if the first tick is past the spawn segment)
            // NOTE: AS of now, spawn segments aren't even used...
            if ((_isFirstTick || prevSegmentType == TimeSegmentType.Spawn) && currentSegmentType != prevSegmentType)
            {
                OnNoteStart?.Invoke(_noteTickInfo);
            }

            if (prevSegmentType != TimeSegmentType.Ending && currentSegmentType == TimeSegmentType.Ending)
            {
                OnNoteEnd?.Invoke(_noteTickInfo);
            }

            OnTick?.Invoke(_noteTickInfo);

            switch (currentSegmentType)
            {
                case TimeSegmentType.Spawn:
                    // No special behavior
                    break;
                case TimeSegmentType.Interaction:
                    if (_isFirstInteraction)
                    {
                        _isFirstInteraction = false;
                        OnFirstInteractionTick?.Invoke(_noteTickInfo);
                    }

                    break;
                case TimeSegmentType.PreEnding:
                    // No special behavior
                    break;
                case TimeSegmentType.Ending:
                    // No special behavior
                    break;
                case TimeSegmentType.CleanedUp:
                    // Invoke event asking for manager to clean this up
                    OnReadyForCleanup?.Invoke(this);
                    break;
            }

            _isFirstTick = false;

            // TODO: More robust logic to handle loopbacks. Right now any loopback results in a total reset..
            if (_prevTickInfo.BeatmapTime > _noteTickInfo.BeatmapTime)
            {
                ResetState();
            }

            return new NoteTickedResults
            {
                HasInteractionFinalResult = interactionProduced,
                TargetHit = didHitTarget,
                InteractionFinalResult = interactionFinalResult
            };
        }

        /// <summary>
        ///     Update timing information
        /// </summary>
        /// <param name="tickInfo"></param>
        /// <param name="tickFlags"></param>
        /// <returns>true if we're in a segment, false otherwise</returns>
        private void UpdateTiming(BeatmapRunner.TickInfo tickInfo, TickFlags tickFlags)
        {
            double currentBeatmapTime = tickInfo.BeatmapTime;
            double previousBeatmapTime = _prevTickInfo.BeatmapTickInfo.BeatmapTime;

            int currentSegmentIndex = CalculateCurrentSegmentIndex(currentBeatmapTime);

            NoteTimeSegment currentSegment = _noteTimeSegments[currentSegmentIndex];

            // Note timespace timings
            double noteTime = currentBeatmapTime - _noteStartTime;
            double normalizedNoteTime = noteTime / (_noteEndTime - _noteStartTime);

            // Segment timespace timings
            double currentSegmentStartTime = currentSegment.SegmentStartTime;

            double currentSegmentTime = currentBeatmapTime - currentSegment.SegmentStartTime;

            // If we're in cleanup segment (last segment) we don't have an end time
            double normalizedSegmentTime = 0;
            if (currentSegmentIndex < _noteTimeSegments.Count - 1)
            {
                double currentSegmentEndTime = _noteTimeSegments[currentSegmentIndex + 1].SegmentStartTime;
                normalizedSegmentTime = currentSegmentTime / (currentSegmentEndTime - currentSegmentStartTime);
            }

            // Other timings
            double timeSinceNoteEnd = currentBeatmapTime - _noteEndTime;

            // See if we're in an interaction window
            NoteInteraction.NoteInteraction inWindowInteraction = null;
            NoteInteraction.NoteInteraction inPassingInteraction = null;

            // Assume we have no overlaps, so we break early
            // A passing window will be narrower than the interaction window
            foreach (NoteInteraction.NoteInteraction interaction in _allInteractions)
            {
                bool inInteractionWindow = interaction.IsInInteractTimingWindow(currentBeatmapTime);
                if (inInteractionWindow)
                {
                    inWindowInteraction = interaction;

                    bool inPassingInteractionWindow = interaction.IsInPassTimingWindow(currentBeatmapTime);
                    if (inPassingInteractionWindow)
                    {
                        inPassingInteraction = interaction;
                    }

                    break;
                }
            }

            int currentInteractionIndex = GetInteractionIndex(currentSegment.Interaction);

            _prevTickInfo = _noteTickInfo;
            _noteTickInfo = new NoteTickInfo
            {
                NoteSegment = _noteTimeSegments[currentSegmentIndex],
                NoteTime = noteTime,
                NormalizedNoteTime = normalizedNoteTime,
                SegmentTime = currentSegmentTime,
                NormalizedSegmentTime = normalizedSegmentTime,
                TimeSinceNoteEnd = timeSinceNoteEnd,
                InsideInteractionWindow = inWindowInteraction,
                InsidePassInteractionWindow = inPassingInteraction,

                SegmentIndex = currentSegmentIndex,
                InteractionIndex = currentInteractionIndex,

                BeatmapTickInfo = tickInfo,

                Flags = tickFlags
            };
        }

        private int CalculateCurrentSegmentIndex(double currentBeatmapTime)
        {
            var inSegment = false;
            int segmentIndex = -1;
            for (var i = 0; i < _noteTimeSegments.Count; i++)
            {
                if (currentBeatmapTime < _noteTimeSegments[i].SegmentStartTime)
                {
                    segmentIndex = i - 1;
                    inSegment = true;
                    break;
                }
            }

            // If we're past the last segment, we're in the last segment (cleanup)
            if (!inSegment)
            {
                segmentIndex = _noteTimeSegments.Count - 1;
            }

            // We're before spawn segment; for now, just use first segment
            if (segmentIndex == -1)
            {
                return 0;
            }

            return segmentIndex;
        }

        /// <summary>
        ///     Handle exiting an interaction window. Returns true if an interaction final result was produced, false otherwise
        /// </summary>
        /// <param name="noteTickInfo"></param>
        /// <param name="previousTiming"></param>
        /// <param name="interactionFinalResult"></param>
        /// <param name="didHitTarget"></param>
        /// <returns></returns>
        private bool HandleExitingInteractionWindow(
            NoteTickInfo noteTickInfo,
            NoteTickInfo previousTiming,
            out NoteInteractionFinalResult interactionFinalResult,
            out bool didHitTarget)
        {
            NoteInteraction.NoteInteraction currentInsidePassWindowInteraction =
                noteTickInfo.InsidePassInteractionWindow;
            NoteInteraction.NoteInteraction prevInsidePassWindowInteraction =
                previousTiming.InsidePassInteractionWindow;

            didHitTarget = false;
            interactionFinalResult = default;

            // If we were previously inside an interaction window, and now we're not, we've exited the window
            // TODO: This does not work properly with overlapping windows
            bool didJustExitInteractionWindow =
                prevInsidePassWindowInteraction != null && currentInsidePassWindowInteraction == null;
            if (!didJustExitInteractionWindow)
            {
                interactionFinalResult = default;
                return false;
            }

            NoteInteraction.NoteInteraction.NoteInteractionState interactionState =
                prevInsidePassWindowInteraction.State;

            // Special case blocks, since we want the "hit" to only happen when the window passes
            // Even if it is already a failed interaction
            // e.g if the player tries to block too early, we still want the "impact" to occur at the end instead of immediately
            if (interactionState != NoteInteraction.NoteInteraction.NoteInteractionState.Success
                && prevInsidePassWindowInteraction.Type == NoteInteractionType.Block)
            {
                didHitTarget = true;
            }

            // If the interaction ended without a success or failure, then we need to handle
            // it here as a failure due to inaction
            if (interactionState == NoteInteraction.NoteInteraction.NoteInteractionState.Default)
            {
                prevInsidePassWindowInteraction.SetFailed();

                var finalResult = new NoteInteractionFinalResult(
                    this,
                    default,
                    prevInsidePassWindowInteraction.Type,
                    false)
                {
                    Pose = prevInsidePassWindowInteraction.BlockPose
                };

                // Failure events. Use previous tick info, since that is the tick with the interaction failed
                switch (prevInsidePassWindowInteraction.Type)
                {
                    case NoteInteractionType.Block:
                        OnProtagFailBlock?.Invoke(previousTiming, finalResult);
                        break;
                    case NoteInteractionType.Slice:
                        OnProtagMissedHit?.Invoke(previousTiming, finalResult);
                        break;
                }

                OnInteractionFinalResult?.Invoke(previousTiming, finalResult);

                interactionFinalResult = finalResult;
                return true;
            }

            return false;
        }
    }
}