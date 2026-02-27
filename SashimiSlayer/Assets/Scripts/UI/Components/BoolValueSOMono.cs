using UnityEngine;
using UnityEngine.Events;
using ValueSO;
using ValueSO.Core;

namespace UI.Components
{
    /// <summary>
    ///     Exposes BoolSO value as unityevent
    /// </summary>
    public class BoolValueSOMono : MonoBehaviour, IValueSOObserver
    {
        [SerializeField]
        private BoolValueSO _boolValueSO;

        [SerializeField]
        private bool _invertValue;

        [SerializeField]
        private UnityEvent<bool> _onValueChanged;

        private void Awake()
        {
            _boolValueSO.AddListener(this, HandleBoolValueChanged, true);
        }

        private void OnDestroy()
        {
            _boolValueSO.RemoveListener(this);
        }

        private void HandleBoolValueChanged(bool value)
        {
            _onValueChanged?.Invoke(_invertValue ? !value : value);
        }
    }
}