using System;
using Interactions.DataTypes;
using Interactions.Framework;
using UnityEngine;

namespace Interactions.Interactables
{
    public class CircleSliceable
    {
        public event Action OnHovered;
        public event Action OnUnhovered;
        public event Action OnSliced;

        private bool _hovered;
        private bool _used;

        private float _radius;
        private bool _singleUse;
        private SpaceType _spaceType;
        private Transform _hitCircleCenter;
        private IInteractable _sourceInteractable;

        public bool Enabled { get; set; } = true;

        public CircleSliceable(
            IInteractable sourceInteractable,
            float radius,
            bool singleUse,
            SpaceType spaceType,
            Transform hitCircleCenter)
        {
            _radius = radius;
            _singleUse = singleUse;
            _spaceType = spaceType;
            _hitCircleCenter = hitCircleCenter;
            _sourceInteractable = sourceInteractable;
        }

        public InteractionResult AttemptInteraction(InteractionAttempt attempt)
        {
            if (!Enabled || (_used && _singleUse))
            {
                return InteractionResult.Fail(_sourceInteractable);
            }

            Vector2 pos = CameraCanvasPositionToWorld(_hitCircleCenter.position);
            float dist = attempt.Aim.DistanceToSwordPlane(pos);
            bool inRadius = dist < _radius;

            if (attempt.Type == InteractionType.Hover)
            {
                bool shouldHover = inRadius;
                if (shouldHover != _hovered)
                {
                    if (shouldHover)
                    {
                        OnHovered?.Invoke();
                    }
                    else
                    {
                        OnUnhovered?.Invoke();
                    }

                    _hovered = shouldHover;
                }

                return InteractionResult.Fail(_sourceInteractable);
            }

            if (attempt.Type == InteractionType.Slice)
            {
                _used = true;

                if (inRadius)
                {
                    OnSliced?.Invoke();
                    return new InteractionResult
                    {
                        Successful = true,
                        Interactable = _sourceInteractable
                    };
                }
            }

            return InteractionResult.Fail(_sourceInteractable);
        }

        public Vector2 Position => _hitCircleCenter.position;

        private Vector2 CameraCanvasPositionToWorld(Vector2 cameraCanvasPosition)
        {
            if (_spaceType == SpaceType.CameraOverlay || _spaceType == SpaceType.Worldspace)
            {
                // Camera overlay canvases are really in world space
                // Since we're using orthographic we can just return the position directly
                return cameraCanvasPosition;
            }

            Camera camera = Camera.main;
            return camera.ScreenToWorldPoint(cameraCanvasPosition);
        }
    }

    public enum SpaceType
    {
        Worldspace,
        CameraOverlay,
        ScreenOverlay
    }
}