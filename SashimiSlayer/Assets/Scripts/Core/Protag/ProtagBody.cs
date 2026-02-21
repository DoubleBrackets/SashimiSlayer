using EditorUtils.BoldHeader;
using Events;
using Framework.Services;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Protag
{
    public class ProtagBody : MonoBehaviour
    {
        [BoldHeader("Protag Body")]
        [InfoBox("Provides info on the Protag's physical body")]
        [Header("Visuals")]

        [Tooltip("The target transform that Notes move towards")]
        [SerializeField]
        private Transform _targetTransform;

        [SerializeField]
        private Transform _swordPivot;

        [Header("Event (Out)")]

        [SerializeField]
        private Vector2Event _swordPivotPositionChangeEvent;

        public Vector3 TargetPosition => _targetTransform.position;

        private void Awake()
        {
            ServiceLocator.GetService<Protaganist>().NoteTargetPosition = TargetPosition;
        }

        private void Start()
        {
            _swordPivotPositionChangeEvent.Raise(_swordPivot.position);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(TargetPosition, 0.25f);
        }
    }
}