using Core.Scene;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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

        public TimelineAsset CurrentTimeline => _playableDirector?.playableAsset as TimelineAsset;

        /// <summary>
        ///     Loads the map based on the difficulty selected in the LevelLoader. Intended for editor use
        /// </summary>
        /// <param name="difficulty"></param>
        /// <returns></returns>
        public PlayableDirector LoadMap(LevelLoader.Difficulty difficulty)
        {
            if (_song == null)
            {
                Debug.LogWarning("Song is not assigned in TimelineQuickSelect.");
                return null;
            }

            if (_playableDirector == null)
            {
                Debug.LogWarning("PlayableDirector is not assigned in TimelineQuickSelect.");
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