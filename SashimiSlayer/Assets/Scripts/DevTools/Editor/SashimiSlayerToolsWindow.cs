using Beatmapping;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using Core.Scene;
using GameInput.SerialComm;
using Menus.LevelSelect;
using Saving;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;

namespace DevTools.Editor
{
    /// <summary>
    ///     All-in-one dev tool editor window for this game
    /// </summary>
    public class SashimiSlayerToolsWindow : EditorWindow
    {
        private const string DefaultStartupScenePath = "Assets/Scenes/BaseScene.unity";

        private static TimelineAsset currentEditingTimeline;
        private static BeatmapConfigSo currentEditingBeatmap;

        public static BeatmapConfigSo CurrentEditingBeatmap => GetBeatmapFromTimeline(TimelineEditor.inspectedAsset);

        private SashimiDevToolPrefs _prefs;

        private Vector2 _scrollPosition;

        private void OnEnable()
        {
            Debug.Log("SashimiSlayerToolsWindow Enabled");

            _prefs = SashimiDevToolPrefs.instance;

            if (string.IsNullOrEmpty(_prefs.StartupScenePath))
            {
                _prefs.StartupScenePath = DefaultStartupScenePath;
                Debug.Log($"Set startup scene to default: {DefaultStartupScenePath}");
            }

            UpdateStartupScene(_prefs.StartupScenePath);

            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            EditorApplication.wantsToQuit += HandleWantsToQuit;
        }

        private bool HandleWantsToQuit()
        {
            _prefs.SaveThis();
            return true;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorApplication.wantsToQuit -= HandleWantsToQuit;
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
            string currentStartupScenePath = _prefs.StartupScenePath;
            string startupScenePath = EditorGUILayout.TextField("Startup Scene", _prefs.StartupScenePath);

            if (startupScenePath != currentStartupScenePath)
            {
                _prefs.StartupScenePath = startupScenePath;
                UpdateStartupScene(startupScenePath);
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

            if (GUILayout.Button("Select Beatmap Timeline [Normal] (Shift+W)"))
            {
                SelectTimelineFromScene(LevelLoader.Difficulty.Normal);
            }

            if (GUILayout.Button("Select Beatmap Timeline [Hard] (Shift+E)"))
            {
                SelectTimelineFromScene(LevelLoader.Difficulty.Hard);
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

        /// <summary>
        ///     Find the beatmap that contains the given timeline.
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
                BeatmapConfigSo normalMap = track.NormalBeatmap;
                BeatmapConfigSo hardMap = track.HardBeatmap;

                var didMatchMap = false;
                BeatmapConfigSo matchingMap = null;

                if (normalMap.BeatmapTimeline == timeline)
                {
                    didMatchMap = true;
                    matchingMap = normalMap;
                }
                else if (hardMap && hardMap.BeatmapTimeline == timeline)
                {
                    didMatchMap = true;
                    matchingMap = hardMap;
                }

                if (didMatchMap)
                {
                    currentEditingBeatmap = matchingMap;
                    currentEditingTimeline = timeline;
                    BeatmappingEditingSettings.SetBeatmapConfig(currentEditingBeatmap);
                    return matchingMap;
                }
            }

            Debug.LogWarning($"No beatmap found for timeline {timeline.name}.");

            return null;
        }

        [MenuItem("Sashimi Slayer/Sashimi Slayer Tool Window")]
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
            SelectTimelineFromScene(LevelLoader.Difficulty.Normal);
        }

        [MenuItem("Sashimi Slayer/Select Hard #e")]
        private static void SelectTimelineFromSceneHard()
        {
            SelectTimelineFromScene(LevelLoader.Difficulty.Hard);
        }

        private static void SelectTimelineFromScene(LevelLoader.Difficulty difficulty)
        {
            // Search the current scene for a playable director
            var quickSelect = FindObjectOfType<TimelineQuickSelect>();

            if (quickSelect == null)
            {
                Debug.LogWarning("No PlayableDirector found in scene");
                return;
            }

            // Select it
            Selection.activeGameObject = quickSelect.gameObject;
            Selection.activeGameObject = quickSelect.LoadMap(difficulty).gameObject;
            RefreshTimelineEditor();
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
        ///     Set the startup scene for the editor.
        ///     The game must bootstrap from this scene to work.
        /// </summary>
        /// <param name="startupScenePath"></param>
        private void UpdateStartupScene(string startupScenePath)
        {
            if (!string.IsNullOrEmpty(startupScenePath))
            {
                var startupScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(startupScenePath);

                if (startupScene == null)
                {
                    Debug.LogWarning($"Startup scene not found: {startupScenePath}");
                    return;
                }

                EditorSceneManager.playModeStartScene = startupScene;

                Debug.Log($"Set startup scene to {startupScenePath}");
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