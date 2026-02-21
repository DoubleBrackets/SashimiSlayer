using System.Collections.Generic;
using CommonTypes;
using Cysharp.Threading.Tasks;
using Framework.Services;
using GameInput.Interface;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GameLevelSO = Core.Scene.GameLevelSO;

namespace Menus.LevelSelect
{
    /// <summary>
    ///     Handles the UI for the level select menu.
    /// </summary>
    public class LevelSelectManager : MonoBehaviour, IFindsDependencies
    {
        [Header("Dependencies")]

        [SerializeField]
        private SongRosterSO songRoster;

        [SerializeField]
        private SongPanel songPanelPrefab;

        [SerializeField]
        private Transform _panelContainer;

        [Header("Carousal Prompts")]

        [SerializeField]
        private Color _disabledColor;

        [SerializeField]
        private Image _decrementButton;

        [SerializeField]
        private Image _incrementButton;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent OnLevelDecremented;

        [SerializeField]
        private UnityEvent OnLevelIncremented;

        [SerializeField]
        private UnityEvent OnSongChosen;

        private bool _loaded;

        private List<SongPanel> _levelPanels = new();
        private int _currentPanelIndex;
        private bool _hardDifficulty;

        private IUserInput _userInput;

        public void FindDependencies()
        {
            _userInput = ServiceLocator.GetService<IUserInput>();
        }

        private void Awake()
        {
            FindDependencies();

            SetupLevelSelectUI();

            _userInput.OnBlockPoseChanged += HandleBlockPoseChanged;

            UpdatePromptOpacity();
        }

        private void OnDestroy()
        {
            _userInput.OnBlockPoseChanged -= HandleBlockPoseChanged;
        }

        /// <summary>
        ///     Handle switching between level panels when blocking
        /// </summary>
        /// <param name="newState"></param>
        private void HandleBlockPoseChanged(BlockPoseStates newState)
        {
            int prevPanelIndex = _currentPanelIndex;
            int flipBlockDirection = _userInput.FlipParryDirection ? -1 : 1;
            if (newState == BlockPoseStates.BlockLeft)
            {
                // Go next
                _currentPanelIndex -= flipBlockDirection;
            }
            else if (newState == BlockPoseStates.BlockRight)
            {
                // Go previous
                _currentPanelIndex += flipBlockDirection;
            }

            _currentPanelIndex = Mathf.Clamp(_currentPanelIndex, 0, _levelPanels.Count - 1);

            UpdatePromptOpacity();

            if (prevPanelIndex == _currentPanelIndex)
            {
                return;
            }

            if (_currentPanelIndex > prevPanelIndex)
            {
                OnLevelIncremented?.Invoke();
            }
            else
            {
                OnLevelDecremented?.Invoke();
            }

            _levelPanels[prevPanelIndex].SetVisible(false);
            _levelPanels[_currentPanelIndex].SetVisible(true);
        }

        private void UpdatePromptOpacity()
        {
            _decrementButton.color = _currentPanelIndex == 0
                ? _disabledColor
                : Color.white;

            _incrementButton.color = _currentPanelIndex == _levelPanels.Count - 1
                ? _disabledColor
                : Color.white;
        }

        private void SetupLevelSelectUI()
        {
            if (songRoster == null)
            {
                Debug.LogError("LevelRosterSO is not assigned in LevelSelectManager.");
                return;
            }

            if (songRoster.Songs.Count == 0)
            {
                Debug.LogError("No levels found in LevelRosterSO.");
                return;
            }

            foreach (GameLevelSO song in songRoster.Songs)
            {
                SongPanel songPanel = Instantiate(songPanelPrefab, _panelContainer);
                songPanel.SetupUI(song);
                songPanel.OnLevelSelected += OnLoadLevel;
                songPanel.OnPanelSliced += OnLevelSliced;
                songPanel.SetVisible(false);

                _levelPanels.Add(songPanel);
            }

            _levelPanels[_currentPanelIndex].SetVisible(true);
        }

        private void OnLevelSliced()
        {
            OnSongChosen?.Invoke();
        }

        private void OnLoadLevel(GameLevelSO level)
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            ServiceLocator.GetService<LevelLoader>().LoadLevel(level).Forget();
        }
    }
}