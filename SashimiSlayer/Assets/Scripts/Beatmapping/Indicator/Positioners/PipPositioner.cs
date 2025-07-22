using System.Collections.Generic;
using UnityEngine;

namespace Beatmapping.Indicator.Positioners
{
    public abstract class PipPositioner : ScriptableObject
    {
        public abstract List<(Vector2, float)> CalculatePipLocalPositions(int totalPips);
    }
}