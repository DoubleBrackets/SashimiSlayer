using System;
using GameInput;
using GameInput.Interface;
using Saving;
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
        private static SaveService saveService;

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

        public static SaveService GetGameSaveService()
        {
            return saveService ??= new SaveService();
        }

        private void Awake()
        {
            if (instance != null)
            {
                Debug.LogError("Multiple instances of ServiceLocator found.");
                Destroy(gameObject);
                return;
            }

            saveService = new SaveService();

            instance = this;
        }
    }
}