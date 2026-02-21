using Framework.Services;
using GameInput.Haptics;
using GameInput.Interface;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Protag
{
    /// <summary>
    ///     Handles hooking up events to the appropriate rumble
    /// </summary>
    public class RumbleTest : MonoBehaviour
    {
        [Header("Rumbles")]

        [SerializeField]
        private RumbleFeedbackSO _rumble;

        private IUserInput _userInput;

        private void Awake()
        {
            _userInput = ServiceLocator.GetService<IUserInput>();
        }

        [Button("Do Rumble")]
        private void Rumble()
        {
            _userInput.AddRumble(_rumble);
        }
    }
}