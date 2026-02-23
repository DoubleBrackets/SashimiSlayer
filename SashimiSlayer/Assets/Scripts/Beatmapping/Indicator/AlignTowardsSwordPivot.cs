using EditorUtils.BoldHeader;
using Protag.SharedData;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Script that aligns a visual container towards the player position
    /// </summary>
    public class AlignTowardsSwordPivot : MonoBehaviour
    {
        [BoldHeader("Align Towards Player")]
        [Header("Depends")]

        [SerializeField]
        private Transform _visualContainer;

        [Header("Config")]

        [SerializeField]
        private float _rotationOffset;

        private void Update()
        {
            if (_visualContainer == null)
            {
                return;
            }

            Vector3 swordPos = ProtagGlobalData.ProtagSwordPivot;
            Vector3 vectorFromPlayer = _visualContainer.position - swordPos;

            _visualContainer.transform.right = vectorFromPlayer.normalized;
        }
    }
}