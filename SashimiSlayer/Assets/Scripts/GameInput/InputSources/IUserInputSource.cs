using System;
using CommonTypes;
using GameInput.Haptics;

namespace GameInput.InputSources
{
    /// <summary>
    ///     Source of raw gameplay input state
    /// </summary>
    public interface IUserInputSource
    {
        public event Action<BlockPoses> OnBlockPoseChanged;
        public event Action<SheathState> OnSheathStateChanged;
        public event Action OnToggleMenuInput;

        /// <summary>
        ///     Get the current sword angle in degrees
        /// </summary>
        /// <returns></returns>
        public float GetSwordAngle();

        /// <summary>
        ///     Get the current sheath state
        /// </summary>
        /// <returns></returns>
        public SheathState GetSheathState();

        /// <summary>
        ///     Get the current block pose
        /// </summary>
        /// <returns></returns>
        public BlockPoses GetBlockPose();

        public void AddRumble(RumbleFeedbackSO rumbleFeedback);
    }
}