using System.Collections.Generic;
using Beatmapping.NoteInteraction.DataTypes;

namespace Beatmapping.Tooling
{
    public interface INoteInteractionUser
    {
        public IEnumerable<InteractionUsage> GetInteractionUsages();

        /// <summary>
        ///     What interactions and positions does this listener use?
        ///     This lets specific interaction "outputs" declare what interactions, types, and positions it needs to function
        /// </summary>
        public struct InteractionUsage
        {
            public int InteractionIndex;
            public NoteInteractionType NoteInteractionType;
            public int PositionCount;

            public InteractionUsage(NoteInteractionType noteInteractionType, int interactionIndex,
                int positionCount)
            {
                InteractionIndex = interactionIndex;
                PositionCount = positionCount;
                NoteInteractionType = noteInteractionType;
            }
        }
    }
}