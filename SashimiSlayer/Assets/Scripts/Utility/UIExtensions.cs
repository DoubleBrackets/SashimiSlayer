using UnityEngine.UI;

namespace Utility
{
    public static class UIExtensions
    {
        public static float GetNormalizedValue(this Slider slider)
        {
            return slider.value / slider.maxValue;
        }

        public static void SetNormalizedValue(this Slider slider, float normalizedValue)
        {
            slider.value = normalizedValue * slider.maxValue;
        }
    }
}