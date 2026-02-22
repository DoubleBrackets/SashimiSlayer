using Beatmapping;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using DevTools.Editor.StartupSceneTool;
using Framework;
using Framework.LevelLoading;
using GameInput.SerialComm;
using Saving;
using UI.Screens.LevelSelect;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace DevTools.Editor.SashimiSlayer
{
    /// <summary>
    ///     All-in-one dev tool editor window for this game
    /// </summary>
    public class SashimiSlayerToolsWindow : EditorWindow
    {
        private static TimelineAsset currentEditingTimeline;
        private static BeatmapConfigSo currentEditingBeatmap;

        public static BeatmapConfigSo CurrentEditingBeatmap => currentEditingBeatmap;

        private SashimiDevToolPrefs _prefs;

        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            Debug.Log("SashimiSlayerToolsWindow Enabled");

            _prefs = SashimiDevToolPrefs.instance;

            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorApplication.wantsToQuit += HandleWantsToQuit;
            EditorSceneManager.sceneOpened += HandleSceneOpened;
        }

        private bool HandleWantsToQuit()
        {
            _prefs.SaveThis();
            return true;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            EditorApplication.wantsToQuit -= HandleWantsToQuit;
            EditorSceneManager.sceneOpened -= HandleSceneOpened;
        }

        /// <summary>
        ///     Try to select the beatmap from the scene when loading it in, for convenience
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        private void HandleSceneOpened(Scene scene, OpenSceneMode mode)
        {
            TimelineAsset timelineAsset = SelectTimelineFromScene();
            if (timelineAsset != null)
            {
                SelectBeatmapFromTimeline(timelineAsset);
            }
        }

        private void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            SashimiEditorGUIUtils.DrawBoldHeaderWithLayout("Basic Setup");

            DrawBasicSetupControls();

            SashimiEditorGUIUtils.DrawBoldHeaderWithLayout("Beatmapping");

            DrawBeatmappingControls();

            SashimiEditorGUIUtils.DrawBoldHeaderWithLayout("Misc");

            DrawMiscSettingsControls();

            EditorGUILayout.EndScrollView();
        }

        private void DrawBasicSetupControls()
        {
            if (GUILayout.Button("Open Startup Scene Configuration"))
            {
                EditorStartupSceneWindow.ShowWindow();
            }
        }

        private void DrawBeatmappingControls()
        {
            _prefs.SongRoster =
                (SongRosterSO)EditorGUILayout.ObjectField(
                    "Song Roster",
                    _prefs.SongRoster,
                    typeof(SongRosterSO),
                    false);

            if (GUILayout.Button("Select Beatmap Timeline (Shift+W)"))
            {
                TimelineAsset timelineAsset = SelectTimelineFromScene();
                SelectBeatmapFromTimeline(timelineAsset);
            }

            if (GUILayout.Button("Refresh Timeline (Shift+R)"))
            {
                RefreshTimelineEditor();
            }

            _prefs.AutoRefreshTimeline = GUILayout.Toggle(_prefs.AutoRefreshTimeline, "Auto Refresh Timeline");

            string currentBeatmapName = BeatmappingEditingSettings.CurrentEditingBeatmapConfig != null
                ? BeatmappingEditingSettings.CurrentEditingBeatmapConfig.name
                : "None";

            _prefs.PlayFromEditedBeatmap = GUILayout.Toggle(_prefs.PlayFromEditedBeatmap,
                $"Start Play from Edited Beatmap ({currentBeatmapName})");

            _prefs.StartFromTimelinePlayhead = GUILayout.Toggle(_prefs.StartFromTimelinePlayhead,
                $"Start Level From Timeline Playhead ({BeatmappingEditingSettings.TimelinePlayheadTime.ToString()})");

            // Sync to beatmap editing settings
            BeatmappingEditingSettings.StartFromTimelinePlayhead = _prefs.StartFromTimelinePlayhead;
            BeatmappingEditingSettings.PlayFromEditedBeatmap = _prefs.PlayFromEditedBeatmap;
        }

        private void DrawMiscSettingsControls()
        {
            SwordSerialReader.LogPackets =
                GUILayout.Toggle(SwordSerialReader.LogPackets, "Log Serial Comms Sword Packets");

            if (GUILayout.Button("Wipe All Highscores"))
            {
                new SaveService().WipeHighScore();
            }

            if (GUILayout.Button("Open Persistent Data Path"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }

            if (EventSystem.current)
            {
                GameObject selected = EventSystem.current.currentSelectedGameObject;

                // full hierarchy path
                var path = "";

                while (selected != null)
                {
                    path = $"{selected.name}/{path}";
                    Transform parent = selected.transform.parent;
                    selected = parent != null ? parent.gameObject : null;
                }

                GUILayout.Label($"UI Selected: {path}");
            }
        }

        private void SelectBeatmapFromTimeline(TimelineAsset timeline)
        {
            BeatmapConfigSo matchingMap = GetBeatmapFromTimeline(timeline);
            if (matchingMap == null)
            {
                Debug.LogWarning($"No beatmap found for timeline {timeline.name}.");
                return;
            }

            Debug.Log(
                $"Found beatmap {matchingMap.name} for timeline {timeline.name}, setting as current editing beatmap.");

            currentEditingBeatmap = matchingMap;
            currentEditingTimeline = timeline;
            BeatmappingEditingSettings.SetCurrentlyEditingBeatmap(currentEditingBeatmap);
        }

        /// <summary>
        ///     Find the beatmap that contains the given timeline
        /// </summary>
        /// <param name="timeline"></param>
        /// <returns></returns>
        private static BeatmapConfigSo GetBeatmapFromTimeline(TimelineAsset timeline)
        {
            if (timeline == currentEditingTimeline)
            {
                return currentEditingBeatmap;
            }

            foreach (GameLevelSO track in SashimiDevToolPrefs.instance.SongRoster.Songs)
            {
                BeatmapConfigSo map = track.NormalBeatmap;

                var didMatchMap = false;
                BeatmapConfigSo matchingMap = null;

                if (map.BeatmapTimeline == timeline)
                {
                    didMatchMap = true;
                    matchingMap = map;
                }

                if (didMatchMap)
                {
                    return matchingMap;
                }
            }

            Debug.LogWarning($"No beatmap found for timeline {timeline.name}.");

            return null;
        }

        [MenuItem("Sashimi Slayer/Sashimi Slayer Tools Window", false, 1)]
        public static void ShowWindow()
        {
            GetWindow<SashimiSlayerToolsWindow>("Sashimi Slayer Tools");
        }

        [MenuItem("Sashimi Slayer/Refresh Timeline Editor Window #r")]
        public static void RefreshTimelineEditor()
        {
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }

        [MenuItem("Sashimi Slayer/Select Normal #w")]
        private static void SelectTimelineFromSceneNormal()
        {
            SelectTimelineFromScene();
        }

        private static TimelineAsset SelectTimelineFromScene()
        {
            // Search the current scene for a playable director
            var quickSelect = FindObjectOfType<TimelineQuickSelect>();

            if (quickSelect == null)
            {
                Debug.LogWarning("No PlayableDirector found in scene");
                return null;
            }

            // Select it
            Selection.activeGameObject = quickSelect.LoadMap().gameObject;
            RefreshTimelineEditor();
            return quickSelect.CurrentTimeline;
        }

        private static void CleanupNotes()
        {
            BeatNote[] notes = FindObjectsByType<BeatNote>(FindObjectsSortMode.None);
            Debug.Log($"Cleaning up {notes.Length} notes");
            foreach (BeatNote note in notes)
            {
                DestroyImmediate(note.gameObject);
            }
        }

        /// <summary>
        ///     Hook on play mode state to clean up notes before entering play mode
        /// </summary>
        /// <param name="param"></param>
        private void HandlePlayModeChanged(PlayModeStateChange param)
        {
            if (param == PlayModeStateChange.ExitingPlayMode)
            {
                CleanupNotes();
            }
        }
    }
}