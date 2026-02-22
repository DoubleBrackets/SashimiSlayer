using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace UI.Camera
{
    public class CameraCanvasBinder : MonoBehaviour
    {
        [BoldHeader("Camera Canvas Binder")]
        [InfoBox("Assigns main camera to canvas (for camera overlay UI)")]
        [SerializeField]
        private Canvas _canvas;

        private void Start()
        {
            _canvas.worldCamera = UnityEngine.Camera.main;
        }
    }
}