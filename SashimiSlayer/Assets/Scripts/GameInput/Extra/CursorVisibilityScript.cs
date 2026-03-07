using EditorUtils.BoldHeader;
using Events.Basic;
using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using ValueSO;

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

        [SerializeField]
        private BoolEvent _optionsMenuToggled;

        private bool _isMenuOpen;
        private bool _cursorInput;

        private void Awake()
        {
            _currentControlScheme.AddListener(this, HandleControlSchemeChanged, true);
            _optionsMenuToggled.AddListener(HandleOptionsMenuToggled);
        }

        private void Update()
        {
            Cursor.visible = _isMenuOpen || _cursorInput;
        }

        private void OnDestroy()
        {
            _currentControlScheme.RemoveListener(this);
            _optionsMenuToggled.RemoveListener(HandleOptionsMenuToggled);
        }

        private void HandleControlSchemeChanged(ControlSchemes controlScheme)
        {
            _cursorInput = controlScheme == ControlSchemes.KeyboardMouse;
        }

        private void HandleOptionsMenuToggled(bool isMenuOpen)
        {
            _isMenuOpen = isMenuOpen;
        }
    }
}