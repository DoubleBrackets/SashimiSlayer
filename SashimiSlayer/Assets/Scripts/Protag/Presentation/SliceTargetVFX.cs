using EditorUtils.BoldHeader;
using GameJuice.VFX;
using NaughtyAttributes;
using Protag.Presentation.Events;
using Protag.Presentation.Slicing;
using UnityEngine;
using UnityEngine.Serialization;

namespace Protag.Presentation
{
    /// <summary>
    ///     Handles playing VFX on top of targets that are sliced
    /// </summary>
    public class SliceTargetVFX : MonoBehaviour
    {
        [FormerlySerializedAs("_objectSlicedEvent")]
        [BoldHeader("Slice Target VFX")]
        [InfoBox("Handles playing VFX on top of targets that are sliced")]
        [Header("Events (In)")]

        [SerializeField]
        private ObjectsSlicedEvent _objectsSlicedEvent;

        [Header("Depends")]

        [SerializeField]
        private OneshotVFX _sliceVFX;

        private void Awake()
        {
            _objectsSlicedEvent.AddListener(OnObjectSliced);
        }

        private void OnDestroy()
        {
            _objectsSlicedEvent.RemoveListener(OnObjectSliced);
        }

        private void OnObjectSliced(ProtagSlicedObjectList objectList)
        {
            foreach (ProtagSlicedObject data in objectList.SlicedObjects)
            {
                OneshotVFX.PlayVFX(_sliceVFX, data.Position, Quaternion.Euler(0, 0, data.Angle));
            }
        }
    }
}