using EditorUtils.BoldHeader;
using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using ValueSO;
using ValueSO.Core;

namespace GameInput.Extra
{
    /// <summary>
    ///     Handles hiding/showing cursor
    /// </summary>
    public class CursorVisibilityScript : MonoBehaviour, IValueSOObserver
    {
        [BoldHeader("Cursor Visibility")]
        [Header("Events (In)")]

        [SerializeField]
        private ControlSchemeValueSO _currentControlScheme;

        [Header("ValueSO (In)")]

        [SerializeField]
        private BoolValueSO _isMenuOpenValueSO;

        private void Update()
        {
            Cursor.visible = !_isMenuOpenValueSO.Value && _currentControlScheme.Value == ControlSchemes.KeyboardMouse;
        }

        private void OnDestroy()
        {
            _currentControlScheme.RemoveListener(this);
        }
    }
}