using System.Collections.Generic;
using Beatmapping.NoteBehaviors.Visuals;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.NoteBehaviors
{
    /// <summary>
    ///     Visual where the note falls into the sea from End to Cleanup
    /// </summary>
    public class FallIntoSeaEnd : BeatNoteModule
    {
        private const float Gravity = 9.8f;

        [BoldHeader("Fall Into Sea End")]
        [InfoBox("Behavior where the note falls into the sea from End to Cleanup")]
        [Header("Depends")]

        [SerializeField]
        private NoteVisualHandler _visuals;

        [SerializeField]
        private Transform _bodyTransform;

        private BeatNote _beatNote;

        private Vector2 _startPos;
        private Vector2 _endPos;
        private float _expectedFallTime;

        public override IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            beatNote.OnTick += BeatNote_OnTick;
            _startPos = beatNote.GetFinalInteractionPosition();
            _endPos = beatNote.EndPosition;

            if (_startPos.y > _endPos.y)
            {
                _expectedFallTime = Mathf.Sqrt(2 * (_startPos.y - _endPos.y) / Gravity);
            }
            else
            {
                _expectedFallTime = 1;
            }
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            beatNote.OnTick -= BeatNote_OnTick;
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            BeatNote.NoteTimeSegment segment = tickinfo.NoteSegment;

            if (segment.Type == BeatNote.TimeSegmentType.Ending)
            {
                FallDownVisuals(tickinfo);
            }
        }

        private void FallDownVisuals(BeatNote.NoteTickInfo noteTickInfo)
        {
            double time = noteTickInfo.SegmentTime;

            float startY = _startPos.y;

            Vector2 pos = _bodyTransform.position;

            pos.y = startY - (float)(Gravity * time * time / 2);

            _bodyTransform.position = pos;

            var normalizedTime = (float)(time / _expectedFallTime);

            _visuals.SetRotation(normalizedTime * 90f);
            _visuals.SetSpriteAlpha(0.5f);
        }
    }
}