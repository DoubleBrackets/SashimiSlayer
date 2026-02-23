using Events;
using UnityEngine;

namespace Scoring.Events
{
    [CreateAssetMenu(fileName = "BeatmapScoreEvent", menuName = "Events/BeatmapScoreEvent")]
    public class BeatmapScoreEvent : SOEvent<ScoringService.BeatmapScore>
    {
    }
}