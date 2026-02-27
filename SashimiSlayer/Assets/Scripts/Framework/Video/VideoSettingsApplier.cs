using Saving.Values;
using UnityEngine;
using ValueSO;

namespace Framework.Video
{
    public class VideoSettingsApplier : MonoBehaviour, IValueSOObserver
    {
        [Header("ValueSO (Read)")]

        [SerializeField]
        private FullScreenModeValueSO _fullScreenModeValueSO;

        private void Awake()
        {
            _fullScreenModeValueSO.AddListener(this, HandleSetWindowMode, true);
        }

        private void OnDestroy()
        {
            _fullScreenModeValueSO.RemoveListener(this);
        }

        private void HandleSetWindowMode(FullScreenMode fullScreenMode)
        {
            Screen.fullScreenMode = fullScreenMode;
        }
    }
}