using System.Collections.Generic;
using Beatmapping;
using Beatmapping.Interactions;
using Events;
using Events.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Core.Protag
{
    public class SwordIndicator : MonoBehaviour
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
        private NoteInteractionFinalResultEvent _noteFinalResultEvent;

        [SerializeField]
        private SliceResultEvent _sliceResultEvent;

        [SerializeField]
        private ProtagSwordStateEvent _onSwordStateChange;

        [SerializeField]
        private Vector2Event _swordPivotPositionChangeEvent;

        [Header("VFX Events")]

        [SerializeField]
        private UnityEvent _onUISliceVFX;

        [SerializeField]
        private UnityEvent _onEarlySliceVFX;

        [SerializeField]
        private UnityEvent _onPerfectSliceVFX;

        [SerializeField]
        private UnityEvent _onLateSliceVFX;

        public float Angle { get; private set; }

        private readonly List<ParticleSystem.MinMaxCurve> _initialParticleRot = new();

        private int _prevSliceFrame;

        private Vector3 _position;

        private Vector3 _currentSwordPivot;

        private void Awake()
        {
            _onSwordStateChange.AddListener(OnSwordStateChange);
            SetSheatheState(SharedTypes.SheathState.Sheathed);
            _noteFinalResultEvent.AddListener(PlayNoteSliceVFX);
            _sliceResultEvent.AddListener(PlayUISliceVFX);
            _swordPivotPositionChangeEvent.AddListener(SetPosition);

            AddToParticleRotations(_allSliceParticles);
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
            _noteFinalResultEvent.RemoveListener(PlayNoteSliceVFX);
            _sliceResultEvent.RemoveListener(PlayUISliceVFX);
            _swordPivotPositionChangeEvent.RemoveListener(SetPosition);
        }

        private void PlayUISliceVFX(SliceResultData data)
        {
            if (data.SlicedObjectType != SliceResultData.SlicedObject.MenuItem)
            {
                return;
            }

            RotateSliceParticles();
            _onUISliceVFX.Invoke();
            CreateDistortion();
        }

        private void AddToParticleRotations(IReadOnlyList<ParticleSystem> particles)
        {
            foreach (ParticleSystem particle in particles)
            {
                _initialParticleRot.Add(particle.main.startRotation);
            }
        }

        public void PlayNoteSliceVFX(NoteInteraction.FinalResult result)
        {
            if (!result.Successful || result.InteractionType != NoteInteraction.InteractionType.Slice)
            {
                return;
            }

            // Hack fix to prevent multiple VFX from the same slice, since interactions are called for each note
            if (Time.frameCount == _prevSliceFrame)
            {
                return;
            }

            _prevSliceFrame = Time.frameCount;

            RotateSliceParticles();

            TimingWindow.TimingResult timing = result.TimingResult;
            if (timing.IsEarly())
            {
                _onEarlySliceVFX.Invoke();
            }
            else if (timing.IsPerfect())
            {
                _onPerfectSliceVFX.Invoke();
            }
            else if (timing.IsLate())
            {
                _onLateSliceVFX.Invoke();
            }

            CreateDistortion();
        }

        /// <summary>
        ///     Rotate the slice particles based on the current sword angle.
        /// </summary>
        private void RotateSliceParticles()
        {
            for (var i = 0; i < _allSliceParticles.Count; i++)
            {
                ParticleSystem particle = _allSliceParticles[i];
                // particle.transform.position = _cPos;
                ParticleSystem.MinMaxCurve curve = _initialParticleRot[i];
                curve.constantMin += -Angle * Mathf.Deg2Rad;
                curve.constantMax += -Angle * Mathf.Deg2Rad;

                ParticleSystem.MainModule main = particle.main;
                main.startRotation = curve;

                particle.transform.rotation = Quaternion.Euler(0, 0, Angle);
            }
        }

        private void CreateDistortion()
        {
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