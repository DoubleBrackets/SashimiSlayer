using System;
using Framework;
using Interactions.Framework;
using Interactions.Interactables;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions.Components
{
    public class CircularSliceableMono : MonoBehaviour
    {
        [Header("Config")]

        [SerializeField]
        private float _radius;

        [SerializeField]
        private CircleSliceable.SpaceType _spaceType;

        [SerializeField]
        private bool _singleUse;

        [Header("UnityEvents")]

        [SerializeField]
        private UnityEvent _onHoveredEvent;

        [SerializeField]
        private UnityEvent _onUnhoveredEvent;

        [SerializeField]
        private UnityEvent _onSlicedEvent;

        public event Action OnHovered;

        public event Action OnUnhovered;

        public event Action OnSliced;

        private CircleSliceable _circleSliceable;

        public bool Enabled
        {
            get => _circleSliceable.Enabled;
            set => _circleSliceable.Enabled = value;
        }

        private void OnEnable()
        {
            _circleSliceable = new CircleSliceable(
                ServiceLocator.GetService<InteractionService>(),
                _radius,
                _singleUse,
                _spaceType,
                transform
            );

            _circleSliceable.OnHovered += HandleOnHovered;
            _circleSliceable.OnUnhovered += HandleOnUnhovered;
            _circleSliceable.OnSliced += HandleOnSliced;
        }

        private void OnDisable()
        {
            _circleSliceable.OnHovered -= HandleOnHovered;
            _circleSliceable.OnUnhovered -= HandleOnUnhovered;
            _circleSliceable.OnSliced -= HandleOnSliced;

            _circleSliceable.Cleanup();
        }

        private void HandleOnHovered()
        {
            _onHoveredEvent?.Invoke();
            OnHovered?.Invoke();
        }

        private void HandleOnUnhovered()
        {
            _onUnhoveredEvent?.Invoke();
            OnUnhovered?.Invoke();
        }

        private void HandleOnSliced()
        {
            _onSlicedEvent?.Invoke();
            OnSliced?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }
}