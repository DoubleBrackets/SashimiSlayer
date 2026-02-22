using CommonTypes;
using FMOD.Studio;
using FMODUnity;
using Protag.Core;
using Protag.Presentation.Events;
using UnityEngine;

namespace Protag.Presentation
{
    public class ProtagSFX : MonoBehaviour
    {
        private const string BlockPoseParamName = "BlockPose";

        [Header("Listening Events")]

        [SerializeField]
        private ProtagSwordStateEvent _tryBlockEvent;

        [SerializeField]
        private ProtagSwordStateEvent _successfulBlockEvent;

        [Header("SFX")]

        [SerializeField]
        private EventReference _successfulBlockSfx;

        [SerializeField]
        private EventReference _tryBlockSfx;

        private void Awake()
        {
            _tryBlockEvent.AddListener(HandleTryBlock);
            _successfulBlockEvent.AddListener(HandleSuccessfulBlock);
        }

        private void OnDestroy()
        {
            _tryBlockEvent.RemoveListener(HandleTryBlock);
            _successfulBlockEvent.RemoveListener(HandleSuccessfulBlock);
        }

        private void HandleTryBlock(Protaganist.ProtagSwordState swordState)
        {
            BlockPoseSfx(_tryBlockSfx, swordState.BlockPose);
        }

        private void HandleSuccessfulBlock(Protaganist.ProtagSwordState swordState)
        {
            BlockPoseSfx(_successfulBlockSfx, swordState.BlockPose);
        }

        private void BlockPoseSfx(EventReference sfx, BlockPoses blockPose)
        {
            EventInstance instance = RuntimeManager.CreateInstance(sfx);

            instance.setParameterByName(BlockPoseParamName, (int)blockPose);

            instance.start();

            instance.release();
        }
    }
}