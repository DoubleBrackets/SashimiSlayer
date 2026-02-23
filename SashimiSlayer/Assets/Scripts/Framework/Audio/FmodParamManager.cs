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
using Debug = UnityEngine.Debug;

namespace Framework.Audio
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

        [SerializeField]
        private BeatmapEvent _beatmapUnloadedEvent;

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
        private string _sliceTargetCountParam;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
            _objectsSlicedEvent.AddListener(OnObjectsSliced);
            _beatmapLoadedEvent.AddListener(OnBeatmapLoaded);
            _beatmapUnloadedEvent.AddListener(OnBeatmapUnloaded);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
            _objectsSlicedEvent.RemoveListener(OnObjectsSliced);
            _beatmapLoadedEvent.RemoveListener(OnBeatmapLoaded);
            _beatmapUnloadedEvent.RemoveListener(OnBeatmapUnloaded);
        }

        private void OnBeatmapLoaded(BeatmapConfigSo beatmap)
        {
            PitchShiftValues pitchShiftValues = beatmap.PitchShiftValues;

            SetParamByName(_slicePitchShiftParam, pitchShiftValues.SlicePitchShift);
            SetParamByName(_starBlockPitchShiftParam, pitchShiftValues.StarBlockPitchShift);
            SetParamByName(_shellBlockPitchShiftParam, pitchShiftValues.ShellBlockPitchShift);
        }

        private void OnBeatmapUnloaded(BeatmapConfigSo beatmap)
        {
            // Set interaction timing to "Perfect" when in menus
            SetParamByName(_interactionTimingGlobalParam, 1);
        }

        private void OnNoteInteractionFinalResult(NoteInteractionFinalResult result)
        {
            if (result.Successful)
            {
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
        }

        public static void SetParamByName(string param, float value)
        {
            FMOD.Studio.System studioSystem = RuntimeManager.StudioSystem;
            RESULT result = studioSystem.setParameterByName(param, value, true);
            if (result != RESULT.OK)
            {
                Debug.Log($"Failed to set FMOD parameter '{param}' to {value}: {result}");
            }
        }
    }
}