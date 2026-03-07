using System.Collections.Generic;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using UnityEngine;

namespace GameJuice.Notes
{
    public class BeatNoteParticles : BeatNoteModule
    {
        [Header("Audio Events")]

        [SerializeField]
        private ParticleSystem _startParticle;

        private void PlayStartSound(BeatNote.NoteTickInfo tickInfo)

        {
            if (_startParticle && Application.isPlaying)
            {
                _startParticle.Play();
            }
        }

        public override IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            beatNote.OnFirstInteractionTick += PlayStartSound;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            beatNote.OnFirstInteractionTick -= PlayStartSound;
        }
    }
}