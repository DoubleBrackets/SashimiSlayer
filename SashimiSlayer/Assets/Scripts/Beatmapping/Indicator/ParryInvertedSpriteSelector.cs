using UnityEngine;
using UnityEngine.UI;
using ValueSO;
using ValueSO.Core;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Utility component that selects a sprite based on parry direction.
    /// </summary>
    public class ParryInvertedSpriteSelector : MonoBehaviour, IValueSOObserver
    {
        [Header("ValueSO (Read)")]

        [SerializeField]
        private BoolValueSO _invertParryDirections;

        [SerializeField]
        private Sprite _defaultSprite;

        [SerializeField]
        private Sprite _flippedSprite;

        [Header("Depends")]

        [SerializeField]
        private Image _image;

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _invertParryDirections.AddListener(this, OnFlipParry, true);
        }

        private void OnDestroy()
        {
            _invertParryDirections.RemoveListener(this);
        }

        private void OnFlipParry(bool flip)
        {
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