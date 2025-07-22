using System.Collections.Generic;
using Beatmapping;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

namespace Core.Scene
{
    [CreateAssetMenu(fileName = "Game Level", menuName = "Level", order = 51)]
    public class GameLevelSO : ScriptableObject
    {
        public enum LevelTypes
        {
            MainMenu,
            Gameplay,
            LevelResults
        }

        [field: SerializeField]
        [field: Scene]
        public string GameSceneName { get; private set; }

        [field: SerializeField]
        [field: TextArea]
        public string LevelTitle { get; private set; }

        [field: SerializeField]
        [field: TextArea]
        public string LevelDescription { get; private set; }

        [field: SerializeField]
        public BeatmapConfigSo Beatmap { get; private set; }

        [field: SerializeField]
        public LevelTypes LevelType { get; private set; }

        [field: SerializeField]
        [field: ShowAssetPreview]
        public Sprite LevelSelectSprite { get; private set; }

        [field: SerializeField]
        [field: BankRef]
        public List<string> FmodBanksToPreLoad { get; private set; }
    }
}