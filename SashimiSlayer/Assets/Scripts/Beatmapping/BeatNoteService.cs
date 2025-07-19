using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Timing;
using Core.Protag;
using EditorUtils.BoldHeader;
using Events;
using Events.Core;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
#endif

namespace Beatmapping
{
    public class BeatNoteService : MonoBehaviour
    {
        [BoldHeader("Beat Note Service")]
        [InfoBox("Manages creation, ticking, interactions, and cleanup of beat notes during play")]
        [Header("Events (In)")]

        [SerializeField]
        private ProtagSwordStateEvent _onBlockByProtag;

        [SerializeField]
        private ProtagSwordStateEvent _onSliceByProtag;

        [SerializeField]
        private BoolEvent _setSpawnEnabledEvent;

        [FormerlySerializedAs("_noteSliceResultEvent")]
        [Header("Events (Out)")]

        [SerializeField]
        private SliceResultEvent sliceResultEvent;

        private readonly List<BeatNote> _activeBeatNotes = new();

        private bool _spawningEnabled = true;

        private void Awake()
        {
            _onBlockByProtag.AddListener(OnBlockByProtag);
            _onSliceByProtag.AddListener(OnSliceByProtag);
            _setSpawnEnabledEvent.AddListener(SetSpawningEnabled);
        }

        private void OnEnable()
        {
            var timingService = BeatmapTimeManager.Instance;
            if (timingService == null)
            {
                Debug.LogWarning("Timing service not found");
            }

            timingService.OnTick += TimeManager_OnTick;
        }

        private void OnDisable()
        {
            BeatmapTimeManager.Instance.OnTick -= TimeManager_OnTick;
        }

        private void OnDestroy()
        {
            _onBlockByProtag.RemoveListener(OnBlockByProtag);
            _onSliceByProtag.RemoveListener(OnSliceByProtag);
            _setSpawnEnabledEvent.RemoveListener(SetSpawningEnabled);
        }

        private void SetSpawningEnabled(bool spawningEnabled)
        {
            _spawningEnabled = spawningEnabled;
        }

        private void TimeManager_OnTick(BeatmapTimeManager.TickInfo tickInfo)
        {
            // Time manager ticks are during play mode, so include all flags
            TickNotes(tickInfo, BeatNote.TickFlags.All);
        }

        private void OnBlockByProtag(Protaganist.ProtagSwordState swordState)
        {
            foreach (BeatNote note in _activeBeatNotes)
            {
                note.AttemptPlayerBlock(swordState);
            }
        }

        private void OnSliceByProtag(Protaganist.ProtagSwordState swordState)
        {
            var sliceCount = 0;
            foreach (BeatNote note in _activeBeatNotes)
            {
                bool result = note.AttemptPlayerSlice(swordState);
                if (result)
                {
                    sliceCount++;
                }
            }

            sliceResultEvent.Raise(new SliceResultData(sliceCount, SliceResultData.SlicedObject.Note));
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
                hitConfig.DamageDealtToPlayer
            );

            _activeBeatNotes.Add(note);

            note.OnReadyForCleanup += HandleCleanupRequested;

            return note;
        }

        private void TickNotes(BeatmapTimeManager.TickInfo tickInfo, BeatNote.TickFlags tickFlags)
        {
            BeatNote[] hits = _activeBeatNotes.ToArray();
            foreach (BeatNote hit in hits)
            {
                hit.Tick(tickInfo, tickFlags);
            }
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