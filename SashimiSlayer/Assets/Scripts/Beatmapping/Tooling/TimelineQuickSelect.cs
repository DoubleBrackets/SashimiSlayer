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
        ///     Loads the mapselected in the LevelLoader. Intended for editor use
        /// </summary>
        /// <returns></returns>
        public PlayableDirector LoadMap()
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

            _playableDirector.playableAsset = beatmap.BeatmapTimeline;

            return _playableDirector;
        }
    }
}