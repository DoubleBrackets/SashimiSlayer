using System;
using System.Collections.Generic;
using Beatmapping.Interaction.DataTypes;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Notes
{
    /// <summary>
    ///     Data for serializing a beat note instance
    /// </summary>
    [Serializable]
    public struct BeatNoteData
    {
        public Vector2 StartPosition;
        public Vector2 EndPosition;

        [Header("Timing (Beatmap space)")]

        [ReadOnly]
        [AllowNesting]
        public double NoteStartTime;

        [ReadOnly]
        [AllowNesting]
        public double NoteEndTime;

        [ReadOnly]
        [AllowNesting]
        public double NoteBeatCount;

        [SerializeField]
        public List<SequencedNoteInteraction> Interactions;
    }

    [Serializable]
    public struct SequencedNoteInteraction
    {
        [Tooltip("Beat offset")]
        [SerializeField]
        private double _beatOffset;

        [Tooltip("If true, the beat offset is from the end of the note")]
        [SerializeField]
        private bool _offsetFromEnd;

        public NoteInteractionData InteractionData;

        /// <summary>
        ///     Get the interaction beat offset from the start of the note
        /// </summary>
        /// <param name="noteBeatLength"></param>
        /// <returns></returns>
        public double GetBeatsFromNoteStart(double noteBeatLength)
        {
            return _offsetFromEnd ? noteBeatLength - _beatOffset : _beatOffset;
        }
    }
}