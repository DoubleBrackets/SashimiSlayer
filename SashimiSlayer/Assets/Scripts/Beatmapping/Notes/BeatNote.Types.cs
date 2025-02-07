using System;
using UnityEngine;

namespace Beatmapping.Notes
{
    public partial class BeatNote : MonoBehaviour
    {
        public enum TimeSegmentType
        {
            /// <summary>
            ///     Spawning in. Generally buffer time before the note becomes active to do extra stuff
            ///     e.g loading, playing spawn audio early to compensate for buffer latency, etc.
            /// </summary>
            Spawn,

            /// <summary>
            ///     Timing down towards an interaction's target time
            /// </summary>
            Interaction,

            /// <summary>
            ///     No more interactions, timing down to note ending
            /// </summary>
            PreEnding,

            /// <summary>
            ///     In process of ending
            /// </summary>
            Ending,

            /// <summary>
            ///     Special segment that just represents the point where this note will be cleaned up
            /// </summary>
            CleanedUp
        }

        [Flags]
        public enum TickFlags
        {
            None = 0,
            UpdateLocation = 1 << 0,
            TriggerInteractions = 1 << 1,
            All = ~None
        }

        public struct NoteTickInfo
        {
            public double BeatmapTime;
            public double DeltaTime;

            public NoteTimeSegment NoteSegment;

            /// <summary>
            ///     Time since the start of current note segment
            /// </summary>
            public double SegmentTime;

            /// <summary>
            ///     Normalized time since the start of the segment
            /// </summary>
            public double NormalizedSegmentTime;

            /// <summary>
            ///     Time since the start of the note (will be negative if note is spawning)
            /// </summary>
            public double NoteTime;

            /// <summary>
            ///     Normalized time since the start of the note
            /// </summary>
            public double NormalizedNoteTime;

            /// <summary>
            ///     Time since the note has ended (will be negative if note has not ended)
            /// </summary>
            public double TimeSinceNoteEnd;

            /// <summary>
            ///     Interaction that we're currently inside of, if any
            /// </summary>
            public NoteInteraction InsideInteractionWindow;

            /// <summary>
            ///     Interaction that we're currently inside of the PASSING window of, if any
            /// </summary>
            public NoteInteraction InsidePassInteractionWindow;

            /// <summary>
            ///     Index of the segment's interaction, if it exists. -1 otherwise
            /// </summary>
            public int InteractionIndex;

            /// <summary>
            ///     Index of this segment in the sequence of segments
            /// </summary>
            public int SegmentIndex;

            public int SubdivisionIndex;

            public TickFlags Flags;
        }

        public struct NoteTimeSegment
        {
            /// <summary>
            ///     Start time of this time slice, in beatmap timespace
            /// </summary>
            public double SegmentStartTime;

            /// <summary>
            ///     The interaction relevant to this time slice. Should only be set for Interaction type
            ///     slice
            /// </summary>
            public NoteInteraction Interaction;

            public TimeSegmentType Type;
        }
    }
}