using DG.Tweening;
using EditorUtils.BoldHeader;
using Interactions.Components;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace UI.Screens.StartMenu
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

        [SerializeField]
        private BlockableMono _blockable;

        [Header("Config")]

        [SerializeField]
        private float _shakeDuration;

        [SerializeField]
        private float _shakeStrength;

        [SerializeField]
        private int _shakeVibrato;

        [Header("Events")]

        [SerializeField]
        private UnityEvent _onButtonPressed;

        private Tweener _shakeTween;

        private void OnEnable()
        {
            _blockable.OnBlocked += PressButton;
        }

        private void OnDisable()
        {
            _blockable.OnBlocked -= PressButton;
        }

        private void PressButton()
        {
            if (_shakeTween != null)
            {
                _shakeTween.Complete();
            }

            _onButtonPressed.Invoke();
            _shakeTween = _buttonTransform.DOShakePosition(_shakeDuration, _shakeStrength, _shakeVibrato);
        }
    }
}