using System;
using CommonTypes;
using Interactions.DataTypes;
using Interactions.Framework;
using UnityEngine;

namespace Interactions.Interactables
{
    public class Blockable : IInteractable
    {
        public event Action OnBlocked;

        private InteractionService _interactionService;

        private BlockPoses _blockPose;
        private bool _singleUse;
        private bool _blocked;
        private Transform _hitCenter;

        public bool Enabled { get; set; } = true;

        public Blockable(InteractionService interactionService, BlockPoses blockPose, bool singleUse,
            Transform hitCenter)
        {
            _interactionService = interactionService;
            _interactionService.Register(this);
            _blockPose = blockPose;
            _singleUse = singleUse;
            _hitCenter = hitCenter;
        }

        public void Cleanup()
        {
            _interactionService.Unregister(this);
        }

        public InteractionResult AttemptInteraction(InteractionAttempt attempt)
        {
            if (!Enabled || (_singleUse && _blocked))
            {
                return InteractionResult.Fail(this);
            }

            if (attempt.Type != InteractionType.Block)
            {
                return InteractionResult.Fail(this);
            }

            if (attempt.BlockPose != _blockPose)
            {
                return InteractionResult.Fail(this);
            }

            _blocked = true;

            OnBlocked?.Invoke();

            return new InteractionResult
            {
                Successful = true,
                Interactable = this
            };
        }

        public Vector2 Position => _hitCenter.position;
    }
}