using System;
using GameInput;
using GameInput.Interface;
using UnityEngine;

namespace Framework
{
    /// <summary>
    ///     Single point of access to global services
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class ServiceLocator : MonoBehaviour
    {
        [Header("Service References")]

        [SerializeField]
        private InputService _userInputProvider;

        private static ServiceLocator instance;

        public static IUserInput GetUserInput()
        {
            InputService userInput = instance._userInputProvider;

            if (userInput == null)
            {
                // No scenario where we expect a null
                throw new Exception("Missing user input provider");
            }

            return userInput;
        }

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Multiple instances of ServiceLocator found.");
                Destroy(gameObject);
                return;
            }

            instance = this;
        }
    }
}