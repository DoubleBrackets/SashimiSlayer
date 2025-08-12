using System.Collections.Generic;
using Core.Scene;
using Cysharp.Threading.Tasks;
using GameInput;
using UnityEngine;
using UnityEngine.Events;
using GameLevelSO = Core.Scene.GameLevelSO;

namespace Menus.LevelSelect
{
    public class LevelSelectManager : MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private SongRosterSO songRoster;

        [SerializeField]
        private SongPanel songPanelPrefab;

        [SerializeField]
        private Transform _panelContainer;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent OnLevelDecremented;

        [SerializeField]
        private UnityEvent OnLevelIncremented;

        private bool _loaded;

        private List<SongPanel> _levelPanels = new();
        private int _currentPanelIndex;
        private bool _hardDifficulty;

        private void Awake()
        {
            SetupLevelSelectUI();

            InputService.Instance.OnBlockPoseChanged += HandleBlockPoseChanged;
        }

        private void OnDestroy()
        {
            InputService.Instance.OnBlockPoseChanged -= HandleBlockPoseChanged;
        }

        /// <summary>
        ///     Handle switching between level panels when blocking
        /// </summary>
        /// <param name="newState"></param>
        private void HandleBlockPoseChanged(SharedTypes.BlockPoseStates newState)
        {
            int prevPanelIndex = _currentPanelIndex;
            int flipBlockDirection = InputService.Instance.FlipParryDirection ? -1 : 1;
            if (newState == SharedTypes.BlockPoseStates.BotPose)
            {
                // Go next
                _currentPanelIndex += flipBlockDirection;
            }
            else if (newState == SharedTypes.BlockPoseStates.TopPose)
            {
                // Go previous
                _currentPanelIndex -= flipBlockDirection;
            }

            _currentPanelIndex = Mathf.Clamp(_currentPanelIndex, 0, _levelPanels.Count - 1);

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
                songPanel.OnLevelSelected += OnLevelSelected;
                songPanel.SetVisible(false);

                _levelPanels.Add(songPanel);
            }

            _levelPanels[_currentPanelIndex].SetVisible(true);
        }

        private void OnLevelSelected(GameLevelSO level)
        {
            if (_loaded)
            {
                return;
            }

            _loaded = true;
            LevelLoader.Instance.LoadLevel(level).Forget();
        }

        public void SetDifficulty(bool hardDifficulty)
        {
            LevelLoader.Instance.SetDifficulty(hardDifficulty
                ? LevelLoader.Difficulty.Hard
                : LevelLoader.Difficulty.Normal);

            _hardDifficulty = hardDifficulty;
            foreach (SongPanel panel in _levelPanels)
            {
                panel.SetDifficulty(hardDifficulty);
            }
        }
    }
}