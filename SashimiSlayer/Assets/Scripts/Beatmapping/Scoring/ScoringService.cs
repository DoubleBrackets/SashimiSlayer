using System;
using Beatmapping.Interactions;
using EditorUtils.BoldHeader;
using Events;
using Events.Core;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Scoring
{
    /// <summary>
    ///     Handles tracking gameplay score and interaction results
    /// </summary>
    public class ScoringService : MonoBehaviour
    {
        public struct BeatmapScore : IComparable<BeatmapScore>
        {
            public string BeatmapID;
            public BeatmapConfigSo Beatmap;
            public int Perfects;
            public int Earlies;
            public int Lates;
            public int Misses;

            public int FinalScore;

            public int MaxScore;

            /// <summary>
            ///     Unused; player death has been disabled
            /// </summary>
            public bool DidSucceed;

            public int CompareTo(BeatmapScore other)
            {
                return FinalScore.CompareTo(other.FinalScore);
            }
        }

        [BoldHeader("Scoring Service")]
        [InfoBox("Handles tracking gameplay score and interaction results")]
        [Header("Depends")]

        [SerializeField]
        private ScoreConfigSO _scoreConfig;

        [Header("Events (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _interactionFinalResultEvent;

        [SerializeField]
        private BeatmapEvent _loadBeatmapEvent;

        [SerializeField]
        private VoidEvent _playerDeathEvent;

        [SerializeField]
        private VoidEvent _onDrawGuiEvent;

        [Header("Events (Out)")]

        [SerializeField]
        private BeatmapScoreEvent _beatmapScoreEvent;

        public static ScoringService Instance { get; private set; }

        public BeatmapScore CurrentScore => _currentScore;

        [ShowNonSerializedField]
        private BeatmapScore _currentScore;

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

            _interactionFinalResultEvent.AddListener(OnBeatInteractionResult);
            _loadBeatmapEvent.AddListener(OnLoadBeatmap);
            _playerDeathEvent.AddListener(HandlePlayerDeath);
            _onDrawGuiEvent.AddListener(DrawGUI);
        }

        private void OnDestroy()
        {
            _interactionFinalResultEvent.RemoveListener(OnBeatInteractionResult);
            _loadBeatmapEvent.RemoveListener(OnLoadBeatmap);
            _playerDeathEvent.RemoveListener(HandlePlayerDeath);
            _onDrawGuiEvent.RemoveListener(DrawGUI);
        }

        private void DrawGUI()
        {
            GUILayout.Label(JsonUtility.ToJson(_currentScore));
        }

        private void OnBeatInteractionResult(NoteInteraction.FinalResult interactionFinalResult)
        {
            _currentScore.MaxScore += _scoreConfig.PointsForPerfect;
            TimingWindow.Score score = interactionFinalResult.TimingResult.Score;

            if (score == TimingWindow.Score.Miss || !interactionFinalResult.Successful)
            {
                _currentScore.Misses++;
                _currentScore.FinalScore += _scoreConfig.PointsForMiss;
            }
            else if (score == TimingWindow.Score.Perfect)
            {
                _currentScore.Perfects++;
                _currentScore.FinalScore += _scoreConfig.PointsForPerfect;
            }
            else if (score == TimingWindow.Score.Pass)
            {
                if (interactionFinalResult.TimingResult.Direction == TimingWindow.Direction.Early)
                {
                    _currentScore.Earlies++;
                    _currentScore.FinalScore += _scoreConfig.PointsForEarly;
                }
                else
                {
                    _currentScore.Lates++;
                    _currentScore.FinalScore += _scoreConfig.PointsForLate;
                }
            }

            _beatmapScoreEvent.Raise(_currentScore);
        }

        private void HandlePlayerDeath()
        {
            _currentScore.DidSucceed = false;
        }

        private void OnLoadBeatmap(BeatmapConfigSo beatmap)
        {
            // Reset scoring
            _currentScore = new BeatmapScore
            {
                BeatmapID = beatmap.BeatmapID,
                Beatmap = beatmap,
                DidSucceed = true
            };

            _beatmapScoreEvent.Raise(_currentScore);
        }
    }
}