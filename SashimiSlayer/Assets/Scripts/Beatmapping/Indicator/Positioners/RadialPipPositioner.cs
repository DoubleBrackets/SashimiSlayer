using System.Collections.Generic;
using UnityEngine;

namespace Beatmapping.Indicator.Positioners
{
    [CreateAssetMenu(fileName = "RadialPipPositioner")]
    public class RadialPipPositioner : PipPositioner
    {
        [Header("Layout")]

        [SerializeField]
        private float _finalPipAngle;

        [SerializeField]
        private float _pipIntervalAngle;

        [SerializeField]
        private float _finalPipIntervalAngle;

        [SerializeField]
        private float _pipRadius;

        [SerializeField]
        private int _pipDirection;

        [SerializeField]
        private Vector2 _centerOffset;

        [Header("Rotation")]

        [SerializeField]
        private float _rotateOffset;

        public override List<(Vector2, float)> CalculatePipLocalPositions(int totalPips)
        {
            float angleAccumulate = _finalPipAngle;

            var positions = new List<(Vector2, float)>(totalPips);
            for (var i = 0; i < totalPips; i++)
            {
                Vector2 dir = Quaternion.Euler(
                                  0,
                                  0,
                                  angleAccumulate) *
                              Vector2.up;

                Vector2 pos = dir * _pipRadius + _centerOffset;
                float angle = angleAccumulate + _rotateOffset;
                positions.Add((pos, angle));
                angleAccumulate += (i == 0 ? _finalPipIntervalAngle : _pipIntervalAngle) * _pipDirection;
            }

            return positions;
        }
    }
}