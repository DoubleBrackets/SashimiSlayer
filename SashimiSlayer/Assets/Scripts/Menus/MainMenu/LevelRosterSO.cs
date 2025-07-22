using System.Collections.Generic;
using Core.Scene;
using Menus.ScoreScreen;
using UnityEngine;

namespace Menus.MainMenu
{
    [CreateAssetMenu(fileName = "LevelRoster", menuName = "MainMenu/LevelRoster")]
    public class LevelRosterSO : ScriptableObject
    {
        [field: SerializeField]
        public List<GameLevelSO> Levels { get; private set; }

        public void WipeHighScores()
        {
            Debug.Log("Wiping all high scores");
            foreach (GameLevelSO level in Levels)
            {
                PlayerPrefs.SetFloat(FinalScoreDisplay.GetHighscorePrefKey(level.Beatmap.BeatmapID), 0);
            }
        }
    }
}