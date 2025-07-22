using DG.Tweening;
using EditorUtils.BoldHeader;
using Events;
using NaughtyAttributes;
using UnityEngine;

namespace UI.Health
{
    public class ProgressbarScript : MonoBehaviour
    {
        [BoldHeader("Brogress Bar")]
        [InfoBox("Handles the progress bar for beatmap time")]
        [Header("Depends")]

        [SerializeField]
        private UnityEngine.UI.Slider _progressBar;

        [Header("Events (In)")]

        [SerializeField]
        private FloatEvent _normalizedProgressEvent;

        private readonly float _damageLingerTime = 0.5f;

        private void Awake()
        {
            _normalizedProgressEvent.AddListener(SetNormalizedProgress);
        }

        private void OnDestroy()
        {
            _normalizedProgressEvent.RemoveListener(SetNormalizedProgress);
        }

        public void InitializeBar(float maxHealth)
        {
            _progressBar.DOKill();
            _progressBar.maxValue = 1f;
            _progressBar.value = maxHealth;
        }

        public void SetNormalizedProgress(float normalizedProgress)
        {
            _progressBar.DOValue(normalizedProgress, _damageLingerTime);
        }
    }
}