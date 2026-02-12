using Core.Protag;
using Events.Core;
using FMOD.Studio;
using FMODUnity;
using GameInput.Interface;
using UnityEngine;

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

    private void BlockPoseSfx(EventReference sfx, BlockPoseStates blockPose)
    {
        EventInstance instance = RuntimeManager.CreateInstance(sfx);

        instance.setParameterByName(BlockPoseParamName, (int)blockPose);

        instance.start();

        instance.release();
    }
}