using Beatmapping.Events;
using Beatmapping.Timing;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace Beatmapping.Service
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

        [Header("Events (In)")]

        [SerializeField]
        private BeatmapEvent _beatmapLoadedEvent;

        private bool _inProgress;

        private void Awake()
        {
            _beatmapLoadedEvent.AddListener(HandleStartBeatmap);

            // Use manual to play with FMOD dsp time
            _director.timeUpdateMode = DirectorUpdateMode.Manual;
        }

        private void OnDestroy()
        {
            _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
        }

        public void TickTimelineRunner(BeatmapTimeManager.TickInfo tickInfo)
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

        private void HandleStartBeatmap(BeatmapConfigSo beatmap)
        {
            _director.playableAsset = beatmap.BeatmapTimeline;
            _director.Play();
            _director.Evaluate();
            _inProgress = true;
        }
    }
}