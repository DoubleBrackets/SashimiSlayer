using Events;
using GameInput;
using UnityEngine;
using UnityEngine.UI;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Utility component that switches sprites based on parry direction.
    /// </summary>
    public class ParrySymbolSwitcher : MonoBehaviour
    {
        [Header("Events (In)")]

        [SerializeField]
        private BoolEvent _flipParryDirectionEvent;

        [SerializeField]
        private Sprite _defaultSprite;

        [SerializeField]
        private Sprite _flippedSprite;

        [Header("Depends")]

        [SerializeField]
        private Image _image;

        private void Awake()
        {
            if (InputService.Instance != null && InputService.Instance.FlipParryDirection)
            {
                OnFlipParry(true);
            }

            if (_flipParryDirectionEvent)
            {
                _flipParryDirectionEvent.AddListener(OnFlipParry);
            }
        }

        private void OnDestroy()
        {
            if (_flipParryDirectionEvent)
            {
                _flipParryDirectionEvent.RemoveListener(OnFlipParry);
            }
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