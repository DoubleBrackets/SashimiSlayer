using Events;
using Protag.Presentation.Slicing;
using UnityEngine;

namespace Protag.Presentation.Events
{
    [CreateAssetMenu(menuName = "Events/Protag/ObjectSlicedEvent")]
    public class ObjectsSlicedEvent : SOEvent<ProtagSlicedObjectList>
    {
    }
}