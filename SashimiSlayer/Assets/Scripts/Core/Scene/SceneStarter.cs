using Beatmapping.Tooling;
using Cysharp.Threading.Tasks;
using EditorUtils.BoldHeader;
using FMODUnity;
using Menus.LevelSelect;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Core.Scene
{
    /// <summary>
    ///     Bootstrapping script inside base scene to startup and load initial resources.
    /// </summary>
    public class SceneStarter : MonoBehaviour
    {
        [BoldHeader("Scene Bootup")]
        [InfoBox("Handles loading initial loading and entering starting scene")]
        [Header("Depends")]

        [SerializeField]
        private BootupConfigSO _bootupConfigSO;

        [FormerlySerializedAs("mapRosterSO")]
        [FormerlySerializedAs("_levelRosterSO")]
        [SerializeField]
        private TrackRosterSO trackRosterSO;

        [SerializeField]
        private StudioBankLoader _studioBankLoader;

        private void Start()
        {
            Startup().Forget();
        }

        private void OnDestroy()
        {
            _studioBankLoader.Unload();
        }

        private async UniTaskVoid Startup()
        {
            Debug.Log("Loading starting banks");
            _studioBankLoader.Load();

            await UniTask.WaitUntil(() => RuntimeManager.HaveAllBanksLoaded);
            await UniTask.WaitUntil(() => !RuntimeManager.AnySampleDataLoading());

            Debug.Log("All banks loaded");

            // Load startup level
            if (BeatmappingUtilities.PlayFromEditedBeatmap)
            {
                foreach (TrackRosterSO.TrackEntry track in trackRosterSO.Tracks)
                {
                    if (track.NormalMap.Beatmap == BeatmappingUtilities.CurrentEditingBeatmapConfig)
                    {
                        LevelLoader.Instance.LoadLevel(track.NormalMap).Forget();
                        return;
                    }

                    if (track.HardMap.Beatmap == BeatmappingUtilities.CurrentEditingBeatmapConfig)
                    {
                        LevelLoader.Instance.LoadLevel(track.HardMap).Forget();
                        return;
                    }
                }
            }

            LevelLoader.Instance.LoadLevel(_bootupConfigSO.InitialGameLevel).Forget();
        }
    }
}