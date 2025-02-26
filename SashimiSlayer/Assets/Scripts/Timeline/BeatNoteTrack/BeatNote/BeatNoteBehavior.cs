using System;
using Beatmapping;
using Beatmapping.Notes;
using Beatmapping.Timing;
using Beatmapping.Tooling;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Serialization;

namespace Timeline.BeatNoteTrack.BeatNote
{
    [Serializable]
    public class BeatNoteBehavior : PlayableBehaviour
    {
        [FormerlySerializedAs("HitConfig")]
        public BeatNoteTypeSO NoteConfig;

        public BeatNoteData NoteData;

        private BeatNoteService _beatNoteService;

        private Beatmapping.Notes.BeatNote _beatNote;

        /// <summary>
        ///     Process the frame. This should be called by the BeatNoteMixer
        /// </summary>
        public void ProcessFrameMixer(double beatmapTime,
            FrameData info,
            BeatmapConfigSo beatmap,
            BeatNoteService service)
        {
            if (_beatNoteService == null)
            {
                _beatNoteService = service;

                if (_beatNoteService == null)
                {
                    return;
                }
            }

            double currentBeatmapTime = beatmapTime;

            if (_beatNote == null)
            {
                _beatNote = _beatNoteService.SpawnNote(
                    NoteConfig,
                    NoteData,
                    beatmap,
                    currentBeatmapTime);
            }

            // Debug.DrawLine(NoteData.Positions[0], NoteData.Positions[0] + Vector2.up * 10, Color.red);
        }

        public override void OnBehaviourPause(Playable playable, FrameData info)
        {
            // Cleanup();
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Cleanup();
        }

        public void EditorTick(FrameData info, double currentBeatmapTime)
        {
            // Ticking from the mixer is only for previewing in the editor
            if (!Application.isPlaying && _beatNote != null)
            {
                _beatNote.Tick(new BeatmapTimeManager.TickInfo
                {
                    BeatmapTime = currentBeatmapTime,
                    CurrentBeatmap = BeatmappingUtilities.CurrentEditingBeatmapConfig
                }, Beatmapping.Notes.BeatNote.TickFlags.UpdateLocation);

                if (currentBeatmapTime < NoteData.NoteStartTime)
                {
                    Cleanup();
                }
            }
        }

        private void Cleanup()
        {
            // During play mode the note and note manager handle cleanup
            if (Application.isPlaying)
            {
                return;
            }

            if (_beatNote == null)
            {
                return;
            }

            _beatNoteService.CleanupNote(_beatNote);
            _beatNote = null;
        }
    }
}