using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.LayoutNavigation
{
    /// <summary>
    ///     Simple component that automatically focuses a scroll bar when selected
    /// </summary>
    [RequireComponent(typeof(Selectable))]
    public class ScrollbarFocus : MonoBehaviour, ISelectHandler
    {
        [SerializeField]
        private ScrollRect _scrollRect;

        private void OnValidate()
        {
            if (_scrollRect == null)
            {
                _scrollRect = GetComponentInParent<ScrollRect>();
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();

                _scrollRect.content.anchoredPosition =
                    _scrollRect.GetSnapToPositionToBringChildIntoView(GetComponent<RectTransform>(), true);
            }
        }
    }
}