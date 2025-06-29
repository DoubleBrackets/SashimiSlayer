using Beatmapping.Timing;
using EditorUtils.BoldHeader;
using Events;
using Events.Core;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Playables;

namespace Beatmapping
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

        [SerializeField]
        private VoidEvent _playerDeathEvent;

        private bool _inProgress;

        private void Awake()
        {
            _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
            _playerDeathEvent.AddListener(HandlePlayerDeath);

            // Use manual to play with FMOD dsp time
            _director.timeUpdateMode = DirectorUpdateMode.Manual;
        }

        private void OnEnable()
        {
            BeatmapTimeManager.Instance.OnTick += TimeService_OnTick;
        }

        private void OnDisable()
        {
            BeatmapTimeManager.Instance.OnTick -= TimeService_OnTick;
        }

        private void OnDestroy()
        {
            _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
            _playerDeathEvent.RemoveListener(HandlePlayerDeath);
        }

        private void TimeService_OnTick(BeatmapTimeManager.TickInfo tickInfo)
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

        private void HandlePlayerDeath()
        {
            _inProgress = false;
            _director.Pause();
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