using UnityEngine;

namespace Interactions.DataTypes
{
    public struct SwordAim
    {
        public Vector2 Position;
        public float Angle;

        public float DistanceToSwordPlane(Vector3 position)
        {
            Vector3 swordPlaneNormal = Quaternion.Euler(0, 0, Angle) * Vector3.up;
            var swordPlanePoint = (Vector3)Position;

            Vector3 pointOnPlane = position - swordPlanePoint;
            float distance = Mathf.Abs(Vector3.Dot(pointOnPlane, swordPlaneNormal));
            return distance;
        }
    }
}