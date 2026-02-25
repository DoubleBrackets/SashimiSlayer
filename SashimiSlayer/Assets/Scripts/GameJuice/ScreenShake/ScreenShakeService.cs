using Cinemachine;
using UnityEngine;
using ValueSO.Core;

namespace GameJuice.ScreenShake
{
    /// <summary>
    ///     Service for shaking the screen.
    /// </summary>
    public class ScreenShakeService : MonoBehaviour
    {
        [SerializeField]
        private CinemachineImpulseSource _impulseSource;

        [SerializeField]
        private FloatValueSO _screenShakeRatioValueSO;

        public void ShakeScreen(float duration, Vector3 velocity, CinemachineImpulseDefinition.ImpulseShapes shapes)
        {
            _impulseSource.m_ImpulseDefinition.m_TimeEnvelope.m_SustainTime = duration;
            _impulseSource.m_ImpulseDefinition.m_ImpulseShape = shapes;
            _impulseSource.GenerateImpulseWithVelocity(velocity * _screenShakeRatioValueSO.Value);
        }

        public void ShakeScreen(ScreenShakeSO screenShakeSO)
        {
            ShakeScreen(screenShakeSO.Duration, screenShakeSO.ScaledVelocity, screenShakeSO.Shapes);
        }
    }
}