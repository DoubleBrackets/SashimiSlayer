using System.Collections.Generic;

namespace Interactions.DataTypes
{
    public struct InteractionFinalResults
    {
        public InteractionType InteractionType { get; set; }
        public List<InteractionResult> SuccessfulInteractions { get; set; }
    }
}