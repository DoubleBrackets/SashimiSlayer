namespace Beatmapping.Tooling
{
    /// <summary>
    ///     Beatmap editor settings. Intended to be modified through Editor Windows, bridging gameplay and editor.
    ///     This should NOT be placed in an editor folder, since gameplay scripts will read from this
    /// </summary>
    public static class BeatmappingEditingSettings
    {
        /// <summary>
        ///     Should the play mode level start from the editing timeline playhead?
        /// </summary>
        public static bool StartFromTimelinePlayhead { get; set; }

        /// <summary>
        ///     The time of the timeline playhead, for starting the level from the playhead to speed up testing.
        /// </summary>
        public static double TimelinePlayheadTime { get; set; }

        /// <summary>
        ///     Should we load directly into the currently edited beatmap on play
        /// </summary>
        public static bool PlayFromEditedBeatmap { get; set; }

        public static BeatmapConfigSo CurrentEditingBeatmapConfig { get; private set; }

        public static void SetBeatmapConfig(BeatmapConfigSo beatmapConfig)
        {
            CurrentEditingBeatmapConfig = beatmapConfig;
        }
    }
}