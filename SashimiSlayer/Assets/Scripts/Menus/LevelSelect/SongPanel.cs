using System;
using Core.Scene;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Menus.LevelSelect
{
    public class SongPanel : MonoBehaviour
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

        private GameLevelSO _track;

        private bool _hardLevelSelected;

        public void SetupUI(GameLevelSO track)
        {
            _track = track;
            SetDifficulty(false);
            UpdateUI(track);
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
            OnLevelSelected?.Invoke(_track);
        }

        public void SetDifficulty(bool isHardDifficulty)
        {
            if (isHardDifficulty && _track.HardBeatmap == null)
            {
                return;
            }

            _onHardLevelToggled?.Invoke(isHardDifficulty);

            _hardLevelSelected = isHardDifficulty;
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}