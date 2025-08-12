using System;
using System.IO;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using Core.Scene;
using GameInput;
using Menus.LevelSelect;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

namespace Beatmapping.Editor
{
    public class SashimiSlayerUtilWindow : EditorWindow
    {
        private const string PrefsPath = "Assets/Settings/Editor/User/SimpleUtilsPrefs.asset";
        private const string _levelRosterPref = "BeatmapEditorWindow.levelRosterSO";
        private static SongRosterSO songRoster;

        // Caching for mapping timeline to beatmap
        private static TimelineAsset _currentEditingTimeline;
        private static BeatmapConfigSo _currentEditingBeatmap;

        private static string _lastEditedScenePath = string.Empty;

        public static bool AutoRefreshTimeline { get; private set; } = true;

        // Convenience property for easily getting the correct beatmap matching the current timeline
        public static BeatmapConfigSo CurrentEditingBeatmap => GetBeatmapFromTimeline(TimelineEditor.inspectedAsset);
        private UtilsPrefs _prefs;

        private void OnGUI()
        {
            if (GUILayout.Button("Play Game"))
            {
                PlayGame();
            }

            DrawBeatmapRosterField();

            GUILayout.Space(10);
            GUILayout.Label("Beatmap Utils", EditorStyles.boldLabel);

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

            AutoRefreshTimeline = GUILayout.Toggle(AutoRefreshTimeline, "Toggle Auto Refresh Timeline");

            BeatmappingUtilities.ProtagInvincible =
                GUILayout.Toggle(BeatmappingUtilities.ProtagInvincible, "Toggle Protagonist Invincible");

            BeatmappingUtilities.StartFromTimelinePlayhead =
                GUILayout.Toggle(BeatmappingUtilities.StartFromTimelinePlayhead,
                    $"Start Level From Timeline Playhead ({BeatmappingUtilities.TimelinePlayheadTime.ToString()})");

            BeatmapConfigSo currentBeatmap = BeatmappingUtilities.CurrentEditingBeatmapConfig;
            BeatmappingUtilities.PlayFromEditedBeatmap = GUILayout.Toggle(BeatmappingUtilities.PlayFromEditedBeatmap,
                $"Play from Edited Beatmap ({(currentBeatmap != null ? currentBeatmap.name : "None")})");

            SwordSerialReader.LogPackets = GUILayout.Toggle(SwordSerialReader.LogPackets, "Log Sword Packets");

            GUILayout.Space(10);
            GUILayout.Label("Save Data", EditorStyles.boldLabel);

            if (GUILayout.Button("Wipe All Highscores"))
            {
                songRoster.WipeHighScores();
            }

            if (GUILayout.Button("Open Persistent Data Path"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
        }

        private void OnBecameInvisible()
        {
            EditorApplication.playModeStateChanged -= ModeChanged;
        }

        private void OnBecameVisible()
        {
            _prefs = AssetDatabase.LoadAssetAtPath<UtilsPrefs>(PrefsPath);
            if (_prefs == null)
            {
                // Create directories if needed
                string directoryPath = Path.GetDirectoryName(PrefsPath);

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                _prefs = CreateInstance<UtilsPrefs>();
                AssetDatabase.CreateAsset(_prefs, PrefsPath);
                AssetDatabase.SaveAssets();
            }

            EditorApplication.playModeStateChanged += ModeChanged;

            // Load editing beatmap from prefs
            songRoster = AssetDatabase.LoadAssetAtPath<SongRosterSO>(
                EditorPrefs.GetString(_levelRosterPref, string.Empty));
        }

        /// <summary>
        ///     Find the correct beatmap that matches a timeline.
        /// </summary>
        /// <param name="timeline"></param>
        /// <returns></returns>
        private static BeatmapConfigSo GetBeatmapFromTimeline(TimelineAsset timeline)
        {
            if (timeline == _currentEditingTimeline)
            {
                return _currentEditingBeatmap;
            }

            foreach (GameLevelSO track in songRoster.Songs)
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
                    _currentEditingBeatmap = matchingMap;
                    _currentEditingTimeline = timeline;
                    BeatmappingUtilities.SetBeatmapConfig(_currentEditingBeatmap);
                    return matchingMap;
                }
            }

            Debug.LogWarning($"No beatmap found for timeline {timeline.name}.");

            return null;
        }

        [MenuItem("Sashimi Slayer/Sashimi Slayer Tool Window")]
        public static void ShowWindow()
        {
            GetWindow<SashimiSlayerUtilWindow>("Sashimi Slayer Tools");
        }

        [MenuItem("Sashimi Slayer/Refresh Timeline Editor Window #r")]
        public static void RefreshTimelineEditor()
        {
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
        }

        [MenuItem("Sashimi Slayer/Open Current Beatmap Timeline #w")]
        public static void SelectTimelineFromScene(LevelLoader.Difficulty difficulty)
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
        }

        /// <summary>
        ///     Play from the startup scene, and save the current scene for when we exit play mode
        /// </summary>
        private void PlayGame()
        {
            string startupScenePath = _prefs.StartupScenePath;

            _lastEditedScenePath = SceneManager.GetActiveScene().path;
            Debug.Log(_lastEditedScenePath);

            CleanupNotes();

            EditorSceneManager.MarkAllScenesDirty();
            EditorSceneManager.SaveOpenScenes();

            if (!SceneManager.GetSceneByPath(startupScenePath).isLoaded)
            {
                EditorSceneManager.OpenScene(startupScenePath, OpenSceneMode.Single);
            }

            EditorApplication.ExecuteMenuItem("Edit/Play");
        }

        public static void CleanupNotes()
        {
            BeatNote[] notes = FindObjectsByType<BeatNote>(FindObjectsSortMode.None);
            Debug.Log($"Cleaning up {notes.Length} notes");
            foreach (BeatNote note in notes)
            {
                DestroyImmediate(note.gameObject);
            }
        }

        private void ModeChanged(PlayModeStateChange param)
        {
            if (param == PlayModeStateChange.EnteredEditMode)
            {
                Debug.Log($"Loading last edited scene {_lastEditedScenePath}");
                try
                {
                    EditorSceneManager.OpenScene(_lastEditedScenePath, OpenSceneMode.Single);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            else if (param == PlayModeStateChange.EnteredPlayMode)
            {
            }
        }

        private void DrawBeatmapRosterField()
        {
            songRoster =
                (SongRosterSO)EditorGUILayout.ObjectField("Level Roster", songRoster, typeof(SongRosterSO),
                    false);

            EditorPrefs.SetString(_levelRosterPref,
                songRoster ? AssetDatabase.GetAssetPath(songRoster) : string.Empty);
        }
    }
}