using System;
using Beatmapping.Events;
using Beatmapping.NoteInteraction.DataTypes;
using EditorUtils.BoldHeader;
using Events.Basic;
using NaughtyAttributes;
using Scoring;
using Scoring.Events;
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

            public string HighScoreKey => BeatmapID + "HighScore";

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
            _onDrawGuiEvent.AddListener(DrawGUI);
        }

        private void OnDestroy()
        {
            _interactionFinalResultEvent.RemoveListener(OnBeatInteractionResult);
            _loadBeatmapEvent.RemoveListener(OnLoadBeatmap);
            _onDrawGuiEvent.RemoveListener(DrawGUI);
        }

        private void DrawGUI()
        {
            GUILayout.Label(JsonUtility.ToJson(_currentScore));
        }

        private void OnBeatInteractionResult(NoteInteractionFinalResult interactionNoteInteractionFinalResult)
        {
            _currentScore.MaxScore += _scoreConfig.PointsForPerfect;
            TimingWindow.Score score = interactionNoteInteractionFinalResult.TimingResult.Score;

            if (score == TimingWindow.Score.Miss || !interactionNoteInteractionFinalResult.Successful)
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
                if (interactionNoteInteractionFinalResult.TimingResult.Direction == TimingWindow.Direction.Early)
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