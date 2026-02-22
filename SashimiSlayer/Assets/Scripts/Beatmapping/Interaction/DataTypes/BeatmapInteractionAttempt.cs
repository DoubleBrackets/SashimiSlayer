using System.Collections.Generic;
using Beatmapping.Notes;
using CommonTypes;

namespace Beatmapping.Interaction.DataTypes
{
    public class BeatmapInteractionAttempt
    {
        public NoteInteractionType NoteInteractionType;
        public BlockPoses BlockPose;
        public List<BeatNote> Notes;
    }
}