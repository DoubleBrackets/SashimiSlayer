using System;
using Framework;
using Interactions.DataTypes;
using Interactions.Framework;
using Interactions.Interactables;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions.Components
{
    public class CircularSliceableMono : MonoBehaviour, IInteractable
    {
        [Header("Config")]

        [SerializeField]
        private float _radius;

        [SerializeField]
        private SpaceType _spaceType;

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

        private IInteractionService _interactionService;

        public bool Enabled
        {
            get => _circleSliceable.Enabled;
            set => _circleSliceable.Enabled = value;
        }

        private void OnEnable()
        {
            _interactionService = ServiceLocator.GetService<IInteractionService>();
            _circleSliceable = new CircleSliceable(
                this,
                _radius,
                _singleUse,
                _spaceType,
                transform
            );

            _circleSliceable.OnHovered += HandleOnHovered;
            _circleSliceable.OnUnhovered += HandleOnUnhovered;
            _circleSliceable.OnSliced += HandleOnSliced;

            _interactionService.Register(this);
        }

        private void OnDisable()
        {
            _circleSliceable.OnHovered -= HandleOnHovered;
            _circleSliceable.OnUnhovered -= HandleOnUnhovered;
            _circleSliceable.OnSliced -= HandleOnSliced;

            _interactionService.Unregister(this);
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

        public InteractionResult AttemptInteraction(InteractionAttempt attempt)
        {
            return _circleSliceable.AttemptInteraction(attempt);
        }

        public Vector2 Position => _circleSliceable.Position;
    }
}