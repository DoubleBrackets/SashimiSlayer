using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Utility
{
    public class FixedAspectRatioCamera : MonoBehaviour
    {
        [BoldHeader("Fixed Aspect Ratio Camera")]
        [InfoBox("Maintains a fixed aspect ratio w/ black bars")]
        [Header("Config")]

        [SerializeField]
        private float _targetAspectRatio = 16f / 9f;

        [SerializeField]
        private bool _updatePerFrame;

        [Header("References")]

        [SerializeField]
        private Camera _camera;

        private void Start()
        {
            LetterboxCamera();
        }

        private void Update()
        {
            if (_updatePerFrame)
            {
                LetterboxCamera();
            }
        }

        private void LetterboxCamera()
        {
            float windowAspect = (float)Screen.width / Screen.height;
            float scaleHeight = windowAspect / _targetAspectRatio;
            Camera camera = _camera;
            if (scaleHeight < 1f)
            {
                Rect rect = camera.rect;
                rect.width = 1f;
                rect.height = scaleHeight;
                rect.x = 0f;
                rect.y = (1f - scaleHeight) / 2f;
                camera.rect = rect;
            }
            else
            {
                float scaleWidth = 1f / scaleHeight;
                Rect rect = camera.rect;
                rect.width = scaleWidth;
                rect.height = 1f;
                rect.x = (1f - scaleWidth) / 2f;
                rect.y = 0f;
                camera.rect = rect;
            }
        }
    }
}