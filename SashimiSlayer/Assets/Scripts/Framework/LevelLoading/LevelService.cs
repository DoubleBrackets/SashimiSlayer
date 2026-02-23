using System;
using System.Collections.Generic;
using Beatmapping;
using Beatmapping.Events;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorUtils.BoldHeader;
using Events.Basic;
using FMODUnity;
using NaughtyAttributes;
using UI.Screens.TransitionUI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework.LevelLoading
{
    public class LevelService : MonoBehaviour
    {
        [BoldHeader("Level Loader")]
        [InfoBox("Handles changing levels & scenes")]
        [Header("Events (Out)")]

        [SerializeField]
        private BeatmapEvent _beatmapStartEvent;

        [SerializeField]
        private BeatmapEvent _beatmapUnloadEvent;

        [SerializeField]
        private VoidEvent _onEnterMenuEvent;

        [SerializeField]
        private VoidEvent _onExitMenuEvent;

        public event Action<BeatmapConfigSo> BeatmapLevelDoneLoadingEvent;

        private GameLevelSO CurrentLevel { get; set; }

        private string _currentLevelSceneName = string.Empty;
        private GameLevelSO _previousBeatmapLevel;

        /// <summary>
        ///     Queue for levels to be loaded, in case a transition is interrupted by another level load
        /// </summary>
        private Queue<GameLevelSO> _levelLoadQueue = new();

        private bool _isLoading;

        private SceneTransitionVisuals _sceneTransitionVisuals;

        public void Initialize()
        {
            _isLoading = false;
        }

        public void SetSceneTransitionVisuals(SceneTransitionVisuals sceneTransitionVisuals)
        {
            _sceneTransitionVisuals = sceneTransitionVisuals;
        }

        private void OnDestroy()
        {
            Debug.Log($"Killed {DOTween.KillAll()} tweens");
        }

        /// <summary>
        ///     Loads a new level, unloading the current one if necessary
        /// </summary>
        /// <param name="levelToLoad"></param>
        public async UniTask LoadLevel(GameLevelSO levelToLoad)
        {
            if (_isLoading)
            {
                if (_levelLoadQueue.Count < 1)
                {
                    Debug.Log("Level is already loading, adding to queue");
                    _levelLoadQueue.Enqueue(levelToLoad);
                }
                else
                {
                    Debug.LogWarning("Too many queued loads, ignoring load request");
                }

                return;
            }

            _isLoading = true;

            if (_sceneTransitionVisuals)
            {
                _sceneTransitionVisuals.SetTitleText(levelToLoad.LevelTitle);
                await _sceneTransitionVisuals.FadeOut();
            }

            Debug.Log($"Killed {DOTween.KillAll()} tweens");

            // Unload and load banks
            // We block until everything is loaded to avoid latency and
            // prevent issues when the same bank is loaded and unloaded
            if (CurrentLevel != null && CurrentLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
            {
                UnloadBanks(CurrentLevel.FmodBanksToPreLoad);
            }

            if (levelToLoad.LevelType == GameLevelSO.LevelTypes.Gameplay)
            {
                await LoadBanks(levelToLoad.FmodBanksToPreLoad);
            }

            // Unload the current scene
            string sceneName = levelToLoad.GameSceneName;
            if (SceneManager.GetSceneByName(_currentLevelSceneName).isLoaded)
            {
                await SceneManager.UnloadSceneAsync(_currentLevelSceneName);

                if (CurrentLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
                {
                    _beatmapUnloadEvent.Raise(levelToLoad.NormalBeatmap);
                }

                if (CurrentLevel.LevelType == GameLevelSO.LevelTypes.MainMenu
                    && levelToLoad.LevelType != GameLevelSO.LevelTypes.MainMenu)
                {
                    _onExitMenuEvent.Raise();
                }
            }

            // Load the new scene
            Debug.Log($"Loading scene {sceneName}");
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            _currentLevelSceneName = sceneName;
            GameLevelSO previousLevel = CurrentLevel;
            CurrentLevel = levelToLoad;

            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            if (levelToLoad.LevelType == GameLevelSO.LevelTypes.Gameplay)
            {
                _beatmapStartEvent.Raise(levelToLoad.NormalBeatmap);
                BeatmapLevelDoneLoadingEvent?.Invoke(levelToLoad.NormalBeatmap);
                _previousBeatmapLevel = levelToLoad;
            }

            bool wasInMainMenu = previousLevel != null && previousLevel.LevelType == GameLevelSO.LevelTypes.MainMenu;
            if (levelToLoad.LevelType == GameLevelSO.LevelTypes.MainMenu
                && !wasInMainMenu)
            {
                _onEnterMenuEvent.Raise();
            }

            if (_sceneTransitionVisuals)
            {
                await _sceneTransitionVisuals.FadeIn();
            }

            _isLoading = false;

            // Chain with any queued level loads
            if (_levelLoadQueue.Count > 0)
            {
                GameLevelSO nextLevel = _levelLoadQueue.Dequeue();
                Debug.Log($"Loading next level from queue: {nextLevel.LevelTitle}");
                await LoadLevel(nextLevel);
            }
        }

        public void LoadLastBeatmapLevel()
        {
            if (_previousBeatmapLevel == null)
            {
                Debug.LogWarning("No previous beatmap level to load");
                return;
            }

            LoadLevel(_previousBeatmapLevel).Forget();
        }

        private async UniTask LoadBanks(List<string> banks, bool waitForFinish = true)
        {
            foreach (string bankRef in banks)
            {
                try
                {
                    Debug.Log($"Loading bank {bankRef}");
                    // Preload sample data to avoid latency on play
                    RuntimeManager.LoadBank(bankRef, true);
                }
                catch (BankLoadException e)
                {
                    RuntimeUtils.DebugLogException(e);
                }
            }

            if (waitForFinish)
            {
                await UniTask.WaitUntil(() => RuntimeManager.HaveAllBanksLoaded);
                await UniTask.WaitUntil(() => !RuntimeManager.AnySampleDataLoading());
            }
        }

        private void UnloadBanks(List<string> banks)
        {
            foreach (string bankRef in banks)
            {
                Debug.Log($"Unloading bank {bankRef}");
                RuntimeManager.UnloadBank(bankRef);
            }
        }
    }
}