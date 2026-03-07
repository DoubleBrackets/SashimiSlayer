using Interactions.Framework;

namespace Interactions.DataTypes
{
    public struct InteractionResult
    {
        public bool Successful { get; set; }
        public IInteractable Interactable { get; set; }

        public static InteractionResult Fail(IInteractable interactable)
        {
            return new InteractionResult { Successful = false, Interactable = interactable };
        }

        public static InteractionResult Success(IInteractable interactable)
        {
            return new InteractionResult { Successful = true, Interactable = interactable };
        }
    }
}