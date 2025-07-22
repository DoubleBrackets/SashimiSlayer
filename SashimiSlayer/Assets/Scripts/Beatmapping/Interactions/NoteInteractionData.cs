using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beatmapping.Interactions
{
    /// <summary>
    ///     Data for serializing a note interaction
    /// </summary>
    [Serializable]
    public struct NoteInteractionData
    {
        [AllowNesting]
        [Tooltip("The correct block pose, if this interaction is an attack that can be blocked")]
        [HideIf("InteractionType", NoteInteraction.InteractionType.Slice)]
        public SharedTypes.BlockPoseStates BlockPose;

        [Tooltip("The type of interaction this is")]
        public NoteInteraction.InteractionType InteractionType;

        [Tooltip("Additional interaction flags")]
        [HideInInspector]
        public NoteInteraction.InteractionFlags Flags;

        [Tooltip("Hide Timing Indicator")]
        public bool HideIndicator;

        [FormerlySerializedAs("InteractionPositions")]
        [Tooltip("Positions for the interaction")]
        public List<Vector2> Positions;

        public NoteInteraction ToNoteInteraction(TimingWindow window)
        {
            return new NoteInteraction(InteractionType, Flags, BlockPose, Positions, window, HideIndicator);
        }
    }
}