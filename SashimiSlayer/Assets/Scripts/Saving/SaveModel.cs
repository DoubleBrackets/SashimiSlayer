using System;
using System.Collections.Generic;

namespace Saving
{
    /// <summary>
    ///     Simple serializable class to hold all game persistent data
    /// </summary>
    [Serializable]
    public class SaveModel
    {
        public float ScreenShakeRatio = 1;

        // Audio
        public float MasterVolume = 1;
        public float SfxVolume = 1;
        public float MusicVolume = 1;

        // Input
        public float SwordAimMultiplier = 1;
        public float SwordAngleOffset;
        public bool FlipSwordAim;
        public int UpAxis;
        public bool FlipParryDirection;

        // Controller
        public string LastConnectedSerialPortName;

        // Highscore
        public List<HighScoreSaveModel> HighScores;

        public string InputBindingOverrides;
    }

    [Serializable]
    public struct HighScoreSaveModel
    {
        public string NameKey;
        public int FinalScore;
        public int Perfects;
        public int Earlies;
        public int Late;
        public int Miss;
    }
}