using Core.Scene;
using UnityEngine;
using UnityEngine.Playables;

namespace Beatmapping.Tooling
{
    /// <summary>
    ///     Tool script that works with Utility Window to quickly select the desired timeline
    /// </summary>
    public class TimelineQuickSelect : MonoBehaviour
    {
        [SerializeField]
        private PlayableDirector _playableDirector;

        [SerializeField]
        private GameLevelSO _song;

        /// <summary>
        ///     Loads the map based on the difficulty selected in the LevelLoader. Intended for editor use
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public PlayableDirector LoadMap(LevelLoader.Difficulty difficulty)
        {
            if (_playableDirector == null)
            {
                Debug.LogError("PlayableDirector is not assigned in TimelineQuickSelect.");
                return null;
            }

            BeatmapConfigSo beatmap = _song.NormalBeatmap;

            if (difficulty == LevelLoader.Difficulty.Hard && _song.HardBeatmap != null)
            {
                beatmap = _song.HardBeatmap;
            }

            _playableDirector.playableAsset = beatmap.BeatmapTimeline;

            return _playableDirector;
        }
    }
}