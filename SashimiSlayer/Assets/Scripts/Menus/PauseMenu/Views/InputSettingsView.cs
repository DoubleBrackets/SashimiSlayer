using System.Collections.Generic;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.PauseMenu.Views
{
    public class InputSettingsView : PauseMenuView
    {
        private const string SwordAimMultiplier = "SwordAngleMultiplier";
        private const string SwordAngleOffset = "SwordAngleOffset";
        private const string FlipSwordAim = "SwordAngleFlip";
        private const string UpAxis = "UpAxis";
        private const string FlipParryDirection = "FlipParryDirection";

        [Header("Events (Out)")]

        [SerializeField]
        private FloatEvent _swordAngleMultiplierChangeEvent;

        [SerializeField]
        private FloatEvent _swordAngleOffsetChangeEvent;

        [SerializeField]
        private IntEvent _upAxisChangedEvent;

        [SerializeField]
        private BoolEvent _flipParryDirectionChangeEvent;

        [Header("UI")]

        [SerializeField]
        private Slider _swordAngleMultiplierSlider;

        [SerializeField]
        private Slider _swordAngleOffsetSlider;

        [SerializeField]
        private Toggle _swordAngleFlipToggle;

        [SerializeField]
        private TMP_Dropdown _upAxisDropdown;

        [SerializeField]
        private Toggle _flipParryDirectionToggle;

        private float _swordAngleMultiplier;
        private float _swordAngleOffset;
        private bool _swordAngleFlip;
        private int _upAxis;
        private bool _flipParryDirection;

        private bool _isLeftHanded;

        public override void ViewAwake()
        {
            _swordAngleMultiplier = PlayerPrefs.GetFloat(SwordAimMultiplier, 1);
            _swordAngleOffset = PlayerPrefs.GetFloat(SwordAngleOffset, 0);
            _swordAngleFlip = PlayerPrefs.GetInt(FlipSwordAim, 0) == 1;
            _upAxis = PlayerPrefs.GetInt(UpAxis, 1);
            _flipParryDirection = PlayerPrefs.GetInt(FlipParryDirection, 0) == 1;

            _swordAngleMultiplierSlider.onValueChanged.AddListener(HandleSwordAngleMultiplierChange);
            _swordAngleOffsetSlider.onValueChanged.AddListener(HandleSwordAngleOffsetChange);
            _swordAngleFlipToggle.onValueChanged.AddListener(HandleSwordAngleFlipChange);
            _upAxisDropdown.onValueChanged.AddListener(HandleUpAxisChange);
            _flipParryDirectionToggle.onValueChanged.AddListener(HandleFlipParryDirectionChange);

            SetupDropdown();
        }

        public override void ViewStart()
        {
            _swordAngleMultiplierSlider.value = _swordAngleMultiplier;
            _swordAngleOffsetSlider.value = _swordAngleOffset;
            _swordAngleFlipToggle.isOn = _swordAngleFlip;
            _upAxisDropdown.value = _upAxis;
            _flipParryDirectionToggle.isOn = _flipParryDirection;

            // Need to explicitly call these methods to ensure the initial values are set correctly
            // Since UI callbacks don't trigger if the value is the same as the current value (i.e default)
            HandleSwordAngleMultiplierChange(_swordAngleMultiplier);
            HandleUpAxisChange(_upAxis);
            UpdateSwordAngleMultiplier(_swordAngleMultiplier, _swordAngleFlip, _isLeftHanded);
            HandleFlipParryDirectionChange(_flipParryDirection);
        }

        public override void ViewDestroy()
        {
            _swordAngleMultiplierSlider.onValueChanged.RemoveListener(HandleSwordAngleMultiplierChange);
            _swordAngleOffsetSlider.onValueChanged.RemoveListener(HandleSwordAngleOffsetChange);
            _swordAngleFlipToggle.onValueChanged.RemoveListener(HandleSwordAngleFlipChange);
            _upAxisDropdown.onValueChanged.RemoveListener(HandleUpAxisChange);
            _flipParryDirectionToggle.onValueChanged.RemoveListener(HandleFlipParryDirectionChange);
        }

        private void HandleFlipParryDirectionChange(bool value)
        {
            _flipParryDirection = value;
            PlayerPrefs.SetInt(FlipParryDirection, _flipParryDirection ? 1 : 0);
            _flipParryDirectionChangeEvent.Raise(_flipParryDirection ^ _isLeftHanded);
        }

        private void SetupDropdown()
        {
            _upAxisDropdown.ClearOptions();
            _upAxisDropdown.AddOptions(new List<string> { "X", "Y", "Z" });
        }

        private void HandleUpAxisChange(int value)
        {
            _upAxis = value;
            PlayerPrefs.SetInt(UpAxis, _upAxis);
            _upAxisChangedEvent.Raise(_upAxis);
        }

        private void HandleSwordAngleMultiplierChange(float value)
        {
            _swordAngleMultiplier = value;
            PlayerPrefs.SetFloat(SwordAimMultiplier, _swordAngleMultiplier);
            UpdateSwordAngleMultiplier(_swordAngleMultiplier, _swordAngleFlip, _isLeftHanded);
        }

        private void HandleSwordAngleFlipChange(bool value)
        {
            _swordAngleFlip = value;
            PlayerPrefs.SetInt(FlipSwordAim, _swordAngleFlip ? 1 : 0);
            UpdateSwordAngleMultiplier(_swordAngleMultiplier, _swordAngleFlip, _isLeftHanded);
            UpdateSwordAngleOffset();
        }

        public void ToggleFlipSwordAim()
        {
            _swordAngleFlipToggle.isOn = !_swordAngleFlipToggle.isOn;
        }

        private void UpdateSwordAngleMultiplier(float swordAngleMultiplier, bool swordAngleFlip, bool isLeftHanded)
        {
            _swordAngleMultiplierChangeEvent.Raise(swordAngleMultiplier * (swordAngleFlip ^ isLeftHanded ? -1 : 1));
        }

        private void HandleSwordAngleOffsetChange(float value)
        {
            _swordAngleOffset = value;
            PlayerPrefs.SetFloat(SwordAngleOffset, _swordAngleOffset);
            UpdateSwordAngleOffset();
        }

        private void UpdateSwordAngleOffset()
        {
            _swordAngleOffsetChangeEvent.Raise(_swordAngleOffset * (_swordAngleFlip ? -1 : 1));
        }

        public void ToggleFlipAngle()
        {
            _swordAngleFlipToggle.isOn = !_swordAngleFlipToggle.isOn;
        }

        /// <summary>
        ///     Left-handedness is a "hidden" setting that is only set through exhibition hotkey (specific joystick button from
        ///     sword joystick)
        ///     This modifies the parry flip and angle multiplier settings
        /// </summary>
        public void SetIsLeftHanded(bool isLeftHanded)
        {
            Debug.Log($"Left-handed mode set to: {isLeftHanded}");
            _isLeftHanded = isLeftHanded;
            HandleFlipParryDirectionChange(_flipParryDirection);
            HandleSwordAngleFlipChange(_swordAngleFlip);
        }
    }
}