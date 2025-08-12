using System;
using System.IO;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using Core.Scene;
using GameInput;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using TrackRosterSO = Menus.LevelSelect.TrackRosterSO;

namespace Beatmapping.Editor
{
    public class SashimiSlayerUtilWindow : EditorWindow
    {
        private const string PrefsPath = "Assets/Settings/Editor/User/SimpleUtilsPrefs.asset";
        private const string _levelRosterPref = "BeatmapEditorWindow.levelRosterSO";
        private static TrackRosterSO trackRoster;

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

            if (GUILayout.Button("Select Beatmap Timeline (Shift+W)"))
            {
                SelectTimelineFromScene();
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
                trackRoster.WipeHighScores();
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
            trackRoster = AssetDatabase.LoadAssetAtPath<TrackRosterSO>(
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

            foreach (TrackRosterSO.TrackEntry track in trackRoster.Tracks)
            {
                GameLevelSO normalMap = track.NormalMap;
                GameLevelSO hardMap = track.HardMap;
                if (normalMap.Beatmap.BeatmapTimeline == timeline)
                {
                    _currentEditingTimeline = timeline;
                    _currentEditingBeatmap = normalMap.Beatmap;
                    BeatmappingUtilities.SetBeatmapConfig(_currentEditingBeatmap);
                    return normalMap.Beatmap;
                }

                if (hardMap.Beatmap.BeatmapTimeline == timeline)
                {
                    _currentEditingTimeline = timeline;
                    _currentEditingBeatmap = hardMap.Beatmap;
                    BeatmappingUtilities.SetBeatmapConfig(_currentEditingBeatmap);
                    return hardMap.Beatmap;
                }
            }

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
        public static void SelectTimelineFromScene()
        {
            // Search the current scene for a playable director
            var director = FindObjectOfType<PlayableDirector>();

            if (director == null)
            {
                Debug.LogWarning("No PlayableDirector found in scene");
                return;
            }

            // Select it
            Selection.activeGameObject = director.gameObject;
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
            trackRoster =
                (TrackRosterSO)EditorGUILayout.ObjectField("Level Roster", trackRoster, typeof(TrackRosterSO),
                    false);

            EditorPrefs.SetString(_levelRosterPref,
                trackRoster ? AssetDatabase.GetAssetPath(trackRoster) : string.Empty);
        }
    }
}