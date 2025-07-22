using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.NoteBehaviors.Visuals;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using Core.Protag;
using EditorUtils.BoldHeader;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beatmapping.NoteBehaviors
{
    /// <summary>
    ///     Attack Note that moves in three stages
    ///     Spawn -> Attack ready position
    ///     Attack ready position -> Player (Block interaction)
    /// </summary>
    public class TwoPointIncomingAttack : BeatNoteModule
    {
        [BoldHeader("Two Point Attack Behavior")]
        [InfoBox("Note behavior that involves moving from start -> peak -> attacking Protag")]
        [Header("Depends")]

        [SerializeField]
        private BeatNote _beatNote;

        [SerializeField]
        private Transform _bodyTransform;

        [FormerlySerializedAs("_noteVisual")]
        [SerializeField]
        private NoteVisualHandler noteVisualHandler;

        [Header("Config")]

        [SerializeField]
        private int _interactionIndex;

        [SerializeField]
        private float _distanceFromTarget;

        [Header("Visuals")]

        [SerializeField]
        private AnimationCurve _moveCurve;

        [SerializeField]
        [Range(0, 1)]
        private float _middlePointThreshold;

        [Header("Audio")]

        [SerializeField]
        private EventReference _peakSfx;

        [Header("Events")]

        public UnityEvent OnHitPeak;

        public UnityEvent OnHitTarget;

        private Vector2 _startPos;
        private Vector2 _peakPos;
        private Vector2 _targetPos;
        private float _angleToTarget;

        private bool _hitPeak;

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(_bodyTransform.position, _distanceFromTarget);
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (tickinfo.InteractionIndex != _interactionIndex)
            {
                return;
            }

            WaitingForAttackVisual((float)tickinfo.NormalizedSegmentTime, tickinfo.Flags);
        }

        private void BeatNote_ProtagFailBlock(BeatNote.NoteTickInfo tickInfo,
            NoteInteraction.FinalResult finalResult)
        {
            OnHitTarget.Invoke();
        }

        private void WaitingForAttackVisual(float normalizedTime, BeatNote.TickFlags flags)
        {
            // Lerp from start pos to peak pos, then to target pos

            float trajectoryPeakTime = _middlePointThreshold;
            if (normalizedTime <= trajectoryPeakTime)
            {
                float t = _moveCurve.Evaluate(normalizedTime / trajectoryPeakTime);
                _bodyTransform.position = new Vector2(
                    Mathf.Lerp(_startPos.x, _peakPos.x, normalizedTime / trajectoryPeakTime),
                    Mathf.Lerp(_startPos.y, _peakPos.y, t)
                );
                noteVisualHandler.SetRotation(-90 * (1 - t));
                _hitPeak = false;
            }
            else
            {
                if (flags.HasFlag(BeatNote.TickFlags.TriggerInteractions)
                    && !_hitPeak
                    && normalizedTime >= trajectoryPeakTime)
                {
                    _hitPeak = true;
                    OnHitPeak.Invoke();
                    if (!_peakSfx.IsNull)
                    {
                        RuntimeManager.PlayOneShot(_peakSfx);
                    }
                }

                float remappedTime = (normalizedTime - trajectoryPeakTime) / (1 - trajectoryPeakTime);

                _bodyTransform.position = new Vector2(
                    Mathf.Lerp(_peakPos.x, _targetPos.x, remappedTime),
                    Mathf.Lerp(_peakPos.y, _targetPos.y, remappedTime)
                );

                // angle towards target
                noteVisualHandler.SetRotation(180 + _angleToTarget);
            }

            // Make sure sprite is no longer transparent, to prevent ghosts when looping
            noteVisualHandler.SetSpriteAlpha(1);
            noteVisualHandler.SetVisible(true);
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return new List<IInteractionUser.InteractionUsage>
            {
                new(NoteInteraction.InteractionType.Block, _interactionIndex, 2)
            };
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            // Form an arc from start, with the peak at the target position
            _startPos = _beatNote.GetPreviousPosition(_interactionIndex);
            _peakPos = _beatNote.GetInteractionPosition(_interactionIndex, 0);

            _targetPos = GetTargetPositionFromProtag();

            // Override the target position on the BeatNote so following interactions use the right position
            _beatNote.SetInteractionPosition(_interactionIndex, 1, _targetPos);

            // _targetPos = _beatNote.GetInteractionPosition(_interactionIndex, 1);
            _bodyTransform.position = _startPos;

            _angleToTarget = Mathf.Atan2(_targetPos.y - _peakPos.y, _targetPos.x - _peakPos.x) * Mathf.Rad2Deg;

            _beatNote.OnTick += BeatNote_OnTick;
            _beatNote.OnProtagFailBlock += BeatNote_ProtagFailBlock;
        }

        private Vector2 GetTargetPositionFromProtag()
        {
            Vector2 rawTargetPos = Vector2.zero;
            if (Protaganist.Instance != null)
            {
                rawTargetPos = Protaganist.Instance.SwordPosition;
            }
            else
            {
                var body = FindObjectOfType<ProtagBody>();
                if (body != null)
                {
                    rawTargetPos = body.TargetPosition;
                }
            }

            Vector2 dir = (_peakPos - rawTargetPos).normalized;
            return rawTargetPos + dir * _distanceFromTarget;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
            _beatNote.OnProtagFailBlock -= BeatNote_ProtagFailBlock;
        }
    }
}