using System.Collections.Generic;
using Core.Scene;
using UnityEngine;

namespace Menus.LevelSelect
{
    [CreateAssetMenu(fileName = "SongRoster", menuName = "MainMenu/SongRoster")]
    public class SongRosterSO : ScriptableObject
    {
        [field: SerializeField]
        public List<GameLevelSO> Songs { get; private set; }
    }
}