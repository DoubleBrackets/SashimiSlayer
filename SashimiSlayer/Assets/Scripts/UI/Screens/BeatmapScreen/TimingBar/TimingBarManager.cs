using System.Collections.Generic;
using Beatmapping.Events;
using Beatmapping.NoteInteraction.DataTypes;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace UI.Screens.BeatmapScreen.TimingBar
{
    public class TimingBarManager : MonoBehaviour
    {
        [BoldHeader("(UNUSED) Timing Bar Manager")]
        [InfoBox("Manages the interaction timing offset bar. Currently Hidden")]
        [Header("Events (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        [Header("Dependencies")]

        [SerializeField]
        private RectTransform _bar;

        [SerializeField]
        private List<TimingBarTick> _hitResultPrefab;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnBeatInteractionResult);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnBeatInteractionResult);
        }

        private void OnBeatInteractionResult(NoteInteractionFinalResult result)
        {
            if (!result.Successful)
            {
                return;
            }

            float offset = result.TimingResult.NormalizedTimeDelta;

            TimingBarTick hitResult = Instantiate(_hitResultPrefab[(int)result.Pose], _bar);
            var rectTransform = hitResult.GetComponent<RectTransform>();
            hitResult.SetVisuals(result.NoteInteractionType);
            rectTransform.anchoredPosition = new Vector3(offset * _bar.rect.width / 2f, 0, 0);
        }
    }
}