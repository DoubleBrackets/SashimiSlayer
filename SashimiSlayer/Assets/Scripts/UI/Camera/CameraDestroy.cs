using UnityEngine;

namespace UI.Camera
{
    public class CameraDestroy : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject);
        }
    }
}