using Menus.LevelSelect;
using UnityEditor;
using UnityEngine;

namespace DevTools.Editor
{
    /// <summary>
    ///     Holds editor preferences for <see cref="SashimiSlayerToolsWindow" />
    /// </summary>
    [FilePath("UserSettings/SashimiDevToolPrefs.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SashimiDevToolPrefs : ScriptableSingleton<SashimiDevToolPrefs>
    {
        [field: SerializeField]
        public string StartupScenePath { get; set; }

        [field: SerializeField]
        public SongRosterSO SongRoster { get; set; }

        [field: SerializeField]
        public bool AutoRefreshTimeline { get; set; }

        [field: SerializeField]
        public bool PlayFromEditedBeatmap { get; set; }

        [field: SerializeField]
        public bool StartFromTimelinePlayhead { get; set; }

        public void SaveThis()
        {
            Save(true);
        }
    }
}