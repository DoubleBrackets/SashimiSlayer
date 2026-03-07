using System;
using DG.Tweening;
using Events.Basic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Framework;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Beatmapping.Timing
{
    /// <summary>
    ///     Handles running the beatmap, the audio, and syncing timing
    /// </summary>
    public class BeatmapRunner : MonoBehaviour
    {
        private const double MinDeltaTime = 0.05f;

        private enum BeatmapState
        {
            Unloaded,
            WaitingForSoundtrackEvent,
            Playing
        }

        /// <summary>
        ///     Contains all the timing information about the current tick
        /// </summary>
        public struct TickInfo
        {
            /// <summary>
            ///     DSP time since beatmap started (the 0th beat, different from load time)
            /// </summary>
            public double BeatmapTime;

            /// <summary>
            ///     DSP time in level timespace (since the level began)
            /// </summary>
            public double CurrentLevelTime;

            /// <summary>
            ///     The beatmap of this tick
            /// </summary>
            public BeatmapConfigSo CurrentBeatmap;

            /// <summary>
            ///     The subdivision index of this tick
            /// </summary>
            public int SubdivIndex;

            /// <summary>
            ///     Whether the beat crossed a beat this tick
            /// </summary>
            public bool CrossedSubdivThisTick;

            public bool CrossedBeatThisTick => CrossedSubdivThisTick && SubdivIndex % CurrentBeatmap.Subdivisions == 0;

            /// <summary>
            ///     Given a beatmap time, get the closest subdivision index
            /// </summary>
            /// <param name="beatmapTime"></param>
            /// <returns></returns>
            public int GetClosestSubdivisionIndex(double beatmapTime)
            {
                double timeIntervalPerSubdiv = 60 / CurrentBeatmap.Bpm / CurrentBeatmap.Subdivisions;
                return (int)Math.Round(beatmapTime / timeIntervalPerSubdiv);
            }

            public int GetClosestBeatIndex(double beatmapTime)
            {
                double timeIntervalPerBeat = 60 / CurrentBeatmap.Bpm;
                return (int)Math.Round(beatmapTime / timeIntervalPerBeat);
            }
        }

        [Header("Dependencies")]

        [SerializeField]
        private BeatmapTimelineRunner _timelineRunner;

        [Header("Events (In)")]

        [SerializeField]
        private BoolEvent _optionsMenuOpenEvent;

        [Header("Events (Out)")]

        [SerializeField]
        private IntEvent _beatPassedEvent;

        [SerializeField]
        private IntEvent _subdivPassedEvent;

        [SerializeField]
        private FloatEvent _normalizedProgressEvent;

        public TickInfo CurrentTickInfo { get; private set; }

        public event Action<TickInfo> OnTick;

        private BeatmapConfigSo _currentBeatmap;

        /// <summary>
        ///     Global dsp time of previous tick
        /// </summary>
        private double _previousEventTime;

        /// <summary>
        ///     Raw event time of when the beatmap starts
        /// </summary>
        private double _beatmapStartTime;

        private double _timeIntervalPerBeat;
        private double _timeIntervalPerSubdiv;

        private int _sampleRate;
        private ChannelGroup _masterChannel;

        private EventInstance _beatmapSoundtrackInstance;

        private BeatmapState _beatmapState = BeatmapState.Unloaded;

        public void Initialize()
        {
            DOTween.KillAll();
            _optionsMenuOpenEvent.AddListener(HandleOptionsMenuOpen);
        }

        public void Cleanup()
        {
            _optionsMenuOpenEvent.RemoveListener(HandleOptionsMenuOpen);

            if (_beatmapSoundtrackInstance.isValid())
            {
                _beatmapSoundtrackInstance.stop(STOP_MODE.ALLOWFADEOUT);
            }

            _currentBeatmap = null;
        }

        public void BeginRunningBeatmap(BeatmapConfigSo beatmap, out EventInstance soundtrackInstance)
        {
            if (_currentBeatmap != null)
            {
                Debug.LogWarning("Already running a beatmap, cannot run two on the same BeatmapRunner");
                soundtrackInstance = default;
                return;
            }

            GetDspInfo();

            _currentBeatmap = beatmap;

            // Testing util to start from the editor timeline playhead
            double startTime = BeatmappingEditingSettings.StartFromTimelinePlayhead
                ? BeatmappingEditingSettings.TimelinePlayheadTime
                : 0;

            StartBeatmapTrack(beatmap, startTime, out soundtrackInstance);

            _beatmapState = BeatmapState.WaitingForSoundtrackEvent;

            _timelineRunner.StartRunningBeatmap(beatmap);
        }

        /// <summary>
        ///     Tick the beatmap forward
        /// </summary>
        /// <returns>True if the beatmap is still running, false if it has ended</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public bool TickForward()
        {
            if (_currentBeatmap == null)
            {
                Debug.LogError("No beatmap loaded, cannot tick forward");
                return false;
            }

            var running = true;

            switch (_beatmapState)
            {
                case BeatmapState.Unloaded:
                    break;
                case BeatmapState.WaitingForSoundtrackEvent:
                    WaitForSoundtrackEventSync();
                    break;
                case BeatmapState.Playing:
                    running = TickBeatmapPlaying();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _timelineRunner.TickTimelineRunner(CurrentTickInfo);
            return running;
        }

        private void HandleOptionsMenuOpen(bool optionsMenuOpen)
        {
            if (!_beatmapSoundtrackInstance.isValid())
            {
                return;
            }

            if (optionsMenuOpen)
            {
                _beatmapSoundtrackInstance.setPaused(true);
            }
            else
            {
                _beatmapSoundtrackInstance.setPaused(false);
            }
        }

        private bool TickBeatmapPlaying()
        {
            // If the soundtrack instance is no longer valid, the level has ended
            if (!_beatmapSoundtrackInstance.isValid())
            {
                _beatmapState = BeatmapState.Unloaded;
                return false;
            }

            TickPlaying();
            return true;
        }

        private void TickPlaying()
        {
            // Update dps time
            _beatmapSoundtrackInstance.getTimelinePosition(out int eventTime);
            double currentEventTime = eventTime / 1000.0;
            double remainingDeltaTime = currentEventTime - _previousEventTime;

            if (remainingDeltaTime <= 0)
            {
                DoTick(currentEventTime);
                return;
            }

            // Tick the beatmap until we've caught up to the current event time
            // Reduces skipping artifacts on lag spikes
            while (remainingDeltaTime > 0)
            {
                double newRemainingDeltaTime = Math.Max(0, remainingDeltaTime - MinDeltaTime);
                double iterationDeltaTime = remainingDeltaTime - newRemainingDeltaTime;
                double tickIterationEventTime = _previousEventTime + iterationDeltaTime;

                DoTick(tickIterationEventTime);

                remainingDeltaTime = newRemainingDeltaTime;
            }
        }

        private void DoTick(double currentEventTime)
        {
            // Calculate time since start of beatmap
            double currentBeatmapTime = currentEventTime - _beatmapStartTime;
            double previousBeatmapTime = _previousEventTime - _beatmapStartTime;

            var currentBeatIndex = (int)Math.Floor(currentBeatmapTime / _timeIntervalPerBeat);
            var previousBeatIndex = (int)Math.Floor(previousBeatmapTime / _timeIntervalPerBeat);

            bool crossedBeatThisTick = currentBeatIndex > previousBeatIndex;

            var currentSubdivIndex = (int)Math.Floor(currentBeatmapTime / _timeIntervalPerSubdiv);
            var previousSubdivIndex = (int)Math.Floor(previousBeatmapTime / _timeIntervalPerSubdiv);

            bool crossedSubdivThisTick = currentSubdivIndex > previousSubdivIndex;

            if (crossedBeatThisTick)
            {
                _beatPassedEvent.Raise(currentBeatIndex);
            }

            if (crossedSubdivThisTick)
            {
                _subdivPassedEvent.Raise(currentSubdivIndex);
            }

            // Invoke tick event
            CurrentTickInfo = new TickInfo
            {
                BeatmapTime = currentBeatmapTime,
                CurrentLevelTime = currentBeatmapTime + _currentBeatmap.StartTime,
                SubdivIndex = currentSubdivIndex,
                CrossedSubdivThisTick = crossedSubdivThisTick,
                CurrentBeatmap = _currentBeatmap
            };

            OnTick?.Invoke(CurrentTickInfo);

            _previousEventTime = currentEventTime;

            // progress
            _normalizedProgressEvent.Raise((float)(currentBeatmapTime / _currentBeatmap.BeatmapDuration));
        }

        /// <summary>
        ///     After loading a beatmap, wait for the soundtrack event to begin playing before starting the timing.
        /// </summary>
        private void WaitForSoundtrackEventSync()
        {
            if (!_beatmapSoundtrackInstance.isValid())
            {
                return;
            }

            _beatmapSoundtrackInstance.getTimelinePosition(out int currentEventTime);
            double eventTimeSeconds = currentEventTime / 1000.0;

            if (currentEventTime > 0)
            {
                // Event time starts at 0, so we don't need to adjust from the config start time
                _beatmapStartTime = _currentBeatmap.StartTime;
                _previousEventTime = eventTimeSeconds;

                _timeIntervalPerBeat = 60 / _currentBeatmap.Bpm;
                _timeIntervalPerSubdiv = _timeIntervalPerBeat / _currentBeatmap.Subdivisions;

                _beatmapState = BeatmapState.Playing;
            }
        }

        private void StartBeatmapTrack(BeatmapConfigSo beatmap, double startTime, out EventInstance soundtrackInstance)
        {
            EventInstance soundtrack = RuntimeManager.CreateInstance(beatmap.BeatmapSoundtrackEvent);

            soundtrack.setTimelinePosition((int)(startTime * 1000));

            soundtrack.start();

            // release the instance when it ends
            soundtrack.release();

            _beatmapSoundtrackInstance = soundtrack;

            soundtrackInstance = soundtrack;
        }

        private double GetCurrentDspTime()
        {
            _masterChannel.getDSPClock(out ulong dspClock, out ulong parentClock);
            return (double)dspClock / _sampleRate;
        }

        private void GetDspInfo()
        {
            FMOD.System coreSystem = RuntimeManager.CoreSystem;
            coreSystem.getMasterChannelGroup(out ChannelGroup masterChannelGroup);

            RESULT getFormat =
                coreSystem.getSoftwareFormat(out int sampleRate, out SPEAKERMODE speakerMode, out int numrawspeakers);

            coreSystem.getDSPBufferSize(out uint bufferLength, out int numBuffers);

            if (getFormat != RESULT.OK)
            {
                Debug.LogError("Failed to get software format");
            }

            _sampleRate = sampleRate;
            _masterChannel = masterChannelGroup;
        }
    }
}