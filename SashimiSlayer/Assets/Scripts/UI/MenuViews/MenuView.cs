using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.MenuViews
{
    public class MenuView : MonoBehaviour
    {
        [field: SerializeField]
        public bool ViewEnabled { get; set; } = true;

        /// <summary>
        ///     The gameobject to select with EventSystem when this view is shown
        /// </summary>
        [SerializeField]
        private GameObject _onShowSelect;

        [SerializeField]
        private CanvasGroup _canvasGroup;

        [TextArea(3, 10)]
        [SerializeField]
        private string _viewName;

        public string ViewName => _viewName;

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
        public virtual void ViewInitialized()
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
        public void GrabFocus()
        {
            if (EventSystem.current == null)
            {
                return;
            }

            EventSystem.current.SetSelectedGameObject(_onShowSelect);
        }
    }
}