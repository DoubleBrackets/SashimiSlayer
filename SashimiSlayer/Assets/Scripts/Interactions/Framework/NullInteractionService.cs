using Interactions.DataTypes;

namespace Interactions.Framework
{
    public class NullInteractionService : IInteractionService
    {
        public void Register(IInteractable interactable)
        {
        }

        public void Unregister(IInteractable interactable)
        {
        }

        public InteractionFinalResults AttemptInteraction(InteractionAttempt attempt)
        {
            return InteractionFinalResults.Empty();
        }
    }
}