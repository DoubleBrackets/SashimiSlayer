using System;
using UnityEngine;

namespace GameInput.Haptics
{
    [Serializable]
    public struct RumbleProfile
    {
        [field: SerializeField]
        public float Duration { get; set; }

        [field: SerializeField]
        public float Amplitude { get; set; }

        [field: SerializeField]
        public AnimationCurve Curve { get; set; }

        public readonly float Evaluate(float instanceStartTime)
        {
            return Curve.Evaluate(instanceStartTime / Duration) * Amplitude;
        }
    }

    [CreateAssetMenu(fileName = "RumbleFeedback", menuName = "RumbleFeedbackSO")]
    public class RumbleFeedbackSO : ScriptableObject
    {
        [field: SerializeField]
        public RumbleProfile RumbleProfileLowFreq { get; set; }

        [field: SerializeField]
        public RumbleProfile RumbleProfileHighFreq { get; set; }
    }
}