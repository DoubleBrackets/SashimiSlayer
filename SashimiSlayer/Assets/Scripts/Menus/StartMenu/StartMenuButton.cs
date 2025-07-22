using DG.Tweening;
using EditorUtils.BoldHeader;
using GameInput;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Menus.StartMenu
{
    /// <summary>
    ///     Button for the start menu, triggered by block
    /// </summary>
    public class StartMenuButton : MonoBehaviour
    {
        [BoldHeader("Start Menu Button")]
        [InfoBox("Button for the start menu, triggered by a block")]
        [Header("Depends")]

        [SerializeField]
        private Transform _buttonTransform;

        [Header("Config")]

        [SerializeField]
        private SharedTypes.BlockPoseStates _pressPose;

        [SerializeField]
        private bool _pressOnce;

        [SerializeField]
        private float _shakeDuration;

        [SerializeField]
        private float _shakeStrength;

        [SerializeField]
        private int _shakeVibrato;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onButtonPressed;

        private bool _pressed;

        private Tweener _shakeTween;

        private void Start()
        {
            InputService.Instance.OnBlockPoseChanged += OnBlockPoseChanged;
        }

        private void OnDestroy()
        {
            InputService.Instance.OnBlockPoseChanged -= OnBlockPoseChanged;
        }

        private void OnBlockPoseChanged(SharedTypes.BlockPoseStates blockPose)
        {
            if (blockPose != _pressPose)
            {
                return;
            }

            if (_pressOnce && _pressed)
            {
                return;
            }

            if (_shakeTween != null)
            {
                _shakeTween.Complete();
            }

            _onButtonPressed.Invoke();
            _shakeTween = _buttonTransform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato);
            _pressed = true;
        }
    }
}