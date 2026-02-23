using Beatmapping.NoteInteraction.DataTypes;
using Events;
using UnityEngine;

namespace Beatmapping.Events
{
    [CreateAssetMenu(menuName = "Events/Beatmap/NoteInteractionFinalResultEvent")]
    public class NoteInteractionFinalResultEvent : SOEvent<NoteInteractionFinalResult>
    {
    }
}