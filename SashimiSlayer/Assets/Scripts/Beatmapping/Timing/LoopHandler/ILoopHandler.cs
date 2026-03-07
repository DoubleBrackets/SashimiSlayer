using System;
using FMOD.Studio;

namespace Beatmapping.Timing.LoopHandler
{
    public interface ILoopHandler
    {
        /// <summary>
        ///     Updates the current success streak
        /// </summary>
        /// <param name="streak"></param>
        public void SetSuccessStreak(int streak);

        /// <summary>
        ///     Toggle loop skipping. Used as a dev tool and/or for exhibitioning
        /// </summary>
        public void ToggleSkipLoops();

        /// <summary>
        ///     Tick the loop guard to run it's logic
        /// </summary>
        /// <param name="tickInfo"></param>
        public void Tick(BeatmapRunner.TickInfo tickInfo);

        /// <summary>
        ///     True means paused, false means not paused
        /// </summary>
        public event Action<bool> OnPauseNoteSpawning;

        /// <summary>
        ///     Setup the loop handler for a new beatmap instance
        /// </summary>
        /// <param name="instance"></param>
        public void SetupNewBeatmap(EventInstance instance);

        public bool IsSkippingLoops { get; }
    }
}