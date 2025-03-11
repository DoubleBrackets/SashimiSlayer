using Core.Protag;
using UnityEngine;

public class SetSwordPosition : MonoBehaviour
{
    private void Awake()
    {
        Protaganist.Instance.SetSwordPosition(transform.position);
    }
}