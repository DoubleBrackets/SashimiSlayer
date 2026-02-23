using Beatmapping.NoteInteraction.DataTypes;

namespace Beatmapping.Data
{
    /// <summary>
    ///     Results of ticking a note forwards
    /// </summary>
    public struct NoteTickedResults
    {
        public bool HasInteractionFinalResult;
        public bool TargetHit;
        public NoteInteractionFinalResult InteractionFinalResult;

        public static NoteTickedResults Empty => new()
        {
            HasInteractionFinalResult = false,
            TargetHit = false,
            InteractionFinalResult = default
        };
    }
}