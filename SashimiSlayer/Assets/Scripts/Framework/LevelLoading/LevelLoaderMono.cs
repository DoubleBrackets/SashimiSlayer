using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Framework.LevelLoading
{
    public class LevelLoaderMono : MonoBehaviour
    {
        private LevelService _levelService;

        private void Start()
        {
            _levelService = ServiceLocator.GetService<LevelService>();
        }

        public void LoadLevel(GameLevelSO level)
        {
            _levelService.LoadLevel(level).Forget();
        }

        public void LoadLastBeatmapLevel()
        {
            _levelService.LoadLastBeatmapLevel();
        }
    }
}