using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameJuice.VFX
{
    public class DistortionEffect : MonoBehaviour
    {
        private static readonly int DistortionStrength = Shader.PropertyToID("_DistortionStrength");

        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private float _duration;

        [SerializeField]
        private float _strength;

        [SerializeField]
        private AnimationCurve _strengthOverTime;

        private void Start()
        {
            Distort().Forget();
        }

        private async UniTaskVoid Distort()
        {
            var materialPropertyBlock = new MaterialPropertyBlock();
            _spriteRenderer.GetPropertyBlock(materialPropertyBlock);

            var currentTime = 0f;

            while (currentTime < _duration)
            {
                float strength = _strengthOverTime.Evaluate(currentTime / _duration) * _strength;
                materialPropertyBlock.SetFloat(DistortionStrength, strength);
                _spriteRenderer.SetPropertyBlock(materialPropertyBlock);
                currentTime += Time.deltaTime;
                await UniTask.Yield();
            }

            Destroy(gameObject);
        }
    }
}