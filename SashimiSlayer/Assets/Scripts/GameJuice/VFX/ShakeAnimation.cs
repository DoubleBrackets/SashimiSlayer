using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameJuice.VFX
{
    public class ShakeAnimation : MonoBehaviour
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
            [FormerlySerializedAs("_duration")]
            [SerializeField]
            public float Duration;

            [FormerlySerializedAs("_strength")]
            [SerializeField]
            public Vector3 Strength;

            [FormerlySerializedAs("_vibrato")]
            [SerializeField]
            public int Vibrato;

            [FormerlySerializedAs("_fadeOut")]
            [SerializeField]
            public bool FadeOut;
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

                _posTween = _targetTransform.DOShakePosition(_translationShakeConfig.Duration,
                    _translationShakeConfig.Strength,
                    _translationShakeConfig.Vibrato, 90f, _translationShakeConfig.FadeOut);
            }

            if (_shakeTypes.HasFlag(ShakeTypes.Rotation))
            {
                if (_rotTween != null && _rotTween.IsActive())
                {
                    _rotTween.Complete();
                }

                _rotTween = _targetTransform.DOShakeRotation(_rotationShakeConfig.Duration,
                    _rotationShakeConfig.Strength,
                    _rotationShakeConfig.Vibrato, 90f, _rotationShakeConfig.FadeOut);
            }

            if (_shakeTypes.HasFlag(ShakeTypes.Scale))
            {
                if (_scaleTween != null && _scaleTween.IsActive())
                {
                    _scaleTween.Complete();
                }

                _scaleTween = _targetTransform.DOShakeScale(_scaleShakeConfig.Duration, _scaleShakeConfig.Strength,
                    _scaleShakeConfig.Vibrato, 90f, _scaleShakeConfig.FadeOut);
            }
        }
    }
}