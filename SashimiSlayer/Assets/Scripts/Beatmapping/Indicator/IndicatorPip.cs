using System.Collections.Generic;
using DG.Tweening;
using EditorUtils.BoldHeader;
using GameInput;
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

        [Tooltip("If this is a block symbol. Used for flipping the directional sprite based on settings")]
        [SerializeField]
        private bool _isBlockSymbol;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onFlashEntry;

        [SerializeField]
        private UnityEvent _onFlashTrigger;

        public bool IsOn { get; private set; }

        private void Start()
        {
            if (_isBlockSymbol && InputService.Instance != null && InputService.Instance.FlipParryDirection)
            {
                // If this is a block symbol, flip the sprite based on the input settings
                _onSprite.ForEach(sprite => sprite.flipX = !sprite.flipX);
                _offSprite.ForEach(sprite => sprite.flipX = !sprite.flipX);
            }
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