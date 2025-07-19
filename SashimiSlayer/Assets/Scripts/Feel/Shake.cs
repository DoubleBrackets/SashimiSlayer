using System;
using DG.Tweening;
using UnityEngine;

public class Shake : MonoBehaviour
{
    [Flags]
    private enum ShakeTypes
    {
        Position = 1 << 0,
        Rotation = 1 << 1,
        Scale = 1 << 2
    }

    [Serializable]
    public struct ShakeConfig
    {
        [SerializeField]
        public float _duration;

        [SerializeField]
        public Vector3 _strength;

        [SerializeField]
        public int _vibrato;

        [SerializeField]
        public bool _fadeOut;
    }

    [SerializeField]
    private Transform _targetTransform;

    [SerializeField]
    private ShakeTypes _shakeTypes;

    [SerializeField]
    private ShakeConfig _translationShakeConfig;

    [SerializeField]
    private ShakeConfig _rotationShakeConfig;

    [SerializeField]
    private ShakeConfig _scaleShakeConfig;

    private Tween _posTween;
    private Tween _rotTween;
    private Tween _scaleTween;

    public void DoShake()
    {
        if (_targetTransform == null)
        {
            Debug.LogError("Target transform is not set.");
            return;
        }

        if (_shakeTypes.HasFlag(ShakeTypes.Position))
        {
            if (_posTween != null && _posTween.IsActive())
            {
                _posTween.Complete();
            }

            _posTween = _targetTransform.DOShakePosition(_translationShakeConfig._duration,
                _translationShakeConfig._strength,
                _translationShakeConfig._vibrato, 90f, _translationShakeConfig._fadeOut);
        }

        if (_shakeTypes.HasFlag(ShakeTypes.Rotation))
        {
            if (_rotTween != null && _rotTween.IsActive())
            {
                _rotTween.Complete();
            }

            _rotTween = _targetTransform.DOShakeRotation(_rotationShakeConfig._duration, _rotationShakeConfig._strength,
                _rotationShakeConfig._vibrato, 90f, _rotationShakeConfig._fadeOut);
        }

        if (_shakeTypes.HasFlag(ShakeTypes.Scale))
        {
            if (_scaleTween != null && _scaleTween.IsActive())
            {
                _scaleTween.Complete();
            }

            _scaleTween = _targetTransform.DOShakeScale(_scaleShakeConfig._duration, _scaleShakeConfig._strength,
                _scaleShakeConfig._vibrato, 90f, _scaleShakeConfig._fadeOut);
        }
    }
}