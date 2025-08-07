using System;
using Core.Scene;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus.LevelSelect
{
    public class LevelPanel : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text _levelNameText;

        [SerializeField]
        private TMP_Text _levelDescriptionText;

        [SerializeField]
        private Image _thumbnailImage;

        public event Action<GameLevelSO> OnLevelSelected;

        private GameLevelSO _level;

        public void SetupUI(GameLevelSO level)
        {
            _levelNameText.text = level.LevelTitle;
            _levelDescriptionText.text = level.LevelDescription;
            _level = level;
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
            OnLevelSelected?.Invoke(_level);
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}