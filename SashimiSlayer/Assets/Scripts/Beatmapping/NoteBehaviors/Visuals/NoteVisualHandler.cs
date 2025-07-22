using System;
using System.Collections.Generic;
using Beatmapping.Interactions;
using Beatmapping.Notes;
using Beatmapping.Tooling;
using NaughtyAttributes;

namespace Beatmapping.NoteBehaviors.Visuals
{
    /// <summary>
    ///     Handles varying note visuals for notes of the same behavior
    /// </summary>
    public class NoteVisualHandler : BeatNoteModule
    {
        [Serializable]
        public struct NoteVisual
        {
            public NoteVisualObject VisualObject;

            [AllowNesting]
            [HideIf("IsForSlicing")]
            public SharedTypes.BlockPoseStates Pose;

            public bool IsForSlicing;
        }

        public List<NoteVisual> Visuals;

        private NoteVisualObject CurrentVisualObject => Visuals[_currentVisualIndex].VisualObject;

        private int _prevInteractionIndex = -1;

        private int _currentVisualIndex;

        private void Awake()
        {
            DisableAllVisuals();
        }

        public override IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            return null;
        }

        public override void OnNoteInitialized(BeatNote beatNote)
        {
            beatNote.OnTick += BeatNote_OnTick;
            SwitchToVisual(_currentVisualIndex);
        }

        public override void OnNoteCleanedUp(BeatNote beatNote)
        {
            beatNote.OnTick -= BeatNote_OnTick;
        }

        private void BeatNote_OnTick(BeatNote.NoteTickInfo tickinfo)
        {
            NoteInteraction interaction = tickinfo.NoteSegment.Interaction;
            if (interaction == null)
            {
                return;
            }

            int interactionIndex = tickinfo.InteractionIndex;
            if (interactionIndex == _prevInteractionIndex)
            {
                return;
            }

            _prevInteractionIndex = interactionIndex;

            int newVisual = _currentVisualIndex;

            if (interaction.Type == NoteInteraction.InteractionType.Slice)
            {
                newVisual = GetSliceVisualIndex();
            }
            else
            {
                newVisual = GetBlockVisualIndex(interaction);
            }

            // If no visual found, keep current visual
            if (newVisual == -1)
            {
                return;
            }

            if (newVisual == _currentVisualIndex)
            {
                return;
            }

            _currentVisualIndex = newVisual;
            SwitchToVisual(_currentVisualIndex);
        }

        private void SwitchToVisual(int index)
        {
            DisableAllVisuals();
            Visuals[index].VisualObject.SetVisualObjectVisible(true);
        }

        public void SetHitParticle(bool visible)
        {
            CurrentVisualObject.SetHitParticle(visible);
        }

        public void SetTrailParticle(bool visible)
        {
            CurrentVisualObject.SetTrailParticle(visible);
        }

        public void SetFlipX(bool flip)
        {
            CurrentVisualObject.SetFlipX(flip);
        }

        public void SetSpriteAlpha(float alpha)
        {
            CurrentVisualObject.SetSpriteAlpha(alpha);
        }

        public void SetVisible(bool visible)
        {
            CurrentVisualObject.SetSpriteVisible(visible);
        }

        public void SetRotation(float rot)
        {
            CurrentVisualObject.SetRotation(rot);
        }

        public void PlayAnimation(int index, int firstSubdiv)
        {
            // Run all animations, even for hidden visuals, so switching to them is seamless and sync'd
            foreach (NoteVisual visual in Visuals)
            {
                visual.VisualObject.PlayAnimation(index, firstSubdiv);
            }
        }

        /// <summary>
        ///     Single param variant for Unity Events to use
        /// </summary>
        /// <param name="index"></param>
        public void PlayAnimation(int index)
        {
            PlayAnimation(index, -1);
        }

        public void SetupAnimationTransitionOnEnd(int fromIndex, int toIndex)
        {
            // Run all animations, even for hidden visuals, so switching to them is seamless and sync'd
            foreach (NoteVisual visual in Visuals)
            {
                visual.VisualObject.SetAnimationTransitionToOnEnd(fromIndex, toIndex);
            }
        }

        public void AnimationForceTransition(int fromIndex, int toIndex, int currentSubdiv = -1)
        {
            // Run all animations, even for hidden visuals, so switching to them is seamless and sync'd
            foreach (NoteVisual visual in Visuals)
            {
                visual.VisualObject.AnimationForceTransition(fromIndex, toIndex, currentSubdiv);
            }
        }

        private void DisableAllVisuals()
        {
            foreach (NoteVisual visual in Visuals)
            {
                visual.VisualObject.SetVisualObjectVisible(false);
            }
        }

        private int GetSliceVisualIndex()
        {
            return Visuals.FindIndex(v => v.IsForSlicing);
        }

        private int GetBlockVisualIndex(NoteInteraction interaction)
        {
            return Visuals.FindIndex(v => !v.IsForSlicing && v.Pose == interaction.BlockPose);
        }
    }
}