using System.Collections.Generic;
using Framework.LevelLoading;
using UnityEngine;

namespace UI.Screens.LevelSelect
{
    [CreateAssetMenu(fileName = "SongRoster", menuName = "MainMenu/SongRoster")]
    public class SongRosterSO : ScriptableObject
    {
        [field: SerializeField]
        public List<GameLevelSO> Songs { get; private set; }
    }
}