using System;
using CommonTypes;
using Interactions.DataTypes;
using Interactions.Framework;

namespace Interactions.Interactables
{
    public class SingleDirBlockable
    {
        public event Action OnBlocked;

        private IInteractionService _interactionService;

        private BlockPoses _blockPose;
        private bool _singleUse;
        private bool _blocked;
        private IInteractable _sourceInteractable;

        public bool Enabled { get; set; } = true;

        public SingleDirBlockable(IInteractable sourceInteractable, BlockPoses blockPose, bool singleUse)
        {
            _blockPose = blockPose;
            _singleUse = singleUse;
            _sourceInteractable = sourceInteractable;
        }

        public InteractionResult AttemptInteraction(InteractionAttempt attempt)
        {
            if (!Enabled || (_singleUse && _blocked))
            {
                return InteractionResult.Fail(_sourceInteractable);
            }

            if (attempt.Type != InteractionType.Block)
            {
                return InteractionResult.Fail(_sourceInteractable);
            }

            if (attempt.BlockPose != _blockPose)
            {
                return InteractionResult.Fail(_sourceInteractable);
            }

            _blocked = true;

            OnBlocked?.Invoke();

            return new InteractionResult
            {
                Successful = true,
                Interactable = _sourceInteractable
            };
        }
    }
}