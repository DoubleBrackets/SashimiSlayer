using EditorUtils.BoldHeader;
using Events;
using UnityEngine;

namespace GameInput
{
    /// <summary>
    ///     Handles hiding/showing cursor
    /// </summary>
    public class CursorVisibilityScript : MonoBehaviour
    {
        [BoldHeader("Cursor Visibility")]
        [Header("Events (In)")]

        [SerializeField]
        private IntEvent _controlSchemeChanged;

        [SerializeField]
        private BoolEvent _optionsMenuToggled;

        private bool _isMenuOpen;
        private bool _cursorInput;

        private void Awake()
        {
            _controlSchemeChanged.AddListener(HandleControlSchemeChanged);
            _optionsMenuToggled.AddListener(HandleOptionsMenuToggled);
        }

        private void Update()
        {
            Cursor.visible = _isMenuOpen || _cursorInput;
        }

        private void OnDestroy()
        {
            _controlSchemeChanged.RemoveListener(HandleControlSchemeChanged);
            _optionsMenuToggled.RemoveListener(HandleOptionsMenuToggled);
        }

        private void HandleControlSchemeChanged(int controlScheme)
        {
            _cursorInput = controlScheme == (int)ControlSchemes.KeyboardMouse;
        }

        private void HandleOptionsMenuToggled(bool isMenuOpen)
        {
            _isMenuOpen = isMenuOpen;
        }
    }
}