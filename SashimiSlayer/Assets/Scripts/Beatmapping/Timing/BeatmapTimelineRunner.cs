using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace Beatmapping.Timing
{
    /// <summary>
    ///     Handles the timeline playback in step with the beatmap music FMOD event
    /// </summary>
    public class BeatmapTimelineRunner : MonoBehaviour
    {
        [BoldHeader("Beatmap Timeline Runner")]
        [InfoBox("Handles the timeline playback and ticking in coordination with beatmap")]
        [SerializeField]
        private PlayableDirector _director;

        private bool _inProgress;

        public void TickTimelineRunner(BeatmapRunner.TickInfo tickInfo)
        {
            if (_inProgress)
            {
                _director.time = tickInfo.CurrentLevelTime;

                // Check for end of startup
                if (_director.time >= _director.duration)
                {
                    _director.Stop();
                }
            }

            _director.Evaluate();
        }

        public void StartRunningBeatmap(BeatmapConfigSo beatmap)
        {
            _director.timeUpdateMode = DirectorUpdateMode.Manual;
            _director.playableAsset = beatmap.BeatmapTimeline;
            _director.Play();
            _director.Evaluate();
            _inProgress = true;
        }
    }
}