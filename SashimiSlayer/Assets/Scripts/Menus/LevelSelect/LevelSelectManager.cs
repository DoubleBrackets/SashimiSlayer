using System.Collections.Generic;
using Core.Scene;
using Cysharp.Threading.Tasks;
using GameInput;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Menus.LevelSelect
{
    public class LevelSelectManager : MonoBehaviour
    {
        [Header("Dependencies")]

        [SerializeField]
        private LevelRosterSO _levelRoster;

        [SerializeField]
        private LevelPanel levelPanelPrefab;

        [SerializeField]
        private Transform _panelContainer;

        [SerializeField]
        private TMP_Text _levelDescriptionText;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent OnLevelDecremented;

        [SerializeField]
        private UnityEvent OnLevelIncremented;

        private bool _loaded;

        private List<LevelPanel> _levelPanels = new();
        private int _currentPanelIndex;

        private void Awake()
        {
            SetupLevelSelectUI();

            InputService.Instance.OnBlockPoseChanged += HandleBlockPoseChanged;
        }

        private void OnDestroy()
        {
            InputService.Instance.OnBlockPoseChanged -= HandleBlockPoseChanged;
        }

        private void HandleBlockPoseChanged(SharedTypes.BlockPoseStates newState)
        {
            int prevPanelIndex = _currentPanelIndex;
            if (newState == SharedTypes.BlockPoseStates.BotPose && _currentPanelIndex > 0)
            {
                // Go previous
                _currentPanelIndex--;
                OnLevelDecremented?.Invoke();
            }
            else if (newState == SharedTypes.BlockPoseStates.TopPose && _currentPanelIndex < _levelPanels.Count - 1)
            {
                // Go next
                _currentPanelIndex++;
                OnLevelIncremented?.Invoke();
            }

            if (prevPanelIndex == _currentPanelIndex)
            {
                return;
            }

            _levelPanels[prevPanelIndex].SetVisible(false);
            _levelPanels[_currentPanelIndex].SetVisible(true);
            UpdateDescriptionText();
        }

        private void UpdateDescriptionText()
        {
            _levelDescriptionText.text = _levelRoster.Levels[_currentPanelIndex].LevelDescription;
        }

        private void SetupLevelSelectUI()
        {
            if (_levelRoster == null)
            {
                Debug.LogError("LevelRosterSO is not assigned in LevelSelectManager.");
                return;
            }

            if (_levelRoster.Levels.Count == 0)
            {
                Debug.LogError("No levels found in LevelRosterSO.");
                return;
            }

            foreach (GameLevelSO level in _levelRoster.Levels)
            {
                LevelPanel levelPanel = Instantiate(levelPanelPrefab, _panelContainer);
                levelPanel.SetupUI(level);
                levelPanel.OnLevelSelected += OnLevelSelected;
                levelPanel.SetVisible(false);

                _levelPanels.Add(levelPanel);
            }

            _levelPanels[_currentPanelIndex].SetVisible(true);
            UpdateDescriptionText();
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
    }
}