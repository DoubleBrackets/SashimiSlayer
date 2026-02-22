using Interactions.DataTypes;
using UnityEngine;

namespace Interactions.Framework
{
    public interface IInteractable
    {
        InteractionResult AttemptInteraction(InteractionAttempt attempt);
        Vector2 Position { get; }
    }
}