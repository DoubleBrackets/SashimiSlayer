using System;
using CommonTypes;
using Framework;
using Interactions.DataTypes;
using Interactions.Framework;
using Interactions.Interactables;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions.Components
{
    public class BlockableMono : MonoBehaviour, IInteractable
    {
        [Header("Config")]

        [SerializeField]
        private bool _singleUse;

        [SerializeField]
        private BlockPoses _blockPose;

        [Header("UnityEvents")]

        [SerializeField]
        private UnityEvent _onBlockedEvent;

        public event Action OnBlocked;

        private SingleDirBlockable _blockable;

        private IInteractionService _interactionService;

        public bool Enabled
        {
            get => _blockable.Enabled;
            set => _blockable.Enabled = value;
        }

        private void OnEnable()
        {
            _interactionService = ServiceLocator.GetService<IInteractionService>();
            _blockable = new SingleDirBlockable(this, _blockPose, _singleUse);

            _blockable.OnBlocked += HandleOnBlocked;

            _interactionService.Register(this);
        }

        private void OnDisable()
        {
            _blockable.OnBlocked -= HandleOnBlocked;

            _interactionService.Unregister(this);
        }

        private void HandleOnBlocked()
        {
            _onBlockedEvent?.Invoke();
            OnBlocked?.Invoke();
        }

        public InteractionResult AttemptInteraction(InteractionAttempt attempt)
        {
            return _blockable.AttemptInteraction(attempt);
        }

        public Vector2 Position => transform.position;
    }
}