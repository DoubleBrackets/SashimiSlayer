using System.Collections.Generic;
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
        private List<SpriteRenderer> _onSprite;

        [SerializeField]
        private List<SpriteRenderer> _offSprite;

        [Header("Visuals")]

        [SerializeField]
        private AnimationCurve _alphaCurve;

        [SerializeField]
        private float _squishScale;

        [SerializeField]
        private float _squishDuration;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onFlashEntry;

        [SerializeField]
        private UnityEvent _onFlashTrigger;

        public bool IsOn { get; private set; }

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

        /// <summary>
        ///     "Flash" the pip on the beat it triggers
        /// </summary>
        public void FlashTriggerBeat()
        {
            _onFlashTrigger.Invoke();
        }

        public void FlashEntry()
        {
            _onFlashEntry.Invoke();
        }

        public void SetOn(bool isOn)
        {
            _onSprite.ForEach(sprite => sprite.enabled = isOn);
            _offSprite.ForEach(sprite => sprite.enabled = !isOn);

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
            _onSprite.ForEach(sprite => sprite.gameObject.SetActive(isVisible));
            _offSprite.ForEach(sprite => sprite.gameObject.SetActive(isVisible));
        }
    }
}