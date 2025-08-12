using System;
using Core.Scene;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menus.LevelSelect
{
    public class TrackPanel : MonoBehaviour
    {
        [Header("Depends")]

        [SerializeField]
        private TMP_Text _levelNameText;

        [SerializeField]
        private TMP_Text _levelDescriptionText;

        [SerializeField]
        private Image _thumbnailImage;

        [Header("UnityEvents")]

        [SerializeField]
        private UnityEvent<bool> _onHardLevelToggled;

        public event Action<GameLevelSO> OnLevelSelected;

        private GameLevelSO _normalLevel;
        private GameLevelSO _hardLevel;

        private bool _hardLevelSelected;

        public void SetupUI(TrackRosterSO.TrackEntry track)
        {
            _normalLevel = track.NormalMap;
            _hardLevel = track.HardMap;
            SetHardLevel(false);
            UpdateUI(_normalLevel);
        }

        private void UpdateUI(GameLevelSO level)
        {
            if (level == null)
            {
                return;
            }

            _levelNameText.text = level.LevelTitle;
            _levelDescriptionText.text = level.LevelDescription;
            _thumbnailImage.sprite = level.LevelSelectSprite;
        }

        public void SetHovered(bool val)
        {
            if (val)
            {
                transform.DOScale(1.25f, 0.15f);
            }
            else
            {
                transform.DOScale(1f, 0.15f);
            }
        }

        public void SelectLevel()
        {
            OnLevelSelected?.Invoke(_hardLevelSelected ? _hardLevel : _normalLevel);
        }

        public void SetHardLevel(bool isHardLevel)
        {
            if (isHardLevel && _hardLevel == null)
            {
                return;
            }

            _onHardLevelToggled?.Invoke(isHardLevel);

            _hardLevelSelected = isHardLevel;
            UpdateUI(_hardLevelSelected ? _hardLevel : _normalLevel);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}