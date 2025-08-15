using System.Collections.Generic;
using Core.Scene;
using Menus.ScoreScreen;
using UnityEngine;

namespace Menus.LevelSelect
{
    [CreateAssetMenu(fileName = "SongRoster", menuName = "MainMenu/SongRoster")]
    public class SongRosterSO : ScriptableObject
    {
        [field: SerializeField]
        public List<GameLevelSO> Songs { get; private set; }

        public void WipeHighScores()
        {
            Debug.Log("Wiping all high scores");
            foreach (GameLevelSO song in Songs)
            {
                if (song.NormalBeatmap)
                {
                    PlayerPrefs.SetFloat(FinalScoreDisplay.GetHighscorePrefKey(song.NormalBeatmap.BeatmapID), 0);
                }

                if (song.HardBeatmap)
                {
                    PlayerPrefs.SetFloat(FinalScoreDisplay.GetHighscorePrefKey(song.HardBeatmap.BeatmapID), 0);
                }
            }
        }
    }
}