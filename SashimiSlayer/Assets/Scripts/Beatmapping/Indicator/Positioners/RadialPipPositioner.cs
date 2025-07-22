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
        private float _pipRadius;

        [SerializeField]
        private int _pipDirection;

        [SerializeField]
        private Vector2 _centerOffset;

        [SerializeField]
        private float _rotateOffset;

        public override List<(Vector2, float)> CalculatePipLocalPositions(int totalPips)
        {
            float startingAngle = _finalPipAngle;

            var positions = new List<(Vector2, float)>(totalPips);
            for (var i = 0; i < totalPips; i++)
            {
                Vector2 dir = Quaternion.Euler(
                                  0,
                                  0,
                                  startingAngle + i * _pipIntervalAngle * _pipDirection) *
                              Vector2.up;

                Vector2 pos = dir * _pipRadius + _centerOffset;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + _rotateOffset;
                positions.Add((dir * _pipRadius + _centerOffset, angle));
            }

            return positions;
        }
    }
}