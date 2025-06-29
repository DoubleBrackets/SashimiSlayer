using System;
using Beatmapping.Timing;
using Core.Audio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Timeline;

namespace Beatmapping
{
    [CreateAssetMenu(fileName = "BeatmapConfig", menuName = "BeatmapConfigSO")]
    public class BeatmapConfigSo : ScriptableObject
    {
        [field: Header("Song Timing")]

        [field: SerializeField]
        [field: TextArea]
        public string BeatmapID { get; private set; }

        [field: SerializeField]
        public double Bpm { get; private set; }

        /// <summary>
        ///     Number of subdivisions per beat.
        /// </summary>
        [field: SerializeField]
        public int Subdivisions { get; private set; }

        [field: SerializeField]
        public double StartTime { get; private set; }

        [field: SerializeField]
        public int BeatsPerMeasure { get; private set; }

        [field: Header("Beatmap Data")]

        [field: SerializeField]
        public TimelineAsset BeatmapTimeline { get; private set; }

        [field: SerializeField]
        public EventReference BeatmapSoundtrackEvent { get; private set; }

        [field: SerializeField]
        public PitchShiftValues PitchShiftValues { get; private set; }

        [field: Header("Gameplay Data")]

        [field: SerializeField]
        public int PlayerMaxHealth { get; private set; }

        [field: SerializeField]
        public TimingWindowSO TimingWindowSO { get; private set; }

        [field: SerializeField]
        public GameObject ResultsScreenCustomPrefab { get; private set; }

        /// <summary>
        ///     Duration of the beatmap level, in seconds, from the start time
        /// </summary>
        public double BeatmapDuration => BeatmapTimeline.duration - StartTime;

        /// <summary>
        ///     Take a time and snap it to the nearest subdivision
        /// </summary>
        /// <param name="rawTime"></param>
        /// <returns></returns>
        public double QuantizeTime(double rawTime)
        {
            double startTime = StartTime;
            double bpm = Bpm;

            int subdivisions = Subdivisions;
            subdivisions = subdivisions == 0 ? 1 : subdivisions;

            double subdivDuration = 60 / bpm / subdivisions;

            double beatTime = (rawTime - startTime) / subdivDuration;

            double quantizedBeatTime = Math.Round(beatTime);

            rawTime = startTime + quantizedBeatTime * subdivDuration;

            return rawTime;
        }
    }
}