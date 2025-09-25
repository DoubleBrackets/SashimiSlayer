using System;
using System.Runtime.InteropServices;
using AOT;
using Events;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Beatmapping.Timing
{
    /// <summary>
    ///     Handles FMOD destination markers that act as "guards" against loop leaking.
    ///     Loop leaking is when loops trigger on the frame after their actual time, causing notes to spawn that occur within 1
    ///     frame after the loop
    /// </summary>
    public class LoopRegionGuardMarker : MonoBehaviour
    {
        private static LoopRegionGuardMarker _instance;

        [SerializeField]
        private BeatmapTimeManager _beatmapTimeManager;

        [Header("Event (Out)")]

        [SerializeField]
        private BoolEvent _setBeatNoteSpawningEnabledEvent;

        [SerializeField]
        private UnityEvent _onLoopPassed;

        private EVENT_CALLBACK _callback;

        private int _successfulStreakRequiredToUnlockLoopRegion;

        private double _guardMarkerBeatmapTime;

        private int _currentSuccessfulStreak;

        private void Awake()
        {
            _instance = this;
            _beatmapTimeManager.OnBeatmapSoundtrackInstanceCreated += OnBeatmapSoundtrackInstanceCreated;
            _beatmapTimeManager.OnTick += OnTick;
        }

        private void OnDestroy()
        {
            _beatmapTimeManager.OnBeatmapSoundtrackInstanceCreated -= OnBeatmapSoundtrackInstanceCreated;
            _beatmapTimeManager.OnTick -= OnTick;
        }

        private void OnTick(BeatmapTimeManager.TickInfo tickInfo)
        {
            // If we looped back to before the guard marker, wipe the marker and unlock the note spawning
            if (tickInfo.BeatmapTime < _guardMarkerBeatmapTime)
            {
                _successfulStreakRequiredToUnlockLoopRegion = 0;
                _setBeatNoteSpawningEnabledEvent.Raise(true);
            }
        }

        /// <summary>
        ///     Called by the FMOD Param manager
        /// </summary>
        /// <param name="fmodStreakParam"></param>
        public void OnFMODStreakParamSet(int fmodStreakParam)
        {
            _currentSuccessfulStreak = fmodStreakParam;
            // After each interaction unlock the note spawning
            // This shouldn't ever come into play if the loop guard markers are set up correctly in FMOD
            // So it's more of a "just in case" someone misplaces a marker...
            _setBeatNoteSpawningEnabledEvent.Raise(true);
        }

        private void EnableSpawningOnLoopPassed()
        {
            bool spawningEnabled = _currentSuccessfulStreak >= _successfulStreakRequiredToUnlockLoopRegion;
            _setBeatNoteSpawningEnabledEvent.Raise(spawningEnabled);
            if (spawningEnabled)
            {
                _onLoopPassed?.Invoke();
            }
        }

        private void OnBeatmapSoundtrackInstanceCreated(EventInstance instance)
        {
            _callback = OnEventCallback;
            instance.setCallback(_callback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
        }

        /// <summary>
        ///     Handle guard marker callback.
        ///     This MUST BE STATIC, or else will cause crashes.
        ///     https://qa.fmod.com/t/unity-integration-may-be-crashing-unity-editor-if-a-callback-references-unity-gameobject/15501
        /// </summary>
        /// <param name="type"></param>
        /// <param name="instancePtr"></param>
        /// <param name="parameterPtr"></param>
        /// <returns></returns>
        [MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
        private static RESULT OnEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
        {
            var instance = new EventInstance(instancePtr);

            IntPtr timelineInfoPtr;
            RESULT result = instance.getUserData(out timelineInfoPtr);
            if (result != RESULT.OK)
            {
                Debug.LogError("Timeline Callback error: " + result);
                return RESULT.OK;
            }

            var parameter =
                (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));

            // The name of the marker is the number of successes needed to pass the incoming loop region check
            // This number is used to alert the note manager that a loop check is incoming
            // This is a hack used to prevent spawning notes due to loop leaks
            string markerName = parameter.name;

            Debug.Log("Loop region guard marker passed: " + markerName);

            try
            {
                _instance._successfulStreakRequiredToUnlockLoopRegion = int.Parse(markerName.Split(' ')[0]);
                _instance._guardMarkerBeatmapTime = _instance._beatmapTimeManager.CurrentTickInfo.BeatmapTime;
                _instance.EnableSpawningOnLoopPassed();
            }
            catch (Exception)
            {
                return RESULT.OK;
            }

            return RESULT.OK;
        }
    }
}