using Protag.SharedData;
using UnityEngine;

namespace Protag
{
    [ExecuteAlways]
    public class SetProtagTargetPos : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"Setting protag target position to {transform.position}");
            ProtagGlobalData.ProtagTargetPosition = transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ProtagGlobalData.ProtagTargetPosition, 0.5f);
        }
    }
}