using UnityEngine;
using UnityEngine.Events;

namespace Menus
{
    /// <summary>
    ///     Simple toggle component intended for use with generic slice button
    /// </summary>
    public class ToggleUI : MonoBehaviour
    {
        [Header("Config")]

        [SerializeField]
        private bool _startToggledOn;

        [Header("UnityEvents")]

        [SerializeField]
        private UnityEvent _onToggledOn;

        [SerializeField]
        private UnityEvent _onToggledOff;

        private bool _isToggledOn;

        private void Awake()
        {
            SetState(_startToggledOn);
        }

        public void Toggle()
        {
            SetState(!_isToggledOn);
        }

        public void SetState(bool state)
        {
            _isToggledOn = state;
            if (_isToggledOn)
            {
                _onToggledOn?.Invoke();
            }
            else
            {
                _onToggledOff?.Invoke();
            }
        }
    }
}