using System.Collections.Generic;
using Beatmapping.Data;
using Beatmapping.Events;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.NoteManagement;
using Beatmapping.Notes;
using Beatmapping.Timing;
using Events.Basic;
using Framework;
using Interactions.Framework;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Service
{
    /// <summary>
    ///     Top level controller for beatmap gameplay. This needs to execute in edit mode for beatmap editing purposes
    /// </summary>
    [ExecuteAlways]
    public class BeatmapService : MonoBehaviour, IBeatmapService
    {
        [InfoBox("Top level controller for beatmap gameplay")]
        [SerializeField]
        private BeatmapServiceContainerSO _beatmapServiceContainer;

        [Header("Dependencies")]

        [SerializeField]
        private BeatmapRunner _beatmapRunner;

        [SerializeField]
        private Transform _noteParentTransform;

        [Header("Channels (In)")]

        [SerializeField]
        private BoolEvent _setNoteSpawnEnabledEvent;

        [Header("Events (Out)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        private IInteractionService _interactionService;
        private BeatNoteManager _beatNoteManager;
        public BeatmapConfigSo CurrentBeatmap => _currentBeatmap;

        private BeatmapConfigSo _currentBeatmap;

        private bool _initialized;

        /// <summary>
        ///     We want to initialize this in both edit and play mode to support editor beatmapping, but awake
        ///     does not call after script reloads in edit mode, so use OnValidate for that case
        /// </summary>
        private void Awake()
        {
            TryInitialize();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying)
            {
                // Weird hack, since script reloads don't reinstantiate the object (i.e _initialized is still true)
                Cleanup();
                TryInitialize();
            }
        }

        private void TryInitialize()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;
            Debug.Log($"Initializing BeatmapService in {(Application.isPlaying ? "play mode" : "edit mode")}");

            if (Application.isPlaying)
            {
                _interactionService = ServiceLocator.GetService<IInteractionService>();
                _beatmapServiceContainer.RegisterImplementation(this);
            }
            else
            {
                // Mock it
                _interactionService = new NullInteractionService();
            }

            var noteFactory = new BeatNoteFactory(_noteParentTransform, _interactionService);
            _beatNoteManager = new BeatNoteManager(noteFactory);

            _setNoteSpawnEnabledEvent.AddListener(HandleSetNoteSpawningEnabled);
            _beatmapRunner.Initialize();
        }

        public void BeginRunningBeatmap(BeatmapConfigSo beatmap)
        {
            _currentBeatmap = beatmap;
            _beatmapRunner.BeginRunningBeatmap(beatmap);
        }

        private void HandleSetNoteSpawningEnabled(bool noteSpawningEnabled)
        {
            _beatNoteManager.SpawningEnabled = noteSpawningEnabled;
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            if (!_initialized)
            {
                return;
            }

            _beatmapServiceContainer.DeregisterImplementation();
            _beatmapRunner.Cleanup();

            _initialized = false;
        }

        public BeatmapTickFinalResults TickForward()
        {
            _beatmapRunner.TickForward();
            List<NoteTickedResults> noteTickResults =
                _beatNoteManager.TickNotes(_beatmapRunner.CurrentTickInfo, BeatNote.TickFlags.All);
            return new BeatmapTickFinalResults
            {
                NoteTickedResults = noteTickResults
            };
        }

        public IList<NoteInteractionFinalResult> GetNoteInteractionFinalResults(BeatmapInteractionAttempt attempt)
        {
            IList<NoteInteractionFinalResult> results = _beatNoteManager.GetNoteInteractionFinalResults(attempt);

            foreach (NoteInteractionFinalResult result in results)
            {
                _noteInteractionFinalResultEvent.Raise(result);
            }

            return results;
        }

        public BeatNote SpawnNote(BeatNoteTypeSO hitConfig,
            BeatNoteData data,
            BeatmapConfigSo beatmap,
            double initalizeTime)
        {
            return _beatNoteManager.SpawnNote(hitConfig, data, beatmap, initalizeTime);
        }

        public void CleanupNote(BeatNote beatNote)
        {
            _beatNoteManager.CleanupNote(beatNote);
        }
    }
}