using System.Collections.Generic;
using UnityEngine;

namespace Protag.Presentation.Slicing
{
    /// <summary>
    ///     Represents an object that has been sliced by the protagonist, intended for presentation purposes.
    /// </summary>
    public struct ProtagSlicedObject
    {
        public Vector2 Position;
        public float Angle;
    }

    public struct ProtagSlicedObjectList
    {
        public List<ProtagSlicedObject> SlicedObjects;
    }
}