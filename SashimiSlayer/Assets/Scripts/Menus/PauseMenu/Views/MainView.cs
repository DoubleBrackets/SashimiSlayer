using Beatmapping;
using Core.Scene;
using Events.Core;
using UnityEngine;
using Button = UnityEngine.UI.Button;

namespace Menus.PauseMenu.Views
{
    public class MainView : PauseMenuView
    {
        [Header("Events (In)")]

        [SerializeField]
        private BeatmapEvent _beatmapLoadedEvent;

        [SerializeField]
        private BeatmapEvent _beatmapUnloadedEvent;

        [Header("Depends")]

        [SerializeField]
        private LevelLoaderMono _levelLoader;

        [SerializeField]
        private Button _restartButton;

        private BeatmapConfigSo _currentBeatmap;

        public override void ViewDestroy()
        {
            _beatmapLoadedEvent.RemoveListener(OnBeatmapLoaded);
            _beatmapUnloadedEvent.RemoveListener(OnBeatmapUnloaded);
            _restartButton.onClick.RemoveListener(RestartLevel);
        }

        public override void ViewAwake()
        {
            _beatmapLoadedEvent.AddListener(OnBeatmapLoaded);
            _beatmapUnloadedEvent.AddListener(OnBeatmapUnloaded);
            _restartButton.onClick.AddListener(RestartLevel);
            _restartButton.interactable = false;
        }

        private void OnBeatmapLoaded(BeatmapConfigSo beatmap)
        {
            _currentBeatmap = beatmap;
            _restartButton.interactable = true;
        }

        private void OnBeatmapUnloaded()
        {
            _currentBeatmap = null;
            _restartButton.interactable = false;
        }

        private void RestartLevel()
        {
            _levelLoader.LoadLastBeatmapLevel();
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}