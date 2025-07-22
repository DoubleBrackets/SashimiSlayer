using System;
using UnityEngine;

namespace GameInput
{
    public abstract class BaseUserInputProvider : MonoBehaviour
    {
        public abstract event Action<SharedTypes.BlockPoseStates> OnBlockPoseChanged;

        public abstract event Action<SharedTypes.SheathState> OnSheathStateChanged;

        /// <summary>
        ///     Get the sword angle in degrees
        /// </summary>
        /// <returns></returns>
        public abstract float GetSwordAngle();

        public abstract SharedTypes.SheathState GetSheathState();

        public abstract SharedTypes.BlockPoseStates GetBlockPose();
    }
}