using Beatmapping;
using Beatmapping.Tooling;
using Core.Scene;
using Timeline.BeatNoteTrack.BeatNote;
using UnityEngine;
using UnityEngine.Playables;

// The runtime instance of a the TextTrack. It is responsible for blending and setting the final data
// on the Text binding
namespace Timeline.BeatNoteTrack
{
    public class BeatNoteMixer : PlayableBehaviour
    {
        private BeatNoteService _mTrackBinding;

        // Called every frame that the timeline is evaluated. ProcessFrame is invoked after its' inputs.
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            _mTrackBinding = playerData as BeatNoteService;
            if (_mTrackBinding == null)
            {
                Debug.LogWarning("BeatNoteMixer: Track binding is null");
            }

            BeatmapConfigSo beatmap;
            if (Application.isPlaying)
            {
                beatmap = LevelLoader.Instance.CurrentLevel.Beatmap;
            }
            else
            {
                beatmap = BeatmappingUtilities.CurrentEditingBeatmapConfig;
            }

            double currentTime = playable.GetTime();
            double currentBeatmapTime = currentTime - beatmap.StartTime;

            // If scrubbing in editing mode, cache the playhead time
            if (!Application.isPlaying)
            {
                BeatmappingUtilities.TimelinePlayheadTime = currentTime;
            }

            // Iterate through all the notes in the track and process frame
            int inputCount = playable.GetInputCount();
            for (var i = 0; i < inputCount; i++)
            {
                float inputWeight = playable.GetInputWeight(i);
                var inputPlayable = (ScriptPlayable<BeatNoteBehavior>)playable.GetInput(i);
                BeatNoteBehavior input = inputPlayable.GetBehaviour();

                if (inputWeight > 0)
                {
                    input.ProcessFrameMixer(currentBeatmapTime, info, beatmap, playerData as BeatNoteService);
                }

                input.EditorTick(info, currentBeatmapTime);
            }
        }
    }
}