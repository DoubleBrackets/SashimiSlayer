using Framework.LevelLoading;
using UI.Screens.LevelSelect;
using UI.Screens.PauseMenu;
using UI.Screens.TransitionUI;
using UnityEngine;

namespace Framework
{
    /// <summary>
    ///     Holds configuration for the initial game configuration
    /// </summary>
    [CreateAssetMenu(fileName = "StartupConfigSO", menuName = "StartupConfigSO", order = 51)]
    public class StartupConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public GameLevelSO InitialGameLevel { get; private set; }

        [field: SerializeField]
        public SongRosterSO SongRoster { get; private set; }

        [field: Header("Global UI Prefabs")]

        [field: SerializeField]
        public PauseMenuController PauseMenuPrefab { get; private set; }

        [field: SerializeField]
        public SceneTransitionVisuals SceneTransitionVisualsPrefab { get; private set; }

        public GameLevelSO GetStartupLevel()
        {
            if (this == null || InitialGameLevel == null)
            {
                Debug.LogError("No initial level found in StartupConfigSO.");
                return null;
            }

            SongRosterSO songRoster = SongRoster;

            if (BeatmappingEditingSettings.PlayFromEditedBeatmap)
            {
                return songRoster.Songs.Find(song =>
                    song.NormalBeatmap == BeatmappingEditingSettings.CurrentEditingBeatmapConfig);
            }

            return InitialGameLevel;
        }
    }
}