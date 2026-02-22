using Cysharp.Threading.Tasks;
using Protag.Core;
using Protag.Presentation.Events;
using UnityEngine;

namespace Protag.Presentation
{
    /// <summary>
    ///     Handles the parry icon animation when the protag attempts a parry
    /// </summary>
    public class ProtagParryIcon : MonoBehaviour
    {
        [Header("Listening Events")]

        [SerializeField]
        private ProtagSwordStateEvent _tryBlockEvent;

        [SerializeField]
        private ProtagSwordStateEvent _successfulBlockEvent;

        [Header("Visuals")]

        [SerializeField]
        private SimpleAnimator[] _animators;

        private void Awake()
        {
            _tryBlockEvent.AddListener(HandleTryBlock);
            _successfulBlockEvent.AddListener(HandleSuccessBlock);
        }

        private void OnDestroy()
        {
            _tryBlockEvent.RemoveListener(HandleTryBlock);
            _successfulBlockEvent.RemoveListener(HandleSuccessBlock);
        }

        private void HandleSuccessBlock(Protaganist.ProtagSwordState swordState)
        {
            HideAll(swordState).Forget();
        }

        private async UniTaskVoid HideAll(Protaganist.ProtagSwordState swordState)
        {
            await UniTask.Yield();
            for (var i = 0; i < _animators.Length; i++)
            {
                _animators[i].Stop();
            }
        }

        private void HandleTryBlock(Protaganist.ProtagSwordState swordState)
        {
            var pose = (int)swordState.BlockPose;

            for (var i = 0; i < _animators.Length; i++)
            {
                if (i == pose)
                {
                    _animators[i].Play(true);
                }
            }
        }
    }
}