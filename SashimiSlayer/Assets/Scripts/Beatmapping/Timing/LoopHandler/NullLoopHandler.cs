using System;
using FMOD.Studio;

namespace Beatmapping.Timing.LoopHandler
{
    public class NullLoopHandler : ILoopHandler
    {
        public void SetSuccessStreak(int streak)
        {
        }

        public void ToggleSkipLoops()
        {
        }

        public void Tick(BeatmapRunner.TickInfo tickInfo)
        {
        }

        public event Action<bool> OnPauseNoteSpawning;

        public void SetupNewBeatmap(EventInstance instance)
        {
        }

        public bool IsSkippingLoops => false;
    }
}