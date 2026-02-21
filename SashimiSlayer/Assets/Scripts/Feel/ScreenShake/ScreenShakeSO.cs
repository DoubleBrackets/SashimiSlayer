using Cinemachine;
using UnityEngine;

namespace Feel
{
    [CreateAssetMenu(fileName = "ScreenShakeSO")]
    public class ScreenShakeSO : ScriptableObject
    {
        [field: SerializeField]
        public float Duration { get; private set; }

        [field: SerializeField]
        public Vector3 Velocity { get; private set; }

        [field: SerializeField]
        public float Magnitude { get; private set; }

        [field: SerializeField]
        public CinemachineImpulseDefinition.ImpulseShapes Shapes { get; private set; }

        public Vector3 ScaledVelocity => Velocity * Magnitude;
    }
}