using System.Collections.Generic;
using Core.Scene;
using Cysharp.Threading.Tasks;
using GameInput;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Menus.LevelSelect
{
    public class LevelSelectManager : MonoBehaviour
    {
        [FormerlySerializedAs("mapRoster")]
        [FormerlySerializedAs("_levelRoster")]
        [Header("Dependencies")]

        [SerializeField]
        private TrackRosterSO trackRoster;

        [FormerlySerializedAs("levelPanelPrefab")]
        [SerializeField]
        private TrackPanel trackPanelPrefab;

        [SerializeField]
        private Transform _panelContainer;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent OnLevelDecremented;

        [SerializeField]
        private UnityEvent OnLevelIncremented;

        private bool _loaded;

        private List<TrackPanel> _levelPanels = new();
        private int _currentPanelIndex;
        private bool _hardMaps;

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
        }

        private void SetupLevelSelectUI()
        {
            if (trackRoster == null)
            {
                Debug.LogError("LevelRosterSO is not assigned in LevelSelectManager.");
                return;
            }

            if (trackRoster.Tracks.Count == 0)
            {
                Debug.LogError("No levels found in LevelRosterSO.");
                return;
            }

            foreach (TrackRosterSO.TrackEntry track in trackRoster.Tracks)
            {
                TrackPanel trackPanel = Instantiate(trackPanelPrefab, _panelContainer);
                trackPanel.SetupUI(track);
                trackPanel.OnLevelSelected += OnLevelSelected;
                trackPanel.SetVisible(false);

                _levelPanels.Add(trackPanel);
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

        public void SetShowHardMap(bool hardMaps)
        {
            _hardMaps = hardMaps;
            foreach (TrackPanel panel in _levelPanels)
            {
                panel.SetHardLevel(hardMaps);
            }
        }
    }
}