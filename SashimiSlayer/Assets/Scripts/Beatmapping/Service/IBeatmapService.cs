using System.Collections.Generic;
using Beatmapping.Data;
using Beatmapping.NoteInteraction.DataTypes;

namespace Beatmapping.Service
{
    public interface IBeatmapService
    {
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public IList<NoteInteractionFinalResult> GetNoteInteractionFinalResults(BeatmapInteractionAttempt attempt);

        /// <summary>
        ///     Tick the beatmap forward
        /// </summary>
        /// <returns></returns>
        public BeatmapTickFinalResults TickForward();

        public void BeginRunningBeatmap(BeatmapConfigSo beatmap);

        public void ToggleSkipLoop();
    }
}