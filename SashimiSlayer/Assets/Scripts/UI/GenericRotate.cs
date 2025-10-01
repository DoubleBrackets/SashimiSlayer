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

    public void Rotate()
    {
        transform.DORotate(_rotateTo, _expandDuration);
    }

    public void Unrotate()
    {
        transform.DORotate(_defaultRotate, _expandDuration);
    }
}