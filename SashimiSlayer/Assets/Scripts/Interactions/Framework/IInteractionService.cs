using Interactions.DataTypes;

namespace Interactions.Framework
{
    public interface IInteractionService
    {
        public void Register(IInteractable interactable);

        public void Unregister(IInteractable interactable);

        public InteractionFinalResults AttemptInteraction(InteractionAttempt attempt);
    }
}