using CommonTypes;
using EditorUtils.BoldHeader;
using Framework.Services;
using GameInput.Interface;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Menus.StartMenu
{
    /// <summary>
    ///     Passes slice input through a unity event for quick and dirty input
    /// </summary>
    public class SliceInputHook : MonoBehaviour, IFindsDependencies
    {
        [BoldHeader("Slice Input Hook")]
        [InfoBox("Passes slice input through a unity event")]
        [SerializeField]
        private UnityEvent _onSlice;

        private IUserInput _userInput;

        public void FindDependencies()
        {
            _userInput = ServiceLocator.GetService<IUserInput>();
        }

        private void Start()
        {
            FindDependencies();
            _userInput.OnSheathStateChanged += OnSheathStateChanged;
        }

        private void OnDestroy()
        {
            _userInput.OnSheathStateChanged -= OnSheathStateChanged;
        }

        private void OnSheathStateChanged(SheathState sheathState)
        {
            if (sheathState == SheathState.Unsheathed)
            {
                _onSlice.Invoke();
            }
        }
    }
}