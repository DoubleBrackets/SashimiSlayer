using UnityEngine;

namespace UI
{
    public class CameraDestroy : MonoBehaviour
    {
        private void Awake()
        {
            Destroy(gameObject);
        }
    }
}