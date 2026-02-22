using System.Collections.Generic;
using Beatmapping.NoteBehaviors.Visuals;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using EditorUtils.BoldHeader;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Beatmapping.NoteBehaviors
{
    public class HitAndBounceAwayNote : BeatNoteModule
    {
        [BoldHeader("Hit and Bounce Away Note")]
        [InfoBox("Note behavior that involves moving from impact/parry point -> bouncing away offscreen")]
        [Header("Depends")]

        [SerializeField]
        private BeatNote _beatNote;

        [FormerlySerializedAs("_visuals")]
        [SerializeField]
        private NoteVisualHandler visualHandler;

        [SerializeField]
        private Transform _bodyTransform;

        [Header("Events")]

        public UnityEvent OnBounce;

        private Vector2 _endPos;
        private Vector2 _startPos;

        private bool _started;

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickInfo)
        {
            BeatNote.NoteTimeSegment segment = tickInfo.NoteSegment;

            if (segment.Type != BeatNote.TimeSegmentType.PreEnding)
            {
                visualHandler.SetSpriteAlpha(1f);
                _started = false;
                return;
            }

            if (!_started)
            {
                _started = true;
                OnBounce.Invoke();
            }

            _bodyTransform.position = Vector2.Lerp(_startPos, _endPos, (float)tickInfo.NormalizedSegmentTime);

            visualHandler.SetRotation(1200 * (float)tickInfo.SegmentTime);
            visualHandler.SetSpriteAlpha(0.7f);
        }

        public override IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _startPos = beatNote.GetFinalInteractionPosition();
            _endPos = beatNote.EndPosition;
            _started = false;

            _beatNote.OnTick += BeatNote_OnTick;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnTick -= BeatNote_OnTick;
        }
    }
}