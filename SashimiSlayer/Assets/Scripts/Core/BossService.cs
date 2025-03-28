using Beatmapping;
using Beatmapping.Timing;
using Events;
using Events.Core;
using UnityEngine;

public class BossService : MonoBehaviour
{
    [SerializeField]
    private BeatmapEvent _startBeatmapEvent;

    [SerializeField]
    private FloatEvent _bossHealthEvent;

    [SerializeField]
    private FloatEvent _bossMaxHealthEvent;

    public static BossService Instance { get; private set; }

    private double _levelLength;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _startBeatmapEvent.AddListener(HandleStartBeatmap);
    }

    private void OnEnable()
    {
        BeatmapTimeManager.Instance.OnTick += TimeManager_OnTick;
    }

    private void OnDisable()
    {
        BeatmapTimeManager.Instance.OnTick -= TimeManager_OnTick;
    }

    private void OnDestroy()
    {
        _startBeatmapEvent.RemoveListener(HandleStartBeatmap);
    }

    private void TimeManager_OnTick(BeatmapTimeManager.TickInfo tickInfo)
    {
        var t = (float)tickInfo.BeatmapTime;
        _bossHealthEvent.Raise(t);
    }

    private void HandleStartBeatmap(BeatmapConfigSo beatmapConfigSo)
    {
        _levelLength = beatmapConfigSo.BeatmapTimeline.duration - beatmapConfigSo.StartTime;
        _bossMaxHealthEvent.Raise((float)_levelLength);
    }
}