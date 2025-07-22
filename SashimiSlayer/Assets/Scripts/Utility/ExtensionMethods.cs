using UnityEngine;

namespace Utility
{
    public static class ExtensionMethods
    {
        public static bool IsIndexInFlag(this int enumAsInt, int index)
        {
            return (enumAsInt & (1 << index)) != 0;
        }

        public static void SetEnabled(this CanvasGroup canvasGroup, bool state)
        {
            canvasGroup.alpha = state ? 1 : 0;
            canvasGroup.interactable = state;
            canvasGroup.blocksRaycasts = state;
        }

        public static void SetAlpha(this SpriteRenderer spriteRenderer, float alpha)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
    }
}