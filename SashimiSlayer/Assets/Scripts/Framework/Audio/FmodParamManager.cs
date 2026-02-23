using System;
using System.Collections.Generic;
using Beatmapping;
using Beatmapping.Events;
using Beatmapping.NoteInteraction.DataTypes;
using EditorUtils.BoldHeader;
using FMOD;
using FMODUnity;
using NaughtyAttributes;
using Protag.Presentation.Events;
using Protag.Presentation.Slicing;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Core.Audio
{
    [Serializable]
    public struct PitchShiftValues
    {
        [Range(0.5f, 2f)]
        public float SlicePitchShift;

        [Range(0.5f, 2f)]
        public float StarBlockPitchShift;

        [Range(0.5f, 2f)]
        public float ShellBlockPitchShift;
    }

    /// <summary>
    ///     Script that handles setting global FMOD params
    /// </summary>
    public class FmodParamManager : MonoBehaviour
    {
        [BoldHeader("FMOD Param Manager")]
        [InfoBox("Handles setting various global FMOD params (e.g slice timing, streak)")]
        [Header("Events (In)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        [SerializeField]
        private ObjectsSlicedEvent _objectsSlicedEvent;

        [SerializeField]
        private BeatmapEvent _beatmapLoadedEvent;

        [Header("Param Refs")]

        [SerializeField]
        [ParamRef]
        private string _slicePitchShiftParam;

        [SerializeField]
        [ParamRef]
        private string _starBlockPitchShiftParam;

        [SerializeField]
        [ParamRef]
        private string _shellBlockPitchShiftParam;

        [SerializeField]
        [ParamRef]
        private string _interactionTimingGlobalParam;

        [SerializeField]
        [ParamRef]
        private string _successStreakParam;

        [SerializeField]
        [ParamRef]
        private string _sliceTargetCountParam;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent<int> _onFmodStreakParamChanged;

        private int _successStreak;

        /// <summary>
        ///     For exhibitioning; if toggled true, tutorial will not loop (streak is set to max)
        /// </summary>
        private bool _shouldSkipLoops;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
            _objectsSlicedEvent.AddListener(OnObjectsSliced);
            _beatmapLoadedEvent.AddListener(OnBeatmapLoaded);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
            _objectsSlicedEvent.RemoveListener(OnObjectsSliced);
            _beatmapLoadedEvent.RemoveListener(OnBeatmapLoaded);
        }

        private void OnGUI()
        {
            if (_shouldSkipLoops)
            {
                GUILayout.TextField("~");
            }
        }

        private void OnBeatmapLoaded(BeatmapConfigSo beatmap)
        {
            PitchShiftValues pitchShiftValues = beatmap.PitchShiftValues;

            SetParamByName(_slicePitchShiftParam, pitchShiftValues.SlicePitchShift);
            SetParamByName(_starBlockPitchShiftParam, pitchShiftValues.StarBlockPitchShift);
            SetParamByName(_shellBlockPitchShiftParam, pitchShiftValues.ShellBlockPitchShift);

            EndSkipLoops();
        }

        private void OnNoteInteractionFinalResult(NoteInteractionFinalResult result)
        {
            if (result.Successful)
            {
                _successStreak++;

                var timingParamVal = 0;
                switch (result.TimingResult.Score)
                {
                    case TimingWindow.Score.Pass:
                        if (result.TimingResult.Direction == TimingWindow.Direction.Early)
                        {
                            timingParamVal = 0;
                        }
                        else if (result.TimingResult.Direction == TimingWindow.Direction.Late)
                        {
                            timingParamVal = 2;
                        }

                        break;
                    case TimingWindow.Score.Perfect:
                        timingParamVal = 1;
                        break;
                }

                SetParamByName(_interactionTimingGlobalParam, timingParamVal);
            }
            else
            {
                _successStreak = 0;
            }

            // If auto clear is toggled, leave fmod param at max to avoid looping
            if (!_shouldSkipLoops)
            {
                SetStreakParam(_successStreak);
            }
        }

        private void OnObjectsSliced(ProtagSlicedObjectList slicedObjects)
        {
            List<ProtagSlicedObject> objects = slicedObjects.SlicedObjects;

            if (objects.Count == 0)
            {
                return;
            }

            // Set slice target count
            SetParamByName(_sliceTargetCountParam, objects.Count);

            // Set interaction timing to "Perfect"
            // For UI elements this is intended, and for notes, this parameter will be overriden by the note interaction result
            SetParamByName(_interactionTimingGlobalParam, 1);
        }

        private void SetParamByName(string param, float value)
        {
            FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
            RESULT result = studioSystem.setParameterByName(param, value, true);
            if (result != RESULT.OK)
            {
                Debug.Log($"Failed to set FMOD parameter '{param}' to {value}: {result}");
            }
        }

        public void BeginSkipLoops()
        {
            _shouldSkipLoops = true;

            if (_shouldSkipLoops)
            {
                SetStreakParam(99);
            }
        }

        public void EndSkipLoops()
        {
            _shouldSkipLoops = false;
        }

        private void SetStreakParam(int val)
        {
            SetParamByName(_successStreakParam, val);
            _onFmodStreakParamChanged?.Invoke(val);
        }
    }
}