using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using ValueSO;
using ValueSO.Core;

namespace GameInput.Extra
{
    /// <summary>
    ///     Utility component that selects a sprite based on parry direction.
    /// </summary>
    public class ParryInvertedSpriteSelector : MonoBehaviour, IValueSOObserver
    {
        [FormerlySerializedAs("_invertParryDirections")]
        [Header("ValueSO (Read)")]

        [SerializeField]
        private BoolValueSO _invertParrySymbols;

        [SerializeField]
        private SwordHandednessValueSO _currentHandednessValueSO;

        [Header("Config")]

        [SerializeField]
        private Sprite _defaultSprite;

        [SerializeField]
        private Sprite _flippedSprite;

        [Header("Targets")]

        [SerializeField]
        private Image _image;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _invertParrySymbols.AddListener(this, OnFlipParry, true);
            _currentHandednessValueSO.AddListener(this, OnHandednessChange, true);
        }

        private void OnDestroy()
        {
            _invertParrySymbols.RemoveListener(this);
            _currentHandednessValueSO.RemoveListener(this);
        }

        private void OnFlipParry(bool b)
        {
            UpdateFlip();
        }

        private void OnHandednessChange(SwordHandedness handedness)
        {
            UpdateFlip();
        }

        private void UpdateFlip()
        {
            bool flip = _invertParrySymbols.Value ^
                        (_currentHandednessValueSO.Value == SwordHandedness.LeftHandedSword);

            if (_image)
            {
                _image.sprite = flip ? _flippedSprite : _defaultSprite;
            }

            if (_spriteRenderer)
            {
                _spriteRenderer.sprite = flip ? _flippedSprite : _defaultSprite;
            }
        }
    }
}