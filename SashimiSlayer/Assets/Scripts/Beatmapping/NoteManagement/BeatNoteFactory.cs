using System.Collections.Generic;
using Beatmapping.Notes;
using Interactions.Framework;
using UnityEngine;

namespace Beatmapping.NoteManagement
{
    public class BeatNoteFactory
    {
        private Transform _parentTransform;
        private IInteractionService _interactionService;

        public BeatNoteFactory(Transform parentTransform, IInteractionService interactionService)
        {
            _parentTransform = parentTransform;
            _interactionService = interactionService;
        }

        public BeatNote CreateBeatNote(
            BeatNoteTypeSO beatNoteType,
            BeatNoteData data,
            List<NoteInteraction.NoteInteraction> interactions,
            double initializeTime)
        {
            BeatNote note = GameObject.Instantiate(beatNoteType.Prefab, _parentTransform);
            note.Initialize(
                interactions,
                data.StartPosition,
                data.EndPosition,
                data.NoteStartTime,
                data.NoteEndTime,
                initializeTime,
                beatNoteType.HitboxRadius,
                _interactionService
            );
            return note;
        }
    }
}