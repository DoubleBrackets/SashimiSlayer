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
            if (_image == null)
            {
                Debug.LogWarning("Image component is not assigned");
                return;
            }

            _image.sprite = flip ? _flippedSprite : _defaultSprite;

            if (_image.sprite == null)
            {
                Debug.LogWarning("No sprite assigned");
            }
        }
    }
}