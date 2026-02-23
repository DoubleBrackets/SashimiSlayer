using Beatmapping.Notes;
using CommonTypes;

namespace Beatmapping.NoteInteraction.DataTypes
{
    /// <summary>
    ///     The final result of a single note interaction evaluation; either a success when it occurs, or a failure after the
    ///     timing
    ///     window ends
    /// </summary>
    public struct NoteInteractionFinalResult
    {
        public TimingWindow.TimingResult TimingResult;
        public NoteInteractionType NoteInteractionType;
        public BlockPoses Pose;
        public bool Successful;
        public BeatNote Note;

        public NoteInteractionFinalResult(
            BeatNote note,
            TimingWindow.TimingResult timingResult,
            NoteInteractionType noteInteractionType,
            bool successful,
            BlockPoses pose = default)
        {
            TimingResult = timingResult;
            NoteInteractionType = noteInteractionType;
            Successful = successful;
            Pose = pose;
            Note = note;
        }
    }
}