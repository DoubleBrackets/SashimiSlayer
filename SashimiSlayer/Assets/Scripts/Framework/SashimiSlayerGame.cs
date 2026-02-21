using Beatmapping.Tooling;
using Core.Protag;
using Core.Scene;
using Cysharp.Threading.Tasks;
using DevTools;
using EditorUtils.BoldHeader;
using FMODUnity;
using Framework.Services;
using GameInput;
using GameInput.Interface;
using GlobalUI;
using GlobalUI.PauseMenu;
using GlobalUI.TransitionUI;
using Menus.LevelSelect;
using NaughtyAttributes;
using Saving;
using UnityEngine;

namespace Framework
{
    /// <summary>
    ///     Top level game orchestrator, pseudo-entry point for the game
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class SashimiSlayerGame : MonoBehaviour
    {
        [BoldHeader("SashimiSlayerGame")]
        [InfoBox("Top level game orchestrator, pseudo-entry point for the game")]
        [Header("Startup Config")]

        [SerializeField]
        private StartupConfigSO _startupConfigSO;

        [Header("Global UI")]

        [SerializeField]
        private Transform _globalUIParentTransform;

        [Header("Preloading")]

        [SerializeField]
        private StudioBankLoader _studioBankLoader;

        [Header("Services")]

        [SerializeField]
        private LevelLoader _levelLoader;

        [SerializeField]
        private InputService _userInput;

        [SerializeField]
        private ScreenShakeService _screenShakeService;

        [SerializeField]
        private DebugService _debugService;

        [SerializeField]
        private Protaganist _protaganist;

        private ServiceLocator _serviceLocator;
        private SaveService _saveService;

        private PauseMenuController _pauseMenuController;
        private SceneTransitionVisuals _sceneTransitionVisuals;

        private void Awake()
        {
            BeginGame().Forget();
        }

        private void OnDestroy()
        {
            _studioBankLoader.Unload();
        }

        private async UniTaskVoid BeginGame()
        {
            _serviceLocator = new ServiceLocator();
            _saveService = new SaveService();

            GlobalUIFactory globalUIFactory = new(_startupConfigSO,
                _saveService,
                _userInput,
                _screenShakeService,
                _globalUIParentTransform);

            globalUIFactory.CreateGlobalUI(out _pauseMenuController, out _sceneTransitionVisuals);

            _levelLoader.Initialize(_sceneTransitionVisuals);

            _protaganist.Initialize(_userInput, _screenShakeService);

            // Register the ones that need to be accessed globally
            _serviceLocator.RegisterService(_levelLoader);
            _serviceLocator.RegisterService(_saveService);
            _serviceLocator.RegisterService<IUserInput>(_userInput);
            _serviceLocator.RegisterService(_screenShakeService);
            _serviceLocator.RegisterService(_debugService);
            _serviceLocator.RegisterService(_protaganist);

            Debug.Log("Loading starting banks");
            _studioBankLoader.Load();

            // Load all the global banks
            await UniTask.WaitUntil(() => RuntimeManager.HaveAllBanksLoaded);
            await UniTask.WaitUntil(() => !RuntimeManager.AnySampleDataLoading());

            Debug.Log("All banks loaded");

            // Load startup level
            GameLevelSO startupLevel = GetStartupLevel(_startupConfigSO);

            if (startupLevel == null)
            {
                Debug.LogError("No startup level found.");
                return;
            }

            _levelLoader.LoadLevel(startupLevel).Forget();
        }

        private GameLevelSO GetStartupLevel(StartupConfigSO startupConfig)
        {
            if (startupConfig == null || startupConfig.InitialGameLevel == null)
            {
                Debug.LogError("No initial level found in StartupConfigSO.");
                return null;
            }

            SongRosterSO songRoster = startupConfig.SongRoster;

            if (BeatmappingEditingSettings.PlayFromEditedBeatmap)
            {
                return songRoster.Songs.Find(song =>
                    song.NormalBeatmap == BeatmappingEditingSettings.CurrentEditingBeatmapConfig);
            }

            return startupConfig.InitialGameLevel;
        }
    }
}