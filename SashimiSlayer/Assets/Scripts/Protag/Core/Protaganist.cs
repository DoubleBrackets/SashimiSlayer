using System.Collections.Generic;
using System.Linq;
using Base;
using CommonTypes;
using Core.Protag.Core;
using EditorUtils.BoldHeader;
using Events.Basic;
using Interactions.Framework;
using NaughtyAttributes;
using Protag.Presentation.Events;
using Protag.Presentation.Slicing;
using UnityEngine;

namespace Protag.Core
{
    /// <summary>
    ///     Protaganist, placed into the scene by hand, needs global state
    /// </summary>
    public class Protaganist : DescMono
    {
        public struct ProtagSwordState
        {
            public BlockPoses BlockPose;
            public SheathState SheathState;
            public Vector3 SwordPosition;
            public float SwordAngle;
        }

        [BoldHeader("Presentation Events (Out)")]
        [InfoBox(
            "These events are intended to be used by presentation-related components that are not tied to gameplay logic")]
        [SerializeField]
        private VoidEvent _damageTakenEvent;

        [Header("Blocking")]

        [SerializeField]
        private ProtagSwordStateEvent _tryBlock;

        [SerializeField]
        private ProtagSwordStateEvent _blockSuccessEvent;

        [SerializeField]
        private ProtagSwordStateEvent _blockedRight;

        [SerializeField]
        private ProtagSwordStateEvent _blockedLeft;

        [Header("Slicing")]

        [SerializeField]
        private ProtagSwordStateEvent _trySlice;

        [SerializeField]
        private ProtagSwordStateEvent _successfulSliceEvent;

        [SerializeField]
        private ProtagSwordStateEvent _earlySliceEvent;

        [SerializeField]
        private ProtagSwordStateEvent _lateSliceEvent;

        [SerializeField]
        private ProtagSwordStateEvent _perfectSliceEvent;

        [SerializeField]
        private ProtagSwordStateEvent _swordAngleChanged;

        [SerializeField]
        private ProtagSwordStateEvent _swordSheathed;

        [SerializeField]
        private ObjectsSlicedEvent _objectsSliced;

        [Header("Events (In)")]

        [SerializeField]
        private VoidEvent _onDrawDebugGuiEvent;

        /// <summary>
        ///     The position that notes move towards
        /// </summary>
        public Vector3 NoteTargetPosition { get; set; }

        public float SwordAngle => _currentSwordState.SwordAngle;

        private ProtagSwordState _currentSwordState;

        private void Awake()
        {
            _onDrawDebugGuiEvent.AddListener(HandleDrawDebugGUI);
        }

        private void OnDestroy()
        {
            _onDrawDebugGuiEvent.RemoveListener(HandleDrawDebugGUI);
        }

        private void HandleDrawDebugGUI()
        {
            GUILayout.Label($"Sword angle: {_currentSwordState.SwordAngle}");
            GUILayout.Label($"Sword position: {_currentSwordState.SwordPosition}");
            GUILayout.Label($"Sheath state: {_currentSwordState.SheathState}");
        }

        public void SetSheatheState(SheathState newState, out bool doSlice)
        {
            SheathState oldState = _currentSwordState.SheathState;
            _currentSwordState.SheathState = newState;

            if (newState == SheathState.Unsheathed && oldState == SheathState.Sheathed)
            {
                doSlice = true;
                _trySlice.Raise(_currentSwordState);
                return;
            }

            if (newState == SheathState.Sheathed && oldState == SheathState.Unsheathed)
            {
                doSlice = false;
                _swordSheathed.Raise(_currentSwordState);
                return;
            }

            doSlice = false;
        }

        public void SetSwordAngle(float angle, out bool changed)
        {
            changed = !Mathf.Approximately(_currentSwordState.SwordAngle, angle);

            if (!changed)
            {
                return;
            }

            _currentSwordState.SwordAngle = angle;
            _swordAngleChanged.Raise(_currentSwordState);
        }

        public void SetBlockPose(BlockPoses blockPose)
        {
            _currentSwordState.BlockPose = blockPose;
            _tryBlock.Raise(_currentSwordState);
        }

        public void DoSlicePresentation(List<IInteractable> finalInteractedWith, SlicePresentationTypes style)
        {
            var slicedObjectList = new ProtagSlicedObjectList
            {
                SlicedObjects = finalInteractedWith.Select(i => new ProtagSlicedObject
                {
                    Angle = _currentSwordState.SwordAngle,
                    Position = i.Position
                }).ToList()
            };

            _objectsSliced.Raise(slicedObjectList);

            if (style != SlicePresentationTypes.Miss)
            {
                _successfulSliceEvent.Raise(_currentSwordState);
            }

            switch (style)
            {
                case SlicePresentationTypes.Early:
                    _earlySliceEvent.Raise(_currentSwordState);
                    break;
                case SlicePresentationTypes.Late:
                    _lateSliceEvent.Raise(_currentSwordState);
                    break;
                case SlicePresentationTypes.Perfect:
                    _perfectSliceEvent.Raise(_currentSwordState);
                    break;
            }
        }

        public void DoBlockPresentation(BlockPresentationTypes result, BlockPoses pose)
        {
            if (result != BlockPresentationTypes.Miss)
            {
                switch (pose)
                {
                    case BlockPoses.BlockShell:
                        _blockedRight.Raise(_currentSwordState);
                        break;
                    case BlockPoses.BlockStar:
                        _blockedLeft.Raise(_currentSwordState);
                        break;
                }

                _blockSuccessEvent.Raise(_currentSwordState);
            }
        }

        public void DoDamagedPresentation()
        {
            _damageTakenEvent.Raise();
        }
    }
}