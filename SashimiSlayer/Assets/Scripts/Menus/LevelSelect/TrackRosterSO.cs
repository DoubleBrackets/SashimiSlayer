using System;
using System.Collections.Generic;
using Core.Scene;
using Menus.ScoreScreen;
using UnityEngine;

namespace Menus.LevelSelect
{
    [CreateAssetMenu(fileName = "TrackRoster", menuName = "MainMenu/TrackRoster")]
    public class TrackRosterSO : ScriptableObject
    {
        [Serializable]
        public struct TrackEntry
        {
            public GameLevelSO NormalMap;
            public GameLevelSO HardMap;
        }

        [field: SerializeField]
        public List<TrackEntry> Tracks { get; private set; }

        public void WipeHighScores()
        {
            Debug.Log("Wiping all high scores");
            foreach (TrackEntry map in Tracks)
            {
                PlayerPrefs.SetFloat(FinalScoreDisplay.GetHighscorePrefKey(map.NormalMap.Beatmap.BeatmapID), 0);
                PlayerPrefs.SetFloat(FinalScoreDisplay.GetHighscorePrefKey(map.HardMap.Beatmap.BeatmapID), 0);
            }
        }
    }
}