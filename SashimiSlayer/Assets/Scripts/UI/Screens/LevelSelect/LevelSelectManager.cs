using System.Collections.Generic;
using CommonTypes;
using Cysharp.Threading.Tasks;
using Framework;
using Framework.LevelLoading;
using Interactions.DataTypes;
using Interactions.Framework;
using Interactions.Interactables;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using GameLevelSO = Framework.LevelLoading.GameLevelSO;

namespace UI.Screens.LevelSelect
{
    /// <summary>
    ///     Handles the UI for the level select menu.
    /// </summary>
    public class LevelSelectManager : MonoBehaviour, IFindsDependencies, IInteractable
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

        private LevelService _levelService;
        private IInteractionService _interactionService;

        private SingleDirBlockable _leftBlockable;
        private SingleDirBlockable _rightBlockable;

        public void FindDependencies()
        {
            _levelService = ServiceLocator.GetService<LevelService>();
            _interactionService = ServiceLocator.GetService<IInteractionService>();
        }

        private void Awake()
        {
            FindDependencies();

            SetupLevelSelectUI();

            UpdatePromptOpacity();

            _leftBlockable =
                new SingleDirBlockable(this, BlockPoses.BlockLeft, false);
            _rightBlockable =
                new SingleDirBlockable(this, BlockPoses.BlockRight, false);
            UpdateBlockableEnabled();

            _interactionService.Register(this);

            _leftBlockable.OnBlocked += FlipPrevious;
            _rightBlockable.OnBlocked += FlipNext;
        }

        private void OnDestroy()
        {
            _interactionService.Unregister(this);
        }

        public void FlipNext()
        {
            ChangeSelection(1);
        }

        public void FlipPrevious()
        {
            ChangeSelection(-1);
        }

        /// <summary>
        ///     Handle switching between level panels when blocking
        /// </summary>
        private void ChangeSelection(int relativeIndex)
        {
            int prevPanelIndex = _currentPanelIndex;
            _currentPanelIndex += relativeIndex;

            _currentPanelIndex = Mathf.Clamp(_currentPanelIndex, 0, _levelPanels.Count - 1);

            UpdateBlockableEnabled();
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

        private void UpdateBlockableEnabled()
        {
            _leftBlockable.Enabled = _currentPanelIndex > 0;
            _rightBlockable.Enabled = _currentPanelIndex < _levelPanels.Count - 1;
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
            _levelService.LoadLevel(level).Forget();
        }

        public InteractionResult AttemptInteraction(InteractionAttempt attempt)
        {
            InteractionResult left = _leftBlockable.AttemptInteraction(attempt);
            InteractionResult right = _rightBlockable.AttemptInteraction(attempt);

            if (left.Successful)
            {
                return left;
            }

            if (right.Successful)
            {
                return right;
            }

            return InteractionResult.Fail(this);
        }

        public Vector2 Position => transform.position;
    }
}