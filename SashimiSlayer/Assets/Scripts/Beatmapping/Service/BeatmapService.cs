using System.Collections.Generic;
using System.Linq;
using Beatmapping.Data;
using Beatmapping.Events;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.NoteManagement;
using Beatmapping.Notes;
using Beatmapping.Timing;
using Beatmapping.Timing.LoopHandler;
using FMOD.Studio;
using FMODUnity;
using Framework;
using GameInput.ValueSO;
using Interactions.Framework;
using NaughtyAttributes;
using UnityEngine;
using ValueSO.Core;

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

        [Header("ValueSO (Read)")]

        [SerializeField]
        private SwordHandednessValueSO _currentHandednessValueSO;

        [SerializeField]
        private BoolValueSO _invertParryInput;

        [Header("Dependencies")]

        [SerializeField]
        private BeatmapRunner _beatmapRunner;

        [SerializeField]
        private Transform _noteParentTransform;

        [Header("FMODParams")]

        [SerializeField]
        [ParamRef]
        private string _successStreakParam;

        [Header("Events (Out)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        private IInteractionService _interactionService;
        private BeatNoteManager _beatNoteManager;
        public BeatmapConfigSo CurrentBeatmap => _currentBeatmap;

        private BeatmapConfigSo _currentBeatmap;

        private bool _initialized;

        private int _currentSuccessStreak;

        private ILoopHandler _loopHandler;

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
                _loopHandler = new BeatmapLoopHandler(_successStreakParam);
            }
            else
            {
                // Mock it
                _interactionService = new NullInteractionService();
                _loopHandler = new NullLoopHandler();
            }

            var noteFactory = new BeatNoteFactory(_noteParentTransform, _interactionService);
            _beatNoteManager = new BeatNoteManager(noteFactory);

            _loopHandler.OnPauseNoteSpawning += HandlePauseNoteSpawning;

            _beatmapRunner.Initialize();
        }

        public void BeginRunningBeatmap(BeatmapConfigSo beatmap)
        {
            _currentBeatmap = beatmap;
            _beatmapRunner.BeginRunningBeatmap(beatmap, out EventInstance soundtrackInstance);

            _loopHandler.SetupNewBeatmap(soundtrackInstance);

            _currentSuccessStreak = 0;
        }

        public void ToggleSkipLoop()
        {
            _loopHandler.ToggleSkipLoops();
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

            if (_loopHandler != null)
            {
                _loopHandler.OnPauseNoteSpawning -= HandlePauseNoteSpawning;
            }

            _beatmapRunner.Cleanup();

            _initialized = false;
        }

        private void HandlePauseNoteSpawning(bool pause)
        {
            _beatNoteManager.PauseSpawningForLoopGuard = pause;
        }

        public BeatmapTickFinalResults TickForward()
        {
            bool isRunning = _beatmapRunner.TickForward();

            List<NoteTickedResults> noteTickResults =
                _beatNoteManager.TickNotes(_beatmapRunner.CurrentTickInfo, BeatNote.TickFlags.All);

            IList<NoteInteractionFinalResult> interactionResults = noteTickResults
                .Where(r => r.HasInteractionFinalResult)
                .Select(r => r.InteractionFinalResult)
                .ToList();

            foreach (NoteInteractionFinalResult result in interactionResults)
            {
                ProcessInteractionRes(result);
            }

            _loopHandler.Tick(_beatmapRunner.CurrentTickInfo);

            return new BeatmapTickFinalResults
            {
                NoteTickedResults = noteTickResults,
                BeatmapOver = !isRunning
            };
        }

        public IList<NoteInteractionFinalResult> GetNoteInteractionFinalResults(BeatmapInteractionAttempt attempt)
        {
            IList<NoteInteractionFinalResult> results = _beatNoteManager.GetNoteInteractionFinalResults(attempt);

            foreach (NoteInteractionFinalResult result in results)
            {
                ProcessInteractionRes(result);
            }

            return results;
        }

        private void ProcessInteractionRes(NoteInteractionFinalResult result)
        {
            _noteInteractionFinalResultEvent.Raise(result);

            if (result.Successful)
            {
                _currentSuccessStreak++;
            }
            else
            {
                _currentSuccessStreak = 0;
            }

            _loopHandler.SetSuccessStreak(_currentSuccessStreak);
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

        private void OnGUI()
        {
            if (_loopHandler.IsSkippingLoops)
            {
                GUILayout.TextField("~");
            }
        }
    }
}