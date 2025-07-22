using EditorUtils.BoldHeader;
using GameInput;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;

namespace Menus.StartMenu
{
    public class SliceInputHook : MonoBehaviour
    {
        [BoldHeader("Slice Input Hook")]
        [InfoBox("Passes slice input through a unity event")]
        [SerializeField]
        private UnityEvent _onSlice;

        private void Start()
        {
            InputService.Instance.OnSheathStateChanged += OnSheathStateChanged;
        }

        private void OnDestroy()
        {
            InputService.Instance.OnSheathStateChanged -= OnSheathStateChanged;
        }

        private void OnSheathStateChanged(SharedTypes.SheathState sheathState)
        {
            if (sheathState == SharedTypes.SheathState.Unsheathed)
            {
                _onSlice.Invoke();
            }
        }
    }
}