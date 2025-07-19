using Core.Protag;
using Events.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Menus.StartMenu
{
    /// <summary>
    ///     Sliceable fish in the Start Menu
    /// </summary>
    public class StartMenuFish : MonoBehaviour
    {
        [Header("Depends")]

        [SerializeField]
        private SpriteRenderer _fishSprite;

        [Header("Events (Out)")]

        [SerializeField]
        private SliceResultEvent _sliceResultEvent;

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
            _sliceResultEvent.Raise(new SliceResultData(1, SliceResultData.SlicedObject.MenuItem));
            Destroy(gameObject, 2f);
        }
    }
}