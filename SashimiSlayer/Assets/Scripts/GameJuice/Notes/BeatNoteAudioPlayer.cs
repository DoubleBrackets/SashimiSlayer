using System.Collections.Generic;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;

namespace GameJuice.Notes
{
    public class BeatNoteAudioPlayer : BeatNoteModule
    {
        [SerializeField]
        private BeatNote _beatNote;

        [Header("Audio Events")]

        [SerializeField]
        private EventReference _startSound;

        public UnityEvent OnFirstInteractionTick;

        private void PlayStartSound(BeatNote.NoteTickInfo tickInfo)

        {
            OnFirstInteractionTick?.Invoke();
            if (!_startSound.IsNull && Application.isPlaying)
            {
                RuntimeManager.PlayOneShot(_startSound);
            }
        }

        public override IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            _beatNote.OnFirstInteractionTick += PlayStartSound;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            _beatNote.OnFirstInteractionTick -= PlayStartSound;
        }
    }
}