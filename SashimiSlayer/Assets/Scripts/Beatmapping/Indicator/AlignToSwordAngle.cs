using Core.Protag;
using EditorUtils.BoldHeader;
using Events.Core;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Indicator
{
    public class AlignToSwordAngle : MonoBehaviour
    {
        [BoldHeader("Align To Sword Angle")]
        [InfoBox("Matches a target Transform rotation to the protag's sword angle")]
        [Header("Depends")]

        [SerializeField]
        private Transform _visualContainer;

        [Header("Config")]

        [SerializeField]
        private float _rotationOffset;

        [Header("Event (In)")]

        [SerializeField]
        private ProtagSwordStateEvent _protagSwordStateEvent;

        private void Awake()
        {
            _protagSwordStateEvent.AddListener(OnProtagSwordState);
        }

        private void OnDestroy()
        {
            _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
        }

        private void OnProtagSwordState(Protaganist.ProtagSwordState state)
        {
            _visualContainer.rotation = Quaternion.AngleAxis(state.SwordAngle + _rotationOffset, Vector3.forward);
        }
    }
}