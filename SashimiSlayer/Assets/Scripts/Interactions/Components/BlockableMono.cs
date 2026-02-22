using System;
using CommonTypes;
using Framework;
using Interactions.Framework;
using Interactions.Interactables;
using UnityEngine;
using UnityEngine.Events;

namespace Interactions.Components
{
    public class BlockableMono : MonoBehaviour
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

        private Blockable _blockable;

        public bool Enabled
        {
            get => _blockable.Enabled;
            set => _blockable.Enabled = value;
        }

        private void OnEnable()
        {
            _blockable = new Blockable(
                ServiceLocator.GetService<InteractionService>(),
                _blockPose,
                _singleUse,
                transform
            );

            _blockable.OnBlocked += HandleOnBlocked;
        }

        private void OnDisable()
        {
            _blockable.OnBlocked -= HandleOnBlocked;
            _blockable.Cleanup();
        }

        private void HandleOnBlocked()
        {
            _onBlockedEvent?.Invoke();
            OnBlocked?.Invoke();
        }
    }
}