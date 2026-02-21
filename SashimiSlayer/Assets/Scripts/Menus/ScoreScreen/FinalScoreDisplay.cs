using Beatmapping.Scoring;
using CommonTypes;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events;
using Framework;
using Saving;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Menus.ScoreScreen
{
    public class FinalScoreDisplay : MonoBehaviour, IFindsDependencies
    {
        [Header("Display")]

        [SerializeField]
        private TMP_Text _scoreText;

        [SerializeField]
        private TMP_Text _highscoreText;

        [SerializeField]
        private GameObject _newHighscoreVisual;

        [SerializeField]
        private CategoryLine _perfectLine;

        [SerializeField]
        private CategoryLine _earlyLine;

        [SerializeField]
        private CategoryLine _lateLine;

        [SerializeField]
        private CategoryLine _missLine;

        [Header("Events (Out)")]

        [SerializeField]
        private VoidEvent _onProtagVictory;

        [SerializeField]
        private VoidEvent _onProtagLoss;

        [Header("Depends")]

        [SerializeField]
        private ScoreConfigSO _scoreConfig;

        [Header("Timing")]

        [SerializeField]
        private float _delayBetweenLines;

        [SerializeField]
        private float _delayBeforeScore;

        [FormerlySerializedAs("_delayBeforeHighscore")]
        [SerializeField]
        private float _delayBeforeFinalScore;

        [Header("Shake")]

        [SerializeField]
        private float _shakeDuration;

        [SerializeField]
        private float _shakeStrength;

        [SerializeField]
        private int _vibratoStrength;

        private float _percentageAnimated;

        private SaveService _saveService;

        private void Start()
        {
            FindDependencies();
            ScoringService.BeatmapScore scoring = ScoringService.Instance.CurrentScore;

            if (scoring.DidSucceed)
            {
                _onProtagVictory.Raise();
            }
            else
            {
                _onProtagLoss.Raise();
            }

            DisplayUI(scoring).Forget();
        }

        private async UniTaskVoid DisplayUI(ScoringService.BeatmapScore scoring)
        {
            LoadHighScore(scoring, out bool isHighscore, out int currentHighscore);

            GameObject customEffect = scoring.Beatmap.ResultsScreenCustomPrefab;

            if (customEffect != null)
            {
                Instantiate(customEffect, transform.position, Quaternion.identity);
            }

            _scoreText.gameObject.SetActive(false);
            _perfectLine.SetVisible(false);
            _earlyLine.SetVisible(false);
            _lateLine.SetVisible(false);
            _missLine.SetVisible(false);
            _highscoreText.gameObject.SetActive(false);

            var delay = (int)(_delayBetweenLines * 1000);

            await UniTask.Delay((int)(_delayBeforeScore * 1000));

            _perfectLine.SetVisible(true);
            _perfectLine.SetCategory("Perfect", scoring.Perfects, _scoreConfig.PointsForPerfect);

            await UniTask.Delay(delay);

            _earlyLine.SetVisible(true);
            _earlyLine.SetCategory("Early", scoring.Earlies, _scoreConfig.PointsForEarly);

            await UniTask.Delay(delay);

            _lateLine.SetVisible(true);
            _lateLine.SetCategory("Late", scoring.Lates, _scoreConfig.PointsForLate);

            await UniTask.Delay(delay);

            _missLine.SetVisible(true);
            _missLine.SetCategory("Miss", scoring.Misses, _scoreConfig.PointsForMiss);

            await UniTask.Delay((int)(_delayBeforeFinalScore * 1000));

            _scoreText.gameObject.SetActive(true);
            _scoreText.text = scoring.FinalScore.ToString();
            _scoreText.transform.DOShakePosition(_shakeDuration, _shakeStrength, _vibratoStrength);

            await UniTask.Delay(delay);

            UpdateHighscoreVisual(isHighscore, currentHighscore);
        }

        private void LoadHighScore(ScoringService.BeatmapScore scoring, out bool isHighscore, out int currentHighscore)
        {
            float currentHighestScore = 0;

            if (_saveService.GetHighScore(scoring.HighScoreKey, out HighScoreSaveModel highScore))
            {
                currentHighestScore = highScore.FinalScore;
            }

            if (scoring.FinalScore > currentHighestScore && scoring.DidSucceed)
            {
                currentHighscore = scoring.FinalScore;
                isHighscore = true;

                _saveService.SetHighScore(new HighScoreSaveModel
                {
                    NameKey = scoring.HighScoreKey,
                    Earlies = scoring.Earlies,
                    Late = scoring.Lates,
                    Miss = scoring.Misses,
                    Perfects = scoring.Perfects,
                    FinalScore = scoring.FinalScore
                });
            }
            else
            {
                currentHighscore = (int)currentHighestScore;
                isHighscore = false;
            }
        }

        private void UpdateHighscoreVisual(bool isNewHighscore, int currentHighestScore)
        {
            _highscoreText.gameObject.SetActive(true);
            _newHighscoreVisual.SetActive(isNewHighscore);

            _highscoreText.text = $"{currentHighestScore}";
        }

        public void FindDependencies()
        {
            _saveService = ServiceLocator.GetGameSaveService();
        }
    }
}