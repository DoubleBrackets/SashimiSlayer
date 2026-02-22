using Framework;
using Protag.Core;
using UnityEngine;

public class SetSwordPosition : MonoBehaviour
{
    private void Awake()
    {
        ServiceLocator.GetService<Protaganist>().SetSwordPosition(transform.position);
    }
}