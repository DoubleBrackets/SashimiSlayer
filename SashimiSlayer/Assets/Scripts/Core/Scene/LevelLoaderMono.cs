using Cysharp.Threading.Tasks;
using Framework.Services;
using UnityEngine;

namespace Core.Scene
{
    public class LevelLoaderMono : MonoBehaviour
    {
        private LevelLoader _levelLoader;

        private void Start()
        {
            _levelLoader = ServiceLocator.GetService<LevelLoader>();
        }

        public void LoadLevel(GameLevelSO level)
        {
            _levelLoader.LoadLevel(level).Forget();
        }

        public void LoadLastBeatmapLevel()
        {
            _levelLoader.LoadLastBeatmapLevel();
        }
    }
}