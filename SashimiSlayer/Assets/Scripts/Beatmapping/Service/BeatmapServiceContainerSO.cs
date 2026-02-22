using System.Collections.Generic;
using Beatmapping.Data;
using Beatmapping.Interaction.DataTypes;
using UnityEngine;

namespace Beatmapping.Service
{
    /// <summary>
    ///     Used to reference beatmap service cross-scene or when it is not available. Null object pattern
    /// </summary>
    [CreateAssetMenu(fileName = "Beatmap Service Container", menuName = "Beatmapping/Beatmap Service Container")]
    public class BeatmapServiceContainerSO : ScriptableObject, IBeatmapService
    {
        private IBeatmapService _implementation;

        public void RegisterImplementation(IBeatmapService implementation)
        {
            _implementation = implementation;
        }

        public void DeregisterImplementation()
        {
            _implementation = null;
        }

        public IList<NoteInteractionFinalResult> GetNoteInteractionFinalResults(BeatmapInteractionAttempt attempt)
        {
            return _implementation?.GetNoteInteractionFinalResults(attempt) ?? new List<NoteInteractionFinalResult>();
        }

        public BeatmapTickFinalResults TickForward()
        {
            return _implementation?.TickForward() ?? new BeatmapTickFinalResults
            {
                NoteTickedResults = new List<NoteTickedResults>(),
                BeatmapOver = false
            };
        }
    }
}