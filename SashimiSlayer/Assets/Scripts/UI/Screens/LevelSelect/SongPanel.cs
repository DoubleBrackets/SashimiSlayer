using System;
using Framework.LevelLoading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Screens.LevelSelect
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

        public event Action<GameLevelSO> OnLevelSelected;
        public event Action OnPanelSliced;

        private GameLevelSO _track;

        private bool _hardLevelSelected;

        public void SetupUI(GameLevelSO track)
        {
            _track = track;
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

        public void SelectLevel()
        {
            OnLevelSelected?.Invoke(_track);
        }

        public void PanelSliced()
        {
            OnPanelSliced?.Invoke();
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }
    }
}