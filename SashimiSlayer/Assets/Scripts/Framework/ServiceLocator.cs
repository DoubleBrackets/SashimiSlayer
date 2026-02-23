using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    /// <summary>
    ///     Single point of access to global services
    /// </summary>
    public class ServiceLocator
    {
        private static ServiceLocator instance;

        private Dictionary<Type, object> _services = new();

        public ServiceLocator()
        {
            instance = this;
        }

        public void RegisterService<T>(T service)
        {
            if (_services.ContainsKey(typeof(T)))
            {
                Debug.LogWarning($"Service of type {typeof(T)} already registered.");
                return;
            }

            _services[typeof(T)] = service;
        }

        public static T GetService<T>()
        {
            if (instance == null)
            {
                Debug.LogError("ServiceLocator not initialized.");
                return default;
            }

            if (!instance._services.TryGetValue(typeof(T), out object service))
            {
                throw new Exception($"Service of type {typeof(T)} not found.");
            }

            return (T)service;
        }
    }
}