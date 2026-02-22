using System.Collections.Generic;
using CommonTypes;
using Events.Basic;
using Protag.Core;
using Protag.Presentation.Events;
using UnityEngine;

namespace Protag.Presentation
{
    /// <summary>
    ///     Handles visual effects related to the sword's "cutting" line
    ///     Needs special treatment because of rotations
    /// </summary>
    public class SwordCuttingLinePresentation : MonoBehaviour
    {
        [Header("Depends")]

        [SerializeField]
        private LineRenderer _sheathedLineRen;

        [SerializeField]
        private LineRenderer _unsheathedLineRen;

        [SerializeField]
        private GameObject _distortionPrefab;

        [SerializeField]
        private List<ParticleSystem> _allSliceParticles;

        [Header("Events (In)")]

        [SerializeField]
        private ProtagSwordStateEvent _onSwordRotated;

        [SerializeField]
        private ProtagSwordStateEvent _onSwordSheathed;

        [SerializeField]
        private ProtagSwordStateEvent _onSwordUnsheathed;

        [SerializeField]
        private ProtagSwordStateEvent _onCreateDistortionEvent;

        [Header("Channels (In)")]

        [SerializeField]
        private Vector2Event _swordPivotPositionChangeEvent;

        private float _swordAngle;

        private readonly List<ParticleSystem.MinMaxCurve> _initialParticleRot = new();

        private Vector3 _currentSwordPivot;

        private void Awake()
        {
            _onSwordRotated.AddListener(HandleSwordRotated);
            _onSwordSheathed.AddListener(OnSwordSheathed);
            _onSwordUnsheathed.AddListener(OnSwordUnsheathed);

            _onCreateDistortionEvent.AddListener(CreateDistortion);
            _swordPivotPositionChangeEvent.AddListener(SetPosition);

            SetSheatheState(SheathState.Sheathed);
            CacheInitialParticleRotations(_allSliceParticles);
        }

        private void OnDestroy()
        {
            _onSwordRotated.RemoveListener(HandleSwordRotated);
            _onSwordSheathed.RemoveListener(OnSwordSheathed);
            _onSwordUnsheathed.RemoveListener(OnSwordUnsheathed);

            _onCreateDistortionEvent.RemoveListener(CreateDistortion);
            _swordPivotPositionChangeEvent.RemoveListener(SetPosition);
        }

        private void HandleSwordRotated(Protaganist.ProtagSwordState swordState)
        {
            _swordAngle = swordState.SwordAngle;
            UpdatePresentation();
        }

        private void CacheInitialParticleRotations(IReadOnlyList<ParticleSystem> particles)
        {
            foreach (ParticleSystem particle in particles)
            {
                _initialParticleRot.Add(particle.main.startRotation);
            }
        }

        /// <summary>
        ///     Rotate the slice particles based on the current sword angle.
        /// </summary>
        private void RotateSliceParticles(float angle)
        {
            for (var i = 0; i < _allSliceParticles.Count; i++)
            {
                ParticleSystem particle = _allSliceParticles[i];
                // particle.transform.position = _cPos;
                ParticleSystem.MinMaxCurve curve = _initialParticleRot[i];
                curve.constantMin += -angle * Mathf.Deg2Rad;
                curve.constantMax += -angle * Mathf.Deg2Rad;

                ParticleSystem.MainModule main = particle.main;
                main.startRotation = curve;

                particle.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        private void CreateDistortion()
        {
            Instantiate(_distortionPrefab, _currentSwordPivot, Quaternion.Euler(0, 0, _swordAngle));
        }

        private void OnSwordUnsheathed(Protaganist.ProtagSwordState state)
        {
            SetSheatheState(SheathState.Unsheathed);
        }

        private void OnSwordSheathed(Protaganist.ProtagSwordState state)
        {
            SetSheatheState(SheathState.Sheathed);
        }

        private void SetSheatheState(SheathState state)
        {
            _sheathedLineRen.enabled = state == SheathState.Sheathed;
            _unsheathedLineRen.enabled = state == SheathState.Unsheathed;
        }

        private void SetPosition(Vector2 position)
        {
            _currentSwordPivot = position;
            UpdatePresentation();
        }

        private void UpdatePresentation()
        {
            UpdateLineRenOrientation(_sheathedLineRen, _swordAngle, _currentSwordPivot);
            UpdateLineRenOrientation(_unsheathedLineRen, _swordAngle, _currentSwordPivot);
            RotateSliceParticles(_swordAngle);
        }

        private void UpdateLineRenOrientation(LineRenderer lineRen, float angle, Vector3 pivot)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, _swordAngle);
            lineRen.positionCount = 3;
            lineRen.SetPosition(0, pivot + rotation * Vector3.left * 25f);
            lineRen.SetPosition(1, pivot);
            lineRen.SetPosition(2, pivot + rotation * Vector3.right * 25f);
        }
    }
}