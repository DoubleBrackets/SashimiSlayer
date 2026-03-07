using UnityEngine;
using UnityEngine.Events;

namespace UI.Screens.StartMenu
{
    /// <summary>
    ///     Sliceable fish in the Start Menu
    /// </summary>
    public class StartMenuFish : MonoBehaviour
    {
        [Header("Depends")]

        [SerializeField]
        private SpriteRenderer _fishSprite;

        [Header("Unity Events")]

        [SerializeField]
        private UnityEvent _onFishSlice;

        [SerializeField]
        private UnityEvent _onFishPlace;

        public void Place()
        {
            _fishSprite.enabled = true;
            _onFishPlace.Invoke();
        }

        public void Slice()
        {
            _fishSprite.enabled = false;
            _onFishSlice.Invoke();
            Destroy(gameObject, 2f);
        }
    }
}