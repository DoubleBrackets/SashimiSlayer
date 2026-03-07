using GameInput.Interface;
using TMPro;
using UnityEngine;

namespace GameInput.Extra
{
    /// <summary>
    ///     Sword calibration popup for the custom sword controller.
    ///     Pops up when a custom sword controller is detected, and allows the user to set the handedness
    /// </summary>
    public class SwordCalibrationPopup : MonoBehaviour
    {
        [SerializeField]
        private Transform _currentAngleIndicator;

        [SerializeField]
        private TMP_Text _currentHandednessText;

        [SerializeField]
        private GameObject _toggleGameObject;

        private float _currentAngle;

        public void SetVisible(bool visible)
        {
            _toggleGameObject.SetActive(visible);
        }

        public SwordHandedness GetHandedness()
        {
            return _currentAngle < 0
                ? SwordHandedness.LeftHandedSword
                : SwordHandedness.RightHandedSword;
        }

        public void SetCurrentRawAngle(float angle)
        {
            _currentAngle = angle;
            _currentAngleIndicator.localRotation = Quaternion.Euler(0, 0, _currentAngle);

            SwordHandedness handedness = GetHandedness();
            _currentHandednessText.text =
                handedness == SwordHandedness.RightHandedSword
                    ? "Your RIGHT hand is on the grip?"
                    : "Your LEFT hand is on the grip?";
        }
    }
}