using System;
using Beatmapping.Tooling;
using Core.Scene;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Events;
using Events.Core;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using Debug = UnityEngine.Debug;
using STOP_MODE = FMOD.Studio.STOP_MODE;

namespace Beatmapping.Timing
{
    public class BeatmapTimeManager : MonoBehaviour
    {
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
        private GameLevelSO _levelResultLevel;

        [Header("Events (In)")]

        [SerializeField]
        private BeatmapEvent _beatmapLoadedEvent;

        [SerializeField]
        private BeatmapEvent _beatmapUnloadedEvent;

        [SerializeField]
        private BoolEvent _optionsMenuOpenEvent;

        [Header("Events (Out)")]

        [SerializeField]
        private IntEvent _beatPassedEvent;

        [SerializeField]
        private IntEvent _subdivPassedEvent;

        [SerializeField]
        private FloatEvent _normalizedProgressEvent;

        public static BeatmapTimeManager Instance { get; private set; }

        public TickInfo CurrentTickInfo { get; private set; }

        public event Action<EventInstance> OnBeatmapSoundtrackInstanceCreated;

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

        private void Awake()
        {
            DOTween.KillAll();
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _beatmapLoadedEvent.AddListener(HandleStartBeatmap);
            _beatmapUnloadedEvent.AddListener(HandleBeatmapUnloaded);
            _optionsMenuOpenEvent.AddListener(HandleOptionsMenuOpen);
        }

        private void Update()
        {
            if (_currentBeatmap == null)
            {
                return;
            }

            switch (_beatmapState)
            {
                case BeatmapState.Unloaded:
                    break;
                case BeatmapState.WaitingForSoundtrackEvent:
                    WaitForSoundtrackEventSync();
                    break;
                case BeatmapState.Playing:
                    TickBeatmapPlaying();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void OnDestroy()
        {
            _beatmapLoadedEvent.RemoveListener(HandleStartBeatmap);
            _beatmapUnloadedEvent.RemoveListener(HandleBeatmapUnloaded);
            _optionsMenuOpenEvent.RemoveListener(HandleOptionsMenuOpen);
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

        private void EndBeatmap()
        {
            LevelLoader.Instance.LoadLevel(_levelResultLevel).Forget();
            _beatmapState = BeatmapState.Unloaded;
        }

        private void TickBeatmapPlaying()
        {
            // If the soundtrack instsance is no longer valid, the level has ended
            if (!_beatmapSoundtrackInstance.isValid())
            {
                EndBeatmap();
            }
            else
            {
                TickPlaying();
            }
        }

        private void TickPlaying()
        {
            // Update dps time
            _beatmapSoundtrackInstance.getTimelinePosition(out int eventTime);
            double currentEventTime = eventTime / 1000.0;
            double eventDeltaTime = currentEventTime - _previousEventTime;

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

        public int GetClosestSubdivOfTime(double beatmapTime)
        {
            return (int)Math.Round(beatmapTime / _timeIntervalPerSubdiv);
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

        private void HandleStartBeatmap(BeatmapConfigSo beatmap)
        {
            GetDspInfo();

            _currentBeatmap = beatmap;

            // Testing util to start from the editor timeline playhead
            double startTime = BeatmappingUtilities.StartFromTimelinePlayhead
                ? BeatmappingUtilities.TimelinePlayheadTime
                : 0;

            StartBeatmapTrack(beatmap, startTime);

            _beatmapState = BeatmapState.WaitingForSoundtrackEvent;
        }

        private void HandleBeatmapUnloaded(BeatmapConfigSo beatmap)
        {
            if (_beatmapSoundtrackInstance.isValid())
            {
                _beatmapSoundtrackInstance.stop(STOP_MODE.ALLOWFADEOUT);
            }

            _currentBeatmap = null;
        }

        private void StartBeatmapTrack(BeatmapConfigSo beatmap, double startTime)
        {
            EventInstance soundtrack = RuntimeManager.CreateInstance(beatmap.BeatmapSoundtrackEvent);

            soundtrack.setTimelinePosition((int)(startTime * 1000));

            soundtrack.start();

            // release the instance when it ends
            soundtrack.release();

            _beatmapSoundtrackInstance = soundtrack;

            OnBeatmapSoundtrackInstanceCreated?.Invoke(soundtrack);
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