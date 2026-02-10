using CommonTypes;
using DG.Tweening;
using EditorUtils.BoldHeader;
using Framework;
using GameInput.Interface;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Menus.StartMenu
{
    /// <summary>
    ///     Button for the start menu, triggered by block
    /// </summary>
    public class StartMenuButton : MonoBehaviour, IFindsDependencies
    {
        [BoldHeader("Start Menu Button")]
        [InfoBox("Button for the start menu, triggered by a block")]
        [Header("Depends")]

        [SerializeField]
        private Transform _buttonTransform;

        [Header("Config")]

        [SerializeField]
        private BlockPoseStates _pressPose;

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

        private IUserInput _userInput;

        public void FindDependencies()
        {
            _userInput = ServiceLocator.GetUserInput();
        }

        private void Start()
        {
            FindDependencies();
            _userInput.OnBlockPoseChanged += OnBlockPoseChanged;
        }

        private void OnDestroy()
        {
            _userInput.OnBlockPoseChanged -= OnBlockPoseChanged;
        }

        private void OnBlockPoseChanged(BlockPoseStates blockPose)
        {
            if (!isActiveAndEnabled)
            {
                return;
            }

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