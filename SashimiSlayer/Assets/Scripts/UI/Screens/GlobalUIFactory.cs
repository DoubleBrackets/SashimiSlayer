using Framework;
using GameInput.Interface;
using GameJuice.ScreenShake;
using Saving;
using UI.Screens.PauseMenu;
using UI.Screens.TransitionUI;
using UnityEngine;

namespace UI.Screens
{
    /// <summary>
    ///     Handles instantiating and setup of global UI components.'
    /// </summary>
    public class GlobalUIFactory
    {
        private readonly StartupConfigSO _startupConfigSO;
        private readonly SaveService _saveService;
        private readonly IUserInput _userInput;
        private readonly ScreenShakeService _screenShakeService;
        private readonly Transform _parent;

        public GlobalUIFactory(
            StartupConfigSO startupConfigSO,
            SaveService saveService,
            IUserInput userInput,
            ScreenShakeService screenShakeService,
            Transform parent)
        {
            _startupConfigSO = startupConfigSO;
            _saveService = saveService;
            _userInput = userInput;
            _screenShakeService = screenShakeService;
            _parent = parent;
        }

        public void CreateGlobalUI(out PauseMenuController pauseMenuController,
            out SceneTransitionVisuals sceneTransitionVisuals)
        {
            pauseMenuController = GameObject.Instantiate(_startupConfigSO.PauseMenuPrefab, _parent);
            pauseMenuController.Initialize(_userInput, _saveService, _screenShakeService);

            sceneTransitionVisuals = GameObject.Instantiate(_startupConfigSO.SceneTransitionVisualsPrefab, _parent);
        }
    }
}