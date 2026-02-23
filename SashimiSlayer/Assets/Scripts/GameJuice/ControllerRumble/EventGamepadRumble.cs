using Events;
using Framework;
using GameInput.Haptics;
using GameInput.Interface;
using UnityEngine;

namespace GameJuice.ControllerRumble
{
    /// <summary>
    ///     Simple component for playing gamepad rumble on an event
    /// </summary>
    public class EventGamepadRumble : MonoBehaviour
    {
        [Header("Config")]

        [SerializeField]
        private SOEvent _event;

        [SerializeField]
        private RumbleFeedbackSO _rumbleFeedback;

        private IUserInput _userInput;

        private void OnEnable()
        {
            _event.AddListener(HandleEvent);
            _userInput = ServiceLocator.GetService<IUserInput>();
        }

        private void OnDisable()
        {
            _event.RemoveListener(HandleEvent);
        }

        private void HandleEvent()
        {
            _userInput.AddRumble(_rumbleFeedback);
        }
    }
}