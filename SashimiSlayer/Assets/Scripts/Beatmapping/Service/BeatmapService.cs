using System.Collections.Generic;
using Beatmapping.Data;
using Beatmapping.Interaction.DataTypes;
using Beatmapping.Notes;
using Beatmapping.Timing;
using Framework;
using Interactions.Framework;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Service
{
    /// <summary>
    ///     Top level controller for beatmap gameplay. This needs to execute in edit mode for beatmap editing purposes
    /// </summary>
    [ExecuteInEditMode]
    public class BeatmapService : MonoBehaviour, IBeatmapService
    {
        [InfoBox("Top level controller for beatmap gameplay")]
        [SerializeField]
        private BeatmapServiceContainerSO _beatmapServiceContainer;

        [Header("Dependencies")]

        [SerializeField]
        private BeatNoteService _beatNoteService;

        [SerializeField]
        private BeatmapTimeManager _timeManager;

        private InteractionService _interactionService;

        private void Awake()
        {
            _interactionService = ServiceLocator.GetService<InteractionService>();

            _beatNoteService.Initialize(_interactionService);
            _beatmapServiceContainer.RegisterImplementation(this);
        }

        private void OnDestroy()
        {
            _beatmapServiceContainer.DeregisterImplementation();
        }

        public BeatmapTickFinalResults TickForward()
        {
            _timeManager.TickForward();
            List<NoteTickedResults> noteTickResults =
                _beatNoteService.TickNotes(_timeManager.CurrentTickInfo, BeatNote.TickFlags.All);
            return new BeatmapTickFinalResults
            {
                NoteTickedResults = noteTickResults
            };
        }

        public IList<NoteInteractionFinalResult> GetNoteInteractionFinalResults(BeatmapInteractionAttempt attempt)
        {
            return _beatNoteService.GetNoteInteractionFinalResults(attempt);
        }
    }
}