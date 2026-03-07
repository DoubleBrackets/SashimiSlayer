using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace UI.LayoutNavigation
{
    /// <summary>
    ///     UI element that is part of a <see cref="LayoutNavGroup" /> and can navigate between each
    ///     other.
    /// </summary>
    public class LayoutNavElement : MonoBehaviour
    {
        /// <summary>
        ///     The physical selectable that this element represents
        /// </summary>
        [field: InfoBox("UI element that is part of a LayoutNavGroup explicit navigation")]
        [field: SerializeField]
        public Selectable Selectable { get; private set; }

        public void SetNavigation(Navigation nav)
        {
            Selectable.navigation = nav;
        }

        private void OnValidate()
        {
            if (Selectable == null)
            {
                Selectable = GetComponent<Selectable>();
            }
        }
    }
}