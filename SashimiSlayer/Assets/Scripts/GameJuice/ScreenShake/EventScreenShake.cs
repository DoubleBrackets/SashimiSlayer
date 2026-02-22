using Events;
using Framework;
using UnityEngine;

namespace GameJuice.ScreenShake
{
    /// <summary>
    ///     Simple component for playing screen shake on an event
    /// </summary>
    public class EventScreenShake : MonoBehaviour
    {
        [Header("Config")]

        [SerializeField]
        private SOEvent _event;

        [SerializeField]
        private ScreenShakeSO _screenShakeSO;

        private ScreenShakeService _screenShakeService;

        private void OnEnable()
        {
            _event.AddListener(HandleEvent);
            _screenShakeService = ServiceLocator.GetService<ScreenShakeService>();
        }

        private void OnDisable()
        {
            _event.RemoveListener(HandleEvent);
        }

        private void HandleEvent()
        {
            _screenShakeService.ShakeScreen(_screenShakeSO);
        }
    }
}