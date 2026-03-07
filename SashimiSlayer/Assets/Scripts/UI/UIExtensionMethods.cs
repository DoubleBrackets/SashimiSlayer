using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public static class UIExtensionMethods
    {
        /// <summary>
        ///     Returns the position to bring a child into view, snapped to the scroll rect's viewport.
        ///     Taken from
        ///     https://stackoverflow.com/questions/30766020/how-to-scroll-to-a-specific-element-in-scrollrect-with-unity-ui
        /// </summary>
        /// <param name="scroller"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static Vector2 GetSnapToPositionToBringChildIntoView(
            this ScrollRect scroller,
            RectTransform child,
            bool center = false)

        {
            var contentPos = (Vector2)scroller.transform.InverseTransformPoint(scroller.content.position);
            var childPos = (Vector2)scroller.transform.InverseTransformPoint(child.position);
            Vector2 endPos = contentPos - childPos;

            if (center)
            {
                endPos.y -= scroller.viewport.rect.height / 2;
                endPos.x -= scroller.viewport.rect.width / 2;
            }

            // Clamp
            endPos.y = Mathf.Clamp(endPos.y, 0, scroller.content.sizeDelta.y - scroller.viewport.rect.height);
            endPos.x = Mathf.Clamp(endPos.x, 0, scroller.content.sizeDelta.x - scroller.viewport.rect.width);

            // If no horizontal scroll, then don't change contentPos.x
            if (!scroller.horizontal)
            {
                endPos.x = scroller.content.anchoredPosition.x;
            }

            // If no vertical scroll, then don't change contentPos.y
            if (!scroller.vertical)
            {
                endPos.y = scroller.content.anchoredPosition.y;
            }

            return endPos;
        }
    }
}