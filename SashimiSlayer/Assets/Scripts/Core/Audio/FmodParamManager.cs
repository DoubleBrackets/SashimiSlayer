using System;
using Beatmapping;
using Beatmapping.Interactions;
using EditorUtils.BoldHeader;
using Events.Core;
using FMOD;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
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
        private SliceResultEvent _sliceResultEvent;

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

        private int _successfulStreak;

        private void Awake()
        {
            _noteInteractionFinalResultEvent.AddListener(OnNoteInteractionFinalResult);
            _sliceResultEvent.AddListener(OnSliceResult);
            _beatmapLoadedEvent.AddListener(OnBeatmapLoaded);
        }

        private void OnDestroy()
        {
            _noteInteractionFinalResultEvent.RemoveListener(OnNoteInteractionFinalResult);
            _sliceResultEvent.RemoveListener(OnSliceResult);
            _beatmapLoadedEvent.RemoveListener(OnBeatmapLoaded);
        }

        private void OnBeatmapLoaded(BeatmapConfigSo beatmap)
        {
            PitchShiftValues pitchShiftValues = beatmap.PitchShiftValues;

            SetParamByName(_slicePitchShiftParam, pitchShiftValues.SlicePitchShift);
            SetParamByName(_starBlockPitchShiftParam, pitchShiftValues.StarBlockPitchShift);
            SetParamByName(_shellBlockPitchShiftParam, pitchShiftValues.ShellBlockPitchShift);
        }

        private void OnNoteInteractionFinalResult(NoteInteraction.FinalResult result)
        {
            if (result.Successful)
            {
                _successfulStreak++;

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
                _successfulStreak = 0;
            }

            SetParamByName(_successStreakParam, _successfulStreak);
        }

        private void OnSliceResult(SliceResultData data)
        {
            if (data.SliceCount > 0)
            {
                SetParamByName(_sliceTargetCountParam, data.SliceCount);
            }
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
    }
}