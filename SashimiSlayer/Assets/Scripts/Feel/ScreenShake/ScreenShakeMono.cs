using Framework.Services;
using UnityEngine;

namespace Feel
{
    public class ScreenShakeMono : MonoBehaviour
    {
        public void ShakeScreen(ScreenShakeSO screenShakeSO)
        {
            ServiceLocator.GetService<ScreenShakeService>().ShakeScreen(screenShakeSO);
        }
    }
}