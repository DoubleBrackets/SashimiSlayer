using UnityEngine;

namespace Menus.PauseMenu
{
    public abstract class PauseMenuView : MonoBehaviour
    {
        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        ///     Awake replacement since view gameobjects can be set inactive
        /// </summary>
        public virtual void ViewAwake()
        {
        }

        /// <summary>
        ///     Start replacement since view gameobjects can be set inactive
        /// </summary>
        public virtual void ViewStart()
        {
        }

        /// <summary>
        ///     Destroy replacement since view gameobjects can be set inactive
        /// </summary>
        public virtual void ViewDestroy()
        {
        }
    }
}