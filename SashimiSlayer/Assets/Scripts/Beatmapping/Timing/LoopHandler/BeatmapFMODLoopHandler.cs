using System;
using System.Runtime.InteropServices;
using AOT;
using FMOD;
using FMOD.Studio;
using Framework.Audio;
using Debug = UnityEngine.Debug;

namespace Beatmapping.Timing.LoopHandler
{
    /**
     * Handles FMOD looping logic, currently used for "success checks" in the tutorial.
     * Looping logic is handled by FMOD loop regions and parameter conditions, which are hand configured.
     * On the Unity side, only the streak parameter needs to be set.
     * 
     * Handles logic for "skipping" the next loop, which is used for exhibitioning and debugging.
     * 
     * Also handles FMOD markers that act as "guards" against loop leaking.
     * Loop leaking is when loops trigger on the frame after their actual time,
     * causing notes to spawn that occur within 1 frame after the loop.
     * 
     * The markers are placed after the final note interaction inside the loop region, but before the end of the region
     * 
     * Guarding is done by checking when the loop guard marker is passed, then
     * - Disabling note spawning if the current streak is less than the required streak for the loop region
     * - Enabling note spawning if the current streak is greater than or equal to the required streak for the loop region (i.e will pass and not loop)
     * 
     * After disabling, re-enable after the loop occurs, which we check by comparing the current beatmap time
     * to the beatmap time when the guard marker was passed
     */
    public class BeatmapFMODLoopHandler : ILoopHandler
    {
        private static BeatmapFMODLoopHandler _instance;

        public bool IsSkippingLoops => _skippingLoops;

        /// <summary>
        ///     Current success streak
        /// </summary>
        private int _currentSuccessfulStreak;

        /// <summary>
        ///     For exhibitioning; if toggled true, tutorial will not loop (streak is set to max)
        /// </summary>
        private bool _skippingLoops;

        private int _successfulStreakRequiredToUnlockLoopRegion;
        private double _guardMarkerBeatmapTime;
        private string _successStreakParam;
        private double _currentBeatmapTime;

        /// <summary>
        ///     True success streak, since skipping artificially sets streak to max
        /// </summary>
        private int _trueSuccessfulStreak;

        private EVENT_CALLBACK _callback;

        public event Action<bool> OnPauseNoteSpawning;

        public BeatmapFMODLoopHandler(string successStreakParam)
        {
            _instance = this;
            _successStreakParam = successStreakParam;
        }

        public void SetupNewBeatmap(EventInstance instance)
        {
            _callback = OnEventCallback;
            instance.setCallback(_callback, EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
            StopSkippingLoops();
            SetSuccessStreak(0);
        }

        public void Tick(BeatmapRunner.TickInfo tickInfo)
        {
            // If we looped back to before the guard marker, wipe the marker and unpause
            if (tickInfo.BeatmapTime < _guardMarkerBeatmapTime)
            {
                _successfulStreakRequiredToUnlockLoopRegion = 0;
                SetNoteSpawningPaused(false);
            }

            _currentBeatmapTime = tickInfo.BeatmapTime;
        }

        public void SetSuccessStreak(int streak)
        {
            _trueSuccessfulStreak = streak;

            if (_skippingLoops)
            {
                return;
            }

            _currentSuccessfulStreak = streak;
            SetStreakParam(streak);
        }

        public void ToggleSkipLoops()
        {
            if (!_skippingLoops)
            {
                _skippingLoops = true;
                _currentSuccessfulStreak = 99;
                SetStreakParam(99);
            }
            else
            {
                StopSkippingLoops();
            }
        }

        private void StopSkippingLoops()
        {
            _skippingLoops = false;
            _currentSuccessfulStreak = _trueSuccessfulStreak;
            SetStreakParam(_currentSuccessfulStreak);
        }

        /// <summary>
        ///     Handle guard marker callback, i.e when a loop guard marker is passed.
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
                int requiredStreak = int.Parse(markerName.Split(' ')[0]);
                _instance._successfulStreakRequiredToUnlockLoopRegion = requiredStreak;
                _instance._guardMarkerBeatmapTime = _instance._currentBeatmapTime;
                _instance.EnableSpawningOnLoopPassed();
            }
            catch (Exception)
            {
                return RESULT.OK;
            }

            return RESULT.OK;
        }

        private void EnableSpawningOnLoopPassed()
        {
            bool spawningEnabled = _currentSuccessfulStreak >= _successfulStreakRequiredToUnlockLoopRegion;
            SetNoteSpawningPaused(!spawningEnabled);
        }

        private void SetStreakParam(int val)
        {
            FmodParamManager.SetParamByName(_successStreakParam, val);
        }

        private void SetNoteSpawningPaused(bool paused)
        {
            OnPauseNoteSpawning?.Invoke(!paused);
        }
    }
}