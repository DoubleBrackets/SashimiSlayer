using Core.Scene;
using Cysharp.Threading.Tasks;
using Events;
using UnityEngine;

public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Listening Events")]

    [SerializeField]
    private VoidEvent _playerDeathEvent;

    [Header("Dependencies")]

    [SerializeField]
    private GameLevelSO _levelResultLevel;

    private void Awake()
    {
        _playerDeathEvent.AddListener(HandlePlayerDeath);
    }

    private void OnDestroy()
    {
        _playerDeathEvent.RemoveListener(HandlePlayerDeath);
    }

    private void HandlePlayerDeath()
    {
        OnDeath().Forget();
    }

    private async UniTaskVoid OnDeath()
    {
        await UniTask.Delay(1000);
        // LevelLoader.Instance.LoadLevel(_levelResultLevel).Forget();
    }
}