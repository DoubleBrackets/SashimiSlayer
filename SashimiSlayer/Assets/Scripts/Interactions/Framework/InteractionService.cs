using System.Collections.Generic;
using Interactions.DataTypes;

namespace Interactions.Framework
{
    public class InteractionService : IInteractionService
    {
        private List<IInteractable> _interactables = new();

        public void Register(IInteractable interactable)
        {
            _interactables.Add(interactable);
        }

        public void Unregister(IInteractable interactable)
        {
            _interactables.Remove(interactable);
        }

        public InteractionFinalResults AttemptInteraction(InteractionAttempt attempt)
        {
            List<InteractionResult> results = new();

            // Make a copy in case the interactable unregisters itself
            var currentInteractables = new List<IInteractable>(_interactables);

            foreach (IInteractable interactable in currentInteractables)
            {
                InteractionResult result = interactable.AttemptInteraction(attempt);
                if (result.Successful)
                {
                    results.Add(result);
                }
            }

            var finalResults = new InteractionFinalResults
            {
                InteractionType = attempt.Type,
                SuccessfulInteractions = results
            };

            return finalResults;
        }
    }
}