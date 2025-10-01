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

        [FormerlySerializedAs("trackRosterSO")]
        [FormerlySerializedAs("mapRosterSO")]
        [FormerlySerializedAs("_levelRosterSO")]
        [SerializeField]
        private SongRosterSO songRosterSO;

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
                foreach (GameLevelSO song in songRosterSO.Songs)
                {
                    if (song.NormalBeatmap == BeatmappingUtilities.CurrentEditingBeatmapConfig)
                    {
                        LevelLoader.Instance.SetDifficulty(LevelLoader.Difficulty.Normal);
                        LevelLoader.Instance.LoadLevel(song).Forget();
                        return;
                    }

                    if (song.HardBeatmap == BeatmappingUtilities.CurrentEditingBeatmapConfig)
                    {
                        LevelLoader.Instance.SetDifficulty(LevelLoader.Difficulty.Hard);
                        LevelLoader.Instance.LoadLevel(song).Forget();
                        return;
                    }
                }
            }

            LevelLoader.Instance.LoadLevel(_bootupConfigSO.InitialGameLevel).Forget();
        }
    }
}