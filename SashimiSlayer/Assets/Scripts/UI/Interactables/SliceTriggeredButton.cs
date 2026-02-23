using Cysharp.Threading.Tasks;
using Interactions.Components;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Interactables
{
    /// <summary>
    ///     Generic component for triggering an event when the player slices it
    /// </summary>
    [RequireComponent(typeof(CircularSliceableMono))]
    public class SliceTriggeredButton : MonoBehaviour
    {
        [Header("Components")]

        [SerializeField]
        private CircularSliceableMono _circularSliceableMono;

        [Header("UnityEvents")]

        [SerializeField]
        private UnityEvent _buttonSlicedUnityEvent;

        [SerializeField]
        private UnityEvent _buttonSlicedDelayedEvent;

        [SerializeField]
        private UnityEvent _buttonHoveredEvent;

        [SerializeField]
        private UnityEvent _buttonUnhoveredEvent;

        [Header("Config")]

        [SerializeField]
        private float _delay;

        private void OnValidate()
        {
            if (_circularSliceableMono == null)
            {
                _circularSliceableMono = GetComponent<CircularSliceableMono>();
            }
        }

        private void Awake()
        {
            _circularSliceableMono.OnHovered += HandleOnHovered;
            _circularSliceableMono.OnUnhovered += HandleOnUnhovered;
            _circularSliceableMono.OnSliced += HandleOnSliced;
        }

        private void OnDestroy()
        {
            _circularSliceableMono.OnHovered -= HandleOnHovered;
            _circularSliceableMono.OnUnhovered -= HandleOnUnhovered;
            _circularSliceableMono.OnSliced -= HandleOnSliced;
        }

        private void HandleOnSliced()
        {
            _buttonSlicedUnityEvent?.Invoke();
            TriggerDelayedEvent(_delay).Forget();
        }

        private void HandleOnUnhovered()
        {
            _buttonUnhoveredEvent?.Invoke();
        }

        private void HandleOnHovered()
        {
            _buttonHoveredEvent?.Invoke();
        }

        private async UniTaskVoid TriggerDelayedEvent(float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            _buttonSlicedDelayedEvent?.Invoke();
        }
    }
}