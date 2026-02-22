using System.Collections.Generic;
using System.Linq;
using Beatmapping.Data;
using Beatmapping.Interaction.DataTypes;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Timing;
using EditorUtils.BoldHeader;
using Events.Basic;
using Interactions.Framework;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Service
{
    public class BeatNoteService : MonoBehaviour
    {
        [BoldHeader("Beat Note Service")]
        [InfoBox("Manages creation, ticking, interactions, and cleanup of beat notes during play")]
        [Header("Channels (In)")]

        [SerializeField]
        private BoolEvent _setSpawnEnabledEvent;

        [Header("Events (Out)")]

        private readonly List<BeatNote> _activeBeatNotes = new();

        private bool _spawningEnabled = true;
        public BeatmapConfigSo CurrentBeatmap { get; set; }

        private InteractionService _interactionService;

        private void Awake()
        {
            _setSpawnEnabledEvent.AddListener(SetSpawningEnabled);
        }

        private void OnDestroy()
        {
            _setSpawnEnabledEvent.RemoveListener(SetSpawningEnabled);
        }

        public void Initialize(InteractionService interactionService)
        {
            _interactionService = interactionService;
        }

        private void SetSpawningEnabled(bool spawningEnabled)
        {
            _spawningEnabled = spawningEnabled;
        }

        public IList<NoteInteractionFinalResult> GetNoteInteractionFinalResults(BeatmapInteractionAttempt attempt)
        {
            var results = new List<NoteInteractionFinalResult>();
            HashSet<BeatNote> notesToCheck = attempt.Notes.ToHashSet();
            foreach (BeatNote note in _activeBeatNotes)
            {
                if (notesToCheck.Contains(note) &&
                    note.EvaluateInteraction(attempt, out NoteInteractionFinalResult result))
                {
                    results.Add(result);
                }
            }

            return results;
        }

        public List<NoteTickedResults> TickNotes(BeatmapTimeManager.TickInfo tickInfo, BeatNote.TickFlags tickFlags)
        {
            var results = new List<NoteTickedResults>();

            BeatNote[] hits = _activeBeatNotes.ToArray();
            foreach (BeatNote hit in hits)
            {
                results.Add(hit.Tick(tickInfo, tickFlags));
            }

            return results;
        }

        public BeatNote SpawnNote(BeatNoteTypeSO hitConfig,
            BeatNoteData data,
            BeatmapConfigSo beatmap,
            double initalizeTime)
        {
            if (!_spawningEnabled)
            {
                return null;
            }

            TimingWindowSO timingWindowSo = beatmap.TimingWindowSO;

            double noteStartTime = data.NoteStartTime;
            double noteBeatLength = data.NoteBeatCount;
            double timeIntervalPerBeat = 60 / beatmap.Bpm;

            // Create interactions from data
            var interactions = new List<NoteInteraction>();

            NoteInteraction CreateNoteInteraction(SequencedNoteInteraction sequencedInteraction)
            {
                NoteInteractionData interactionData = sequencedInteraction.InteractionData;
                double beatsFromStart = sequencedInteraction.GetBeatsFromNoteStart(noteBeatLength);

                TimingWindow timingWindow = timingWindowSo.CreateTimingWindow(
                    noteStartTime + beatsFromStart * timeIntervalPerBeat);

                return interactionData.ToNoteInteraction(
                    timingWindow);
            }

            foreach (SequencedNoteInteraction sequencedInteraction in data.Interactions)
            {
                interactions.Add(CreateNoteInteraction(sequencedInteraction));
            }

            // Sort, in case they were configured out of chronological order
            interactions.Sort((a, b) => a.TargetTime.CompareTo(b.TargetTime));

            // Instantiate note and register
            BeatNote note = Instantiate(hitConfig.Prefab, transform);
            note.Initialize(
                interactions,
                data.StartPosition,
                data.EndPosition,
                data.NoteStartTime,
                data.NoteEndTime,
                initalizeTime,
                hitConfig.HitboxRadius,
                _interactionService
            );

            _activeBeatNotes.Add(note);

            note.OnReadyForCleanup += HandleCleanupRequested;

            return note;
        }

        public void CleanupNote(BeatNote note)
        {
            _activeBeatNotes.Remove(note);
            if (Application.isPlaying)
            {
                Destroy(note.gameObject);
            }
            else
            {
                DestroyImmediate(note.gameObject);
            }
        }

        private void HandleCleanupRequested(BeatNote note)
        {
            note.OnReadyForCleanup -= HandleCleanupRequested;
            CleanupNote(note);
        }
    }
}