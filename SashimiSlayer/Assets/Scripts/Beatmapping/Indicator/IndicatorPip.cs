using DG.Tweening;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Subdivision tick for the indicator. Can be turned on and off.
    /// </summary>
    public class IndicatorPip : MonoBehaviour
    {
        [BoldHeader("Timing Indicator Pip")]
        [InfoBox("Represents a single pip that can be turned on, off and flashed")]
        [Header("Dependencies")]

        [SerializeField]
        private SpriteRenderer _onSprite;

        [SerializeField]
        private SpriteRenderer _offSprite;

        [Header("Visuals")]

        [SerializeField]
        private AnimationCurve _alphaCurve;

        [SerializeField]
        private float _squishScale;

        [SerializeField]
        private float _squishDuration;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onPipFlash;

        public bool IsOn { get; private set; }

        private float _onSpriteAlpha;
        private float _offSpriteAlpha;

        public void Setup()
        {
            _onSpriteAlpha = _onSprite.color.a;
            _offSpriteAlpha = _offSprite.color.a;
        }

        [Button("Set On")]
        public void SetOn()
        {
            SetOn(true);
        }

        [Button("Set Off")]
        public void SetOff()
        {
            SetOn(false);
        }

        public void Flash()
        {
            _onPipFlash.Invoke();
        }

        public void SetOn(bool isOn)
        {
            _onSprite.gameObject.SetActive(isOn);
            _offSprite.gameObject.SetActive(!isOn);
            IsOn = isOn;
        }

        public void DoSquish()
        {
            transform.localScale = new Vector3(1 / _squishScale, _squishScale, 1);
            transform.DOScaleY(1, _squishDuration);
            transform.DOScaleX(1, _squishDuration);
        }

        public void SetVisible(bool isVisible)
        {
            _onSprite.gameObject.SetActive(isVisible);
            _offSprite.gameObject.SetActive(isVisible);
        }

        public void SetAlpha(float normalized)
        {
            float alpha = _alphaCurve.Evaluate(normalized);

            Color color = _onSprite.color;
            color.a = alpha * _onSpriteAlpha;
            _onSprite.color = color;

            color = _offSprite.color;
            color.a = alpha * _offSpriteAlpha;
            _offSprite.color = color;
        }
    }
}