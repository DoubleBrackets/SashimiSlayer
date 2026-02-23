using UI.Screens.LevelSelect;
using UnityEditor;
using UnityEngine;

namespace DevTools.Editor.SashimiSlayer
{
    /// <summary>
    ///     Holds editor preferences for <see cref="SashimiSlayerToolsWindow" />
    /// </summary>
    [FilePath("UserSettings/SashimiDevToolPrefs.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SashimiDevToolPrefs : ScriptableSingleton<SashimiDevToolPrefs>
    {
        [SerializeField]
        private SongRosterSO _songRoster;

        public SongRosterSO SongRoster
        {
            get => _songRoster;
            set
            {
                if (_songRoster == value)
                {
                    return;
                }

                _songRoster = value;
                Save(true);
            }
        }

        [SerializeField]
        private bool _autoRefreshTimeline;

        public bool AutoRefreshTimeline
        {
            get => _autoRefreshTimeline;
            set
            {
                if (_autoRefreshTimeline == value)
                {
                    return;
                }

                _autoRefreshTimeline = value;
                Save(true);
            }
        }

        [SerializeField]
        private bool _playFromEditedBeatmap;

        public bool PlayFromEditedBeatmap
        {
            get => _playFromEditedBeatmap;
            set
            {
                if (_playFromEditedBeatmap == value)
                {
                    return;
                }

                _playFromEditedBeatmap = value;
                Save(true);
            }
        }

        [SerializeField]
        private bool _startFromTimelinePlayhead;

        public bool StartFromTimelinePlayhead
        {
            get => _startFromTimelinePlayhead;
            set
            {
                if (_startFromTimelinePlayhead == value)
                {
                    return;
                }

                _startFromTimelinePlayhead = value;
                Save(true);
            }
        }

        public void SaveThis()
        {
            Save(true);
        }
    }
}