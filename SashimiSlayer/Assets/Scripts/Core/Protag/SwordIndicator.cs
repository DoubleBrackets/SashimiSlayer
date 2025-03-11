using System.Collections.Generic;
using Events;
using Events.Core;
using UnityEngine;

namespace Core.Protag
{
    public class SwordIndicator : MonoBehaviour
    {
        [Header("Visuals")]

        [SerializeField]
        private LineRenderer _sheathedLineRen;

        [SerializeField]
        private LineRenderer _unsheathedLineRen;

        [SerializeField]
        private ParticleSystem[] _sliceParticles;

        [SerializeField]
        private GameObject _distortionPrefab;

        [Header("Events")]

        [SerializeField]
        private SOEvent _protagSuccessfulSliceEvent;

        [SerializeField]
        private ProtagSwordStateEvent _onSwordStateChange;

        [SerializeField]
        private Vector2Event _swordPivotPositionChangeEvent;

        public float Angle { get; private set; }

        private readonly List<ParticleSystem.MinMaxCurve> _initialParticleRot = new();

        private Vector3 _position;

        private Vector3 _currentSwordPivot;

        private void Awake()
        {
            _onSwordStateChange.AddListener(OnSwordStateChange);
            SetSheatheState(SharedTypes.SheathState.Sheathed);
            _protagSuccessfulSliceEvent.AddListener(OnSuccessfulSlice);
            _swordPivotPositionChangeEvent.AddListener(SetPosition);

            foreach (ParticleSystem particle in _sliceParticles)
            {
                _initialParticleRot.Add(particle.main.startRotation);
            }
        }

        private void Update()
        {
            _currentSwordPivot = _position;
            UpdateOrientation(_sheathedLineRen);
            UpdateOrientation(_unsheathedLineRen);
        }

        private void OnDestroy()
        {
            _onSwordStateChange.RemoveListener(OnSwordStateChange);
            _protagSuccessfulSliceEvent.RemoveListener(OnSuccessfulSlice);
            _swordPivotPositionChangeEvent.RemoveListener(SetPosition);
        }

        private void OnSuccessfulSlice()
        {
            for (var i = 0; i < _sliceParticles.Length; i++)
            {
                ParticleSystem particle = _sliceParticles[i];
                // particle.transform.position = _cPos;
                ParticleSystem.MinMaxCurve curve = _initialParticleRot[i];
                curve.constantMin += -Angle * Mathf.Deg2Rad;
                curve.constantMax += -Angle * Mathf.Deg2Rad;

                ParticleSystem.MainModule main = particle.main;
                main.startRotation = curve;

                particle.transform.rotation = Quaternion.Euler(0, 0, Angle);
                particle.Play();
            }

            Instantiate(_distortionPrefab, _currentSwordPivot, Quaternion.Euler(0, 0, Angle));
        }

        private void OnSwordStateChange(Protaganist.ProtagSwordState swordState)
        {
            SetSheatheState(swordState.SheathState);
            SetAngle(swordState.SwordAngle);
            SetPosition(swordState.SwordPosition);
        }

        private void SetSheatheState(SharedTypes.SheathState state)
        {
            _sheathedLineRen.enabled = state == SharedTypes.SheathState.Sheathed;
            _unsheathedLineRen.enabled = state == SharedTypes.SheathState.Unsheathed;
        }

        private void SetAngle(float angle)
        {
            Angle = angle;
        }

        private void SetPosition(Vector2 position)
        {
            _position = position;
        }

        private void UpdateOrientation(LineRenderer lineRen)
        {
            Quaternion rotation = Quaternion.Euler(0, 0, Angle);
            lineRen.positionCount = 3;
            lineRen.SetPosition(0, _currentSwordPivot + rotation * Vector3.left * 25f);
            lineRen.SetPosition(1, _currentSwordPivot);
            lineRen.SetPosition(2, _currentSwordPivot + rotation * Vector3.right * 25f);
        }
    }
}