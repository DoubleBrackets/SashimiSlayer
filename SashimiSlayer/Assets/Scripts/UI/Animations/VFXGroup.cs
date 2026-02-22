using UnityEngine;

namespace UI.Animations
{
    public class VFXGroup : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer _spriteRenderer;

        [SerializeField]
        private ParticleSystem[] _particles;

        public void Sliced()
        {
            _spriteRenderer.enabled = false;
            foreach (ParticleSystem particle in _particles)
            {
                particle.Play();
            }
        }
    }
}