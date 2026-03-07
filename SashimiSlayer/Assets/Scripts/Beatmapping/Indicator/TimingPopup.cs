using System.Collections.Generic;
using Beatmapping.NoteInteraction.DataTypes;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using UnityEngine;

namespace Beatmapping.Indicator
{
    /// <summary>
    ///     Beat note listener that displays the timing popup on interaction (e.g 'Early' or 'Perfect')
    /// </summary>
    public class TimingPopup : BeatNoteModule
    {
        [SerializeField]
        private ParticleSystem _earlyParticles;

        [SerializeField]
        private ParticleSystem _perfectParticles;

        [SerializeField]
        private ParticleSystem _lateParticles;

        [SerializeField]
        private ParticleSystem _missParticles;

        [SerializeField]
        private ParticleSystem[] _perfectSliceParticles;

        public override IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            beatNote.OnInteractionFinalResult += BeatNote_OnInteractionFinalResult;
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            beatNote.OnInteractionFinalResult -= BeatNote_OnInteractionFinalResult;
        }

        private void BeatNote_OnInteractionFinalResult(BeatNote.NoteTickInfo tickinfo,
            NoteInteractionFinalResult finalresult)
        {
            TimingWindow.Score timingResultScore = finalresult.TimingResult.Score;
            TimingWindow.Direction earlyOrLate = finalresult.TimingResult.Direction;

            if (finalresult.Successful)
            {
                switch (timingResultScore)
                {
                    case TimingWindow.Score.Pass when earlyOrLate == TimingWindow.Direction.Early:
                        _earlyParticles.Play();
                        break;
                    case TimingWindow.Score.Pass:
                    {
                        if (earlyOrLate == TimingWindow.Direction.Late)
                        {
                            _lateParticles.Play();
                        }

                        break;
                    }
                    case TimingWindow.Score.Perfect
                        when finalresult.NoteInteractionType == NoteInteractionType.Slice:
                    {
                        foreach (ParticleSystem perfectSliceParticle in _perfectSliceParticles)
                        {
                            perfectSliceParticle.Play();
                        }

                        break;
                    }
                    case TimingWindow.Score.Perfect:
                        _perfectParticles.Play();
                        break;
                }
            }
            else
            {
                _missParticles.Play();
            }
        }
    }
}