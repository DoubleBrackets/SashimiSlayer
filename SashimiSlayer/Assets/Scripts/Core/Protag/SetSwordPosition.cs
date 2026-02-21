using Core.Protag;
using Framework.Services;
using UnityEngine;

public class SetSwordPosition : MonoBehaviour
{
    private void Awake()
    {
        ServiceLocator.GetService<Protaganist>().SetSwordPosition(transform.position);
    }
}