using Events;
using UnityEngine;

namespace Beatmapping.Events
{
    [CreateAssetMenu(menuName = "Events/Beatmap/BeatmapEvent")]
    public class BeatmapEvent : SOEvent<BeatmapConfigSo>
    {
    }
}