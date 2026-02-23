using System.Collections.Generic;
using System.Linq;
using Beatmapping.Data;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.Notes;
using Beatmapping.Timing;
using UnityEngine;

namespace Beatmapping.NoteManagement
{
    public class BeatNoteManager
    {
        private readonly List<BeatNote> _activeBeatNotes = new();

        private BeatNoteFactory _noteFactory;

        public bool SpawningEnabled { get; set; }

        public bool PauseSpawningForLoopGuard { get; set; }

        public BeatNoteManager(BeatNoteFactory noteFactory)
        {
            _noteFactory = noteFactory;
            SpawningEnabled = true;
            PauseSpawningForLoopGuard = false;
        }

        /// <summary>
        ///     Get the final results of all VALID note interactions for the given attempt
        /// </summary>
        /// <param name="attempt"></param>
        /// <returns></returns>
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

        public List<NoteTickedResults> TickNotes(BeatmapRunner.TickInfo tickInfo, BeatNote.TickFlags tickFlags)
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
            if (!SpawningEnabled || PauseSpawningForLoopGuard)
            {
                return null;
            }

            TimingWindowSO timingWindowSo = beatmap.TimingWindowSO;

            double noteStartTime = data.NoteStartTime;
            double noteBeatLength = data.NoteBeatCount;
            double timeIntervalPerBeat = 60 / beatmap.Bpm;

            // Create interactions from data
            var interactions = new List<NoteInteraction.NoteInteraction>();

            NoteInteraction.NoteInteraction CreateNoteInteraction(SequencedNoteInteraction sequencedInteraction)
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
            BeatNote note = _noteFactory.CreateBeatNote(hitConfig, data, interactions, initalizeTime);

            _activeBeatNotes.Add(note);

            note.OnReadyForCleanup += HandleCleanupRequested;

            return note;
        }

        public void CleanupNote(BeatNote note)
        {
            _activeBeatNotes.Remove(note);
            if (Application.isPlaying)
            {
                GameObject.Destroy(note.gameObject);
            }
            else
            {
                GameObject.DestroyImmediate(note.gameObject);
            }
        }

        private void HandleCleanupRequested(BeatNote note)
        {
            note.OnReadyForCleanup -= HandleCleanupRequested;
            CleanupNote(note);
        }
    }
}