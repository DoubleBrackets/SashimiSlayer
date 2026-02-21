using Core.Scene;
using GlobalUI.PauseMenu;
using GlobalUI.TransitionUI;
using Menus.LevelSelect;
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
    }
}