using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace DevTools.Editor.StartupSceneTool
{
    /// <summary>
    ///     Editor window for configuring the always-startup scene for play mode
    /// </summary>
    [InitializeOnLoad]
    public class EditorStartupSceneWindow : EditorWindow
    {
        private const string DefaultStartupScenePath = "Assets/Scenes/BaseScene.unity";

        private EditorStartupScenePrefs _prefs;

        [MenuItem("Sashimi Slayer/Startup Scene Configuration")]
        public static void ShowWindow()
        {
            GetWindow<EditorStartupSceneWindow>("Startup Scene");
        }

        /// <summary>
        ///     This runs on editor load because of [InitializeOnLoad]
        /// </summary>
        static EditorStartupSceneWindow()
        {
            InitializeStartupScenePath();
        }

        private void OnEnable()
        {
            _prefs = EditorStartupScenePrefs.instance;
            EditorApplication.wantsToQuit += HandleWantsToQuit;
        }

        private void OnDisable()
        {
            EditorApplication.wantsToQuit -= HandleWantsToQuit;
        }

        private static void InitializeStartupScenePath()
        {
            EditorStartupScenePrefs prefs = EditorStartupScenePrefs.instance;

            if (string.IsNullOrEmpty(prefs.StartupScenePath))
            {
                prefs.StartupScenePath = DefaultStartupScenePath;
                Debug.Log($"Set startup scene to default: {DefaultStartupScenePath}");
            }

            UpdateStartupScene(prefs.StartupScenePath);
        }

        private bool HandleWantsToQuit()
        {
            _prefs.SaveThis();
            return true;
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Startup Scene Configuration", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUILayout.HelpBox(
                "The startup scene is the scene that will be loaded when entering Play Mode in the editor. " +
                "The game must bootstrap from this scene to work properly.",
                MessageType.Info);

            EditorGUILayout.Space(10);

            string currentStartupScenePath = _prefs.StartupScenePath;
            string startupScenePath = EditorGUILayout.TextField("Startup Scene Path", _prefs.StartupScenePath);

            if (startupScenePath != currentStartupScenePath)
            {
                _prefs.StartupScenePath = startupScenePath;
                UpdateStartupScene(startupScenePath);
            }

            EditorGUILayout.Space(5);

            // Show object field for easier scene selection
            SceneAsset currentScene = string.IsNullOrEmpty(_prefs.StartupScenePath)
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneAsset>(_prefs.StartupScenePath);

            var newScene = (SceneAsset)EditorGUILayout.ObjectField(
                "Startup Scene",
                currentScene,
                typeof(SceneAsset),
                false);

            if (newScene != currentScene)
            {
                string newPath = newScene != null ? AssetDatabase.GetAssetPath(newScene) : "";
                _prefs.StartupScenePath = newPath;
                UpdateStartupScene(newPath);
            }

            EditorGUILayout.Space(10);

            if (GUILayout.Button("Reset to Default"))
            {
                _prefs.StartupScenePath = DefaultStartupScenePath;
                UpdateStartupScene(DefaultStartupScenePath);
            }

            EditorGUILayout.Space(5);

            // Show current status
            SceneAsset playModeStartScene = EditorSceneManager.playModeStartScene;
            string statusMessage = playModeStartScene != null
                ? $"Current Play Mode Start Scene: {playModeStartScene.name}"
                : "No Play Mode Start Scene set";

            EditorGUILayout.HelpBox(statusMessage, MessageType.None);
        }

        /// <summary>
        ///     Set the startup scene for the editor.
        ///     The game must bootstrap from this scene to work.
        /// </summary>
        /// <param name="startupScenePath"></param>
        private static void UpdateStartupScene(string startupScenePath)
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
            else
            {
                EditorSceneManager.playModeStartScene = null;
                Debug.Log("Cleared startup scene");
            }
        }
    }
}