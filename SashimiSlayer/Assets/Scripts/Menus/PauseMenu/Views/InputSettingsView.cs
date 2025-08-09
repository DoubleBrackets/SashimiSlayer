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
        private UnityEngine.UI.Toggle _swordAngleFlipToggle;

        [SerializeField]
        private TMP_Dropdown _upAxisDropdown;

        [SerializeField]
        private Toggle _flipParryDirectionToggle;

        private float _swordAngleMultiplier;
        private float _swordAngleOffset;
        private bool _swordAngleFlip;
        private int _upAxis;
        private bool _flipParryDirection;

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

            HandleSwordAngleMultiplierChange(_swordAngleMultiplier);
            HandleUpAxisChange(_upAxis);
            UpdateSwordAngleMultiplier();
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
            _flipParryDirectionChangeEvent.Raise(_flipParryDirection);
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
            UpdateSwordAngleMultiplier();
        }

        private void HandleSwordAngleFlipChange(bool value)
        {
            _swordAngleFlip = value;
            PlayerPrefs.SetInt(FlipSwordAim, _swordAngleFlip ? 1 : 0);
            UpdateSwordAngleMultiplier();
            UpdateSwordAngleOffset();
        }

        public void ToggleFlipSwordAim()
        {
            _swordAngleFlipToggle.isOn = !_swordAngleFlipToggle.isOn;
        }

        private void UpdateSwordAngleMultiplier()
        {
            _swordAngleMultiplierChangeEvent.Raise(_swordAngleMultiplier * (_swordAngleFlip ? -1 : 1));
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
    }
}