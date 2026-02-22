using DG.Tweening;
using UnityEngine;

namespace UI.Animations
{
    public class GenericExpand : MonoBehaviour
    {
        [SerializeField]
        private float _expandDuration;

        [SerializeField]
        private float _expandScale;

        [SerializeField]
        private float _defaultScale;

        public void Expand()
        {
            transform.DOScale(_expandScale, _expandDuration);
        }

        public void Shrink()
        {
            transform.DOScale(_defaultScale, _expandDuration);
        }
    }
}