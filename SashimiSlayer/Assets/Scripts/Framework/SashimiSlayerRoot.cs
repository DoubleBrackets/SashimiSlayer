using Beatmapping.Service;
using Cysharp.Threading.Tasks;
using DevTools;
using EditorUtils.BoldHeader;
using FMODUnity;
using Framework.LevelLoading;
using GameInput;
using GameInput.Interface;
using GameJuice.ScreenShake;
using Interactions.Framework;
using NaughtyAttributes;
using Protag.Core;
using Saving;
using UI.Screens;
using UI.Screens.PauseMenu;
using UI.Screens.TransitionUI;
using UnityEngine;

namespace Framework
{
    /// <summary>
    ///     Game root, pseudo-entry point for the game
    /// </summary>
    [DefaultExecutionOrder(-10000)]
    public class SashimiSlayerRoot : MonoBehaviour
    {
        [BoldHeader("SashimiSlayerRoot")]
        [InfoBox("Game root, pseudo-entry point for the game")]
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
        private LevelService _levelService;

        [SerializeField]
        private InputService _userInput;

        [SerializeField]
        private BeatmapServiceContainerSO _beatmapServiceContainer;

        [SerializeField]
        private ScreenShakeService _screenShakeService;

        [SerializeField]
        private DebugService _debugService;

        [SerializeField]
        private Protaganist _protaganist;

        // Services
        private ServiceLocator _serviceLocator;
        private SaveService _saveService;
        private IInteractionService _interactionService;

        // Global UI
        private PauseMenuController _pauseMenuController;
        private SceneTransitionVisuals _sceneTransitionVisuals;

        private CoreGameplayController _gameplayController;

        private void Awake()
        {
            BeginGame().Forget();
        }

        private void OnDestroy()
        {
            _studioBankLoader.Unload();
            _gameplayController?.Cleanup();
        }

        private async UniTaskVoid BeginGame()
        {
            SetupServices();
            SetupGlobalUI();
            await SetupFMOD();

            _gameplayController = new CoreGameplayController(
                _startupConfigSO.ScoreScreenLevel,
                _levelService,
                _protaganist,
                _interactionService,
                _userInput,
                _beatmapServiceContainer
            );

            // Load startup level
            GameLevelSO startupLevel = _startupConfigSO.GetStartupLevel();

            if (startupLevel == null)
            {
                Debug.LogError("No startup level found.");
                return;
            }

            await _levelService.LoadLevel(startupLevel);
        }

        private void SetupServices()
        {
            _serviceLocator = new ServiceLocator();
            _saveService = new SaveService();
            _interactionService = new InteractionService();

            _levelService.Initialize();

            // Register the ones that need to be accessed globally
            _serviceLocator.RegisterService(_levelService);
            _serviceLocator.RegisterService(_saveService);
            _serviceLocator.RegisterService<IUserInput>(_userInput);
            _serviceLocator.RegisterService(_screenShakeService);
            _serviceLocator.RegisterService(_debugService);
            _serviceLocator.RegisterService(_protaganist);
            _serviceLocator.RegisterService(_interactionService);
        }

        private void SetupGlobalUI()
        {
            GlobalUIFactory globalUIFactory = new(_startupConfigSO,
                _saveService,
                _userInput,
                _screenShakeService,
                _globalUIParentTransform);

            globalUIFactory.CreateGlobalUI(out _pauseMenuController, out _sceneTransitionVisuals);

            _levelService.SetSceneTransitionVisuals(_sceneTransitionVisuals);
        }

        private async UniTask SetupFMOD()
        {
            Debug.Log("Loading starting banks");
            _studioBankLoader.Load();

            // Load all the global banks
            await UniTask.WaitUntil(() => RuntimeManager.HaveAllBanksLoaded);
            await UniTask.WaitUntil(() => !RuntimeManager.AnySampleDataLoading());

            Debug.Log("All banks loaded");
        }

        private void Update()
        {
            if (_gameplayController != null)
            {
                _gameplayController.TickGameplay();
            }
        }
    }
}