using Beatmapping.NoteInteraction.DataTypes;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.BeatmapScreen.TimingBar
{
    public class TimingBarTick : MonoBehaviour
    {
        [SerializeField]
        private float _fadeDuration;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [SerializeField]
        private Image _blockImage;

        [SerializeField]
        private Image _sliceImage;

        private void Awake()
        {
            transform.localScale = Vector3.one * 2f;
            transform.DOScale(1, 0.25f);
            _canvasGroup.DOFade(0, _fadeDuration).SetEase(Ease.OutQuint).OnComplete(() => Destroy(gameObject));
        }

        public void SetVisuals(NoteInteractionType resultNoteInteractionType)
        {
            _blockImage.enabled = resultNoteInteractionType == NoteInteractionType.Block;
            _sliceImage.enabled = resultNoteInteractionType == NoteInteractionType.Slice;
        }
    }
}