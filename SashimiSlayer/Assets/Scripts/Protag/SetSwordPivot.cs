using Protag.SharedData;
using UnityEngine;

namespace Protag
{
    [ExecuteAlways]
    public class SetSwordPivot : MonoBehaviour
    {
        private void Awake()
        {
            Debug.Log($"Setting sword pivot to {transform.position}");
            ProtagGlobalData.ProtagSwordPivot = transform.position;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(ProtagGlobalData.ProtagSwordPivot, 0.5f);
        }
    }
}