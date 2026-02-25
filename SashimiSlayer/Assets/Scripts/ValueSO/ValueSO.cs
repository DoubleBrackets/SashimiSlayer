using System;
using System.Collections.Generic;
using UnityEngine;

namespace ValueSO
{
    /// <summary>
    ///     Simple container for holding a shared value and notifying on changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ValueSO<T> : ScriptableObject
    {
        [SerializeField]
        [TextArea(10, 20)]
        private string _description;

        [field: SerializeField]
        private T _value;

        [field: SerializeField]
        public bool Log { get; private set; }

        private Dictionary<IValueSOObserver, Action<T>> _observers = new();

        public T Value => _value;

        /// <summary>
        ///     Set the value of this ValueSO, notifying observers if the value has changed.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="observerToIgnore">Do not notify this observer if its action is registered. Tool to avoid recursion</param>
        public void SetValue(T value, IValueSOObserver observerToIgnore)
        {
            if (EqualityComparer<T>.Default.Equals(_value, value))
            {
                return;
            }

            _value = value;
            foreach (KeyValuePair<IValueSOObserver, Action<T>> observer in _observers)
            {
                if (observer.Key != observerToIgnore)
                {
                    observer.Value(value);
                }
            }

            if (Log)
            {
                Debug.Log($"ValueSO {name} changed to {value}");
            }
        }

        public void SetValue(T value)
        {
            SetValue(value, null);
        }

        /// <summary>
        ///     Add a listener to this ValueSO. If invokeListenerImmediate is true, the callback will be invoked immediately with
        ///     the current value stored in this ValueSO.
        /// </summary>
        /// <param name="observer"></param>
        /// <param name="callback"></param>
        /// <param name="invokeListenerImmediate"></param>
        public void AddListener(IValueSOObserver observer, Action<T> callback, bool invokeListenerImmediate = false)
        {
            if (invokeListenerImmediate)
            {
                callback(Value);
            }

            if (_observers.ContainsKey(observer))
            {
                Debug.LogWarning($"ValueSO {name} already has observer {observer}");
                return;
            }

            _observers.Add(observer, callback);
        }

        public void RemoveListener(IValueSOObserver observer)
        {
            _observers.Remove(observer);
        }
    }
}