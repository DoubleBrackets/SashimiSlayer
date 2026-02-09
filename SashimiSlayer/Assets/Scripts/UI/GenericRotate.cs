using DG.Tweening;
using UnityEngine;

public class GenericRotate : MonoBehaviour
{
    [SerializeField]
    private float _expandDuration;

    [SerializeField]
    private Vector3 _rotateTo;

    [SerializeField]
    private Vector3 _defaultRotate;

    private void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(_defaultRotate);
    }

    public void Rotate()
    {
        transform.DOLocalRotate(_rotateTo, _expandDuration);
    }

    public void Unrotate()
    {
        transform.DOLocalRotate(_defaultRotate, _expandDuration);
    }
}