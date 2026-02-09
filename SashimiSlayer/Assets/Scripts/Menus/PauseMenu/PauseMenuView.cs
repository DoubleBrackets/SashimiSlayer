using UnityEngine;
using UnityEngine.EventSystems;

namespace Menus.PauseMenu
{
    public abstract class PauseMenuView : MonoBehaviour
    {
        /// <summary>
        ///     The gameobject to select with EventSystem when this view is shown
        /// </summary>
        [SerializeField]
        private GameObject _onShowSelect;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        public GameObject OnShowSelect => _onShowSelect;

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

        /// <summary>
        ///     Called when this view is ready to be navigated
        /// </summary>
        public void EnterSelection()
        {
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(_onShowSelect);
            }
        }

        public void SetInteractable(bool interactable)
        {
            _canvasGroup.interactable = interactable;
        }
    }
}