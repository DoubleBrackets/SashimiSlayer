using Cysharp.Threading.Tasks;
using DG.Tweening;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using TMPro;
using UnityEngine;
using Utility;

namespace Core.Scene
{
    public class SceneTransitionVisuals : MonoBehaviour
    {
        [BoldHeader("Scene Transition Visuals")]
        [InfoBox("Handles scene transition visuals")]
        [Header("Depends")]

        [SerializeField]
        private CanvasGroup _transitionCanvasGroup;

        [SerializeField]
        private TMP_Text _titleText;

        [SerializeField]
        private RectTransform _fadeInTransform;

        [SerializeField]
        private RectTransform _ref;

        [Header("Config")]

        [SerializeField]
        private float _fadeOutTime;

        [SerializeField]
        private float _fadeInTime;

        [SerializeField]
        private float _fadeInDelay;

        [SerializeField]
        private float _margin;

        private void Awake()
        {
            _fadeInTransform.gameObject.SetActive(true);
        }

        public void SetTitleText(string text)
        {
            _titleText.text = text;
        }

        public async UniTask FadeOut()
        {
            if (_fadeInTransform == null)
            {
                return;
            }

            _transitionCanvasGroup.SetEnabled(true);

            _fadeInTransform.anchoredPosition = new Vector2(-_ref.rect.width - _margin, 0);
            DOTween.To(
                    () => _fadeInTransform.anchoredPosition.x,
                    x => _fadeInTransform.anchoredPosition = new Vector2(x, 0),
                    0,
                    _fadeInTime)
                .SetEase(Ease.InOutSine);
            await UniTask.Delay((int)(_fadeOutTime * 1000));
        }

        public async UniTask FadeIn()
        {
            if (_fadeInTransform == null)
            {
                return;
            }

            _fadeInTransform.anchoredPosition = new Vector2(0, 0);
            await UniTask.Delay((int)(_fadeInDelay * 1000));
            DOTween.To(
                    () => _fadeInTransform.anchoredPosition.x,
                    x => _fadeInTransform.anchoredPosition = new Vector2(x, 0),
                    _ref.rect.width + _margin,
                    _fadeInTime)
                .SetEase(Ease.InOutSine);
            await UniTask.Delay((int)(_fadeInTime * 1000));

            _transitionCanvasGroup.SetEnabled(false);
        }
    }
}