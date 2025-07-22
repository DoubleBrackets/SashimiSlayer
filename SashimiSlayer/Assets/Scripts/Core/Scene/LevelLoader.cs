using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorUtils.BoldHeader;
using Events.Core;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core.Scene
{
    public class LevelLoader : MonoBehaviour
    {
        [BoldHeader("Level Loader")]
        [InfoBox("Handles changing levels & scenes")]
        [Header("Depends")]

        [SerializeField]
        private SceneTransitionVisuals sceneTransitionVisuals;

        [Header("Events (Out)")]

        [SerializeField]
        private BeatmapEvent _beatmapStartEvent;

        [SerializeField]
        private BeatmapEvent _beatmapUnloadEvent;

        public static LevelLoader Instance { get; private set; }

        public GameLevelSO CurrentLevel { get; private set; }

        private string _currentLevelSceneName = string.Empty;
        private GameLevelSO _previousBeatmapLevel;

        /// <summary>
        ///     Queue for levels to be loaded, in case a transition is interrupted by another level load
        /// </summary>
        private Queue<GameLevelSO> _levelLoadQueue = new();

        private bool _isLoading;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
#if UNITY_EDITOR
            if (SceneManager.sceneCount > 1)
            {
                _currentLevelSceneName = SceneManager.GetSceneAt(1).name;
            }
#endif
        }

        private void OnDestroy()
        {
            Debug.Log($"Killed {DOTween.KillAll()} tweens");
        }

        public async UniTask LoadLevel(GameLevelSO gameLevel)
        {
            if (_isLoading)
            {
                if (_levelLoadQueue.Count < 1)
                {
                    Debug.Log("Level is already loading, adding to queue");
                    _levelLoadQueue.Enqueue(gameLevel);
                }
                else
                {
                    Debug.LogWarning("Too many queued loads, ignoring load request");
                }

                return;
            }

            _isLoading = true;

            sceneTransitionVisuals.SetTitleText(gameLevel.LevelTitle);

            await sceneTransitionVisuals.FadeOut();
            Debug.Log($"Killed {DOTween.KillAll()} tweens");

            // Unload and load banks
            // We block until everything is loaded to avoid latency and
            // prevent issues when the same bank is loaded and unloaded
            if (CurrentLevel != null && CurrentLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
            {
                UnloadBanks(CurrentLevel.FmodBanksToPreLoad);
            }

            if (gameLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
            {
                await LoadBanks(gameLevel.FmodBanksToPreLoad);
            }

            // Unload the current scene
            string sceneName = gameLevel.GameSceneName;
            if (SceneManager.GetSceneByName(_currentLevelSceneName).isLoaded)
            {
                await SceneManager.UnloadSceneAsync(_currentLevelSceneName);
                _beatmapUnloadEvent.Raise(CurrentLevel.Beatmap);
            }

            // Load the new scene
            Debug.Log($"Loading scene {sceneName}");
            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            _currentLevelSceneName = sceneName;
            CurrentLevel = gameLevel;
            SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));

            if (gameLevel.LevelType == GameLevelSO.LevelTypes.Gameplay)
            {
                _beatmapStartEvent.Raise(gameLevel.Beatmap);
                _previousBeatmapLevel = gameLevel;
            }

            await sceneTransitionVisuals.FadeIn();

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