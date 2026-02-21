using UnityEditor;
using UnityEngine;

namespace DevTools.Editor.StartupSceneTool
{
    /// <summary>
    ///     Holds editor preferences for startup scene configuration
    /// </summary>
    [FilePath("UserSettings/EditorStartupScenePrefs.asset", FilePathAttribute.Location.ProjectFolder)]
    public class EditorStartupScenePrefs : ScriptableSingleton<EditorStartupScenePrefs>
    {
        [field: SerializeField]
        public string StartupScenePath { get; set; }

        public void SaveThis()
        {
            Save(true);
        }
    }
}