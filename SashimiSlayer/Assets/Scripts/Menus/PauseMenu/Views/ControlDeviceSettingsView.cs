using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text.RegularExpressions;
using Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.PauseMenu.Views
{
    public class ControlDeviceSettingsView : PauseMenuView
    {
        public const string LastPortName = "LastPortName";

        [Header("Events (Out)")]

        [SerializeField]
        private BoolEvent _setUseHardwareController;

        [SerializeField]
        private StringEvent _connectToSerialPort;

        [Header("Events (In)")]

        [SerializeField]
        private StringEvent _serialPortConnectionStatus;

        [Header("Depends")]

        [SerializeField]
        private TMP_Dropdown _inputDeviceDropdown;

        [SerializeField]
        private TMP_Dropdown _serialPortDropdown;

        [SerializeField]
        private Button _connectButton;

        [SerializeField]
        private TMP_Text _connectionStatusText;

        private string _lastPortName;

        public override void ViewDestroy()
        {
            _inputDeviceDropdown.onValueChanged.RemoveListener(HandleInputDeviceChanged);
            _connectButton.onClick.RemoveListener(HandleConnect);
            _serialPortConnectionStatus.RemoveListener(HandleSerialPortConnectionStatus);
        }

        public override void ViewAwake()
        {
            _lastPortName = PlayerPrefs.GetString(LastPortName, "");
            _inputDeviceDropdown.ClearOptions();
            _inputDeviceDropdown.AddOptions(new List<string> { "HID Input (Default)", "Sword Control Serial" });

            _inputDeviceDropdown.onValueChanged.AddListener(HandleInputDeviceChanged);
            _connectButton.onClick.AddListener(HandleConnect);
            _serialPortConnectionStatus.AddListener(HandleSerialPortConnectionStatus);
        }

        public override void ViewStart()
        {
            ReloadSerialPortDropdown();
        }

        private void HandleSerialPortConnectionStatus(string status)
        {
            _connectionStatusText.text = status;
        }

        /// <summary>
        ///     Quick Connect to the last used port
        /// </summary>
        public void QuickConnect()
        {
            HandleConnect();
        }

        public void ToggleInputMode()
        {
            _inputDeviceDropdown.value = _inputDeviceDropdown.value == 0 ? 1 : 0;
        }

        private void HandleConnect()
        {
            string serialPort = _serialPortDropdown.options[_serialPortDropdown.value].text;

            // Save to last used port
            PlayerPrefs.SetString(LastPortName, serialPort);
            _lastPortName = serialPort;

            _connectToSerialPort.Raise(serialPort);
        }

        private void HandleInputDeviceChanged(int index)
        {
            _setUseHardwareController.Raise(index == 1);
        }

        public void ReloadSerialPortDropdown()
        {
            List<string> serialPorts = SerialPort.GetPortNames().ToList();

            // Filter out ports that are not COM ports
            var comPortRegex = new Regex(@"COM\d+");
            var macPortRegex = new Regex(@"/dev/tty.*");

            serialPorts = serialPorts.Where(port =>
                    comPortRegex.IsMatch(port) ||
                    macPortRegex.IsMatch(port))
                .Reverse()
                .ToList();

            // Move last used port to the top
            if (!string.IsNullOrEmpty(_lastPortName))
            {
                if (serialPorts.Contains(_lastPortName))
                {
                    Debug.Log($"Moving {_lastPortName} to the top since it was last used");
                    serialPorts.Remove(_lastPortName);
                    serialPorts.Insert(0, _lastPortName);
                }
            }

            _serialPortDropdown.ClearOptions();
            _serialPortDropdown.AddOptions(serialPorts);
        }
    }
}