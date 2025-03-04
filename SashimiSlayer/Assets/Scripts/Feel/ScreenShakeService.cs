using Cinemachine;
using UnityEngine;

namespace Feel
{
    /// <summary>
    ///     Service for shaking the screen.
    /// </summary>
    public class ScreenShakeService : MonoBehaviour
    {
        [SerializeField]
        private CinemachineImpulseSource _impulseSource;

        public static ScreenShakeService Instance { get; private set; }

        public float ForceScale { get; set; } = 1;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void ShakeScreen(float duration, Vector3 velocity, CinemachineImpulseDefinition.ImpulseShapes shapes)
        {
            _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;
            _impulseSource.m_ImpulseDefinition.m_ImpulseShape = shapes;
            _impulseSource.GenerateImpulseWithVelocity(velocity * ForceScale);
        }

        public void ShakeScreen(ScreenShakeSO screenShakeSO)
        {
            ShakeScreen(screenShakeSO.Duration, screenShakeSO.ScaledVelocity, screenShakeSO.Shapes);
        }
    }
}