using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace UI.LayoutNavigation
{
    /// <summary>
    ///     Manages UI navigation between navigation elements in the group.
    ///     Sets up navigation explicitly for purely horizontal or vertical layouts.
    ///     Ordered by position in hierarchy, takes any element that is in child hierarchy.
    ///     Dirt simple, and does not handle any layout changes (e.g ui added or removed)
    /// </summary>
    public class LayoutNavGroup : MonoBehaviour
    {
        private enum Direction
        {
            /// <summary>
            ///     Top to bottom
            /// </summary>
            Vertical,

            /// <summary>
            ///     Left to right
            /// </summary>
            Horizontal
        }

        [Header("Config")]

        [SerializeField]
        private Direction _direction;

        [SerializeField]
        private bool _wrap;

        private List<LayoutNavElement> _layoutNavElements = new();

        private void Start()
        {
            Initialize();
        }

        public void Initialize()
        {
            FindChildLayoutNavElements();
            SetupNavigationBetweenElements();
        }

        private void FindChildLayoutNavElements()
        {
            _layoutNavElements = GetComponentsInChildren<LayoutNavElement>().ToList();
        }

        private void SetupNavigationBetweenElements()
        {
            for (var i = 0; i < _layoutNavElements.Count; i++)
            {
                LayoutNavElement previous = i > 0 ? _layoutNavElements[i - 1] : null;
                LayoutNavElement next = i < _layoutNavElements.Count - 1 ? _layoutNavElements[i + 1] : null;
                LayoutNavElement current = _layoutNavElements[i];

                var navigation = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    wrapAround = _wrap
                };

                if (_direction == Direction.Horizontal)
                {
                    navigation.selectOnLeft = previous?.Selectable;
                    navigation.selectOnRight = next?.Selectable;
                }

                if (_direction == Direction.Vertical)
                {
                    navigation.selectOnUp = previous?.Selectable;
                    navigation.selectOnDown = next?.Selectable;
                }

                // Edge cases for wrapping
                if (i == 0 && _wrap)
                {
                    if (_direction == Direction.Horizontal)
                    {
                        navigation.selectOnLeft = _layoutNavElements[^1].Selectable;
                    }
                    else if (_direction == Direction.Vertical)
                    {
                        navigation.selectOnUp = _layoutNavElements[^1].Selectable;
                    }
                }
                else if (i == _layoutNavElements.Count - 1 && _wrap)
                {
                    if (_direction == Direction.Horizontal)
                    {
                        navigation.selectOnRight = _layoutNavElements[0].Selectable;
                    }
                    else if (_direction == Direction.Vertical)
                    {
                        navigation.selectOnDown = _layoutNavElements[0].Selectable;
                    }
                }

                current.SetNavigation(navigation);
            }
        }
    }
}