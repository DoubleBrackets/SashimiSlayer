namespace Beatmapping.NoteInteraction.DataTypes
{
    public enum NoteInteractionType
    {
        /// <summary>
        ///     Interaction where the note will try to hit the protag, and the protag must block
        /// </summary>
        Block,

        /// <summary>
        ///     Interaction where the note is vulnerable, and the protag must hit it
        /// </summary>
        Slice
    }
}