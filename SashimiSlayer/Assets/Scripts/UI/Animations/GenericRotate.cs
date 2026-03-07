using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI.Animations
{
    public class GenericRotate : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField]
        private float _expandDuration;

        [SerializeField]
        private Vector3 _rotateTo;

        [SerializeField]
        private Vector3 _defaultRotate;

        [SerializeField]
        private bool _rotateOnSelected;

        private void OnEnable()
        {
            transform.localRotation = Quaternion.Euler(_defaultRotate);
        }

        public void Rotate()
        {
            transform.DOLocalRotate(_rotateTo, _expandDuration);
        }

        public void Unrotate()
        {
            transform.DOLocalRotate(_defaultRotate, _expandDuration);
        }

        public void OnSelect(BaseEventData eventData)
        {
            if (_rotateOnSelected)
            {
                Rotate();
            }
        }

        public void OnDeselect(BaseEventData eventData)
        {
            if (_rotateOnSelected)
            {
                Unrotate();
            }
        }
    }
}