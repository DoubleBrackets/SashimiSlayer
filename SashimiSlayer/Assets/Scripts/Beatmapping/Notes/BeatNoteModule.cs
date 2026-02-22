using System.Collections.Generic;
using Beatmapping.Tooling;
using UnityEngine;

namespace Beatmapping.Notes
{
    /// <summary>
    ///     Base class for objects that depend on BeatNote.
    /// </summary>
    public abstract class BeatNoteModule : MonoBehaviour, INoteInteractionUser
    {
        /// <summary>
        ///     Returns the interaction usages that this listener uses.
        ///     This is primarily for beatmapping tool use to automatically configure notes from note prefabs
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<INoteInteractionUser.InteractionUsage> GetInteractionUsages();

        public abstract void OnNoteInitialized(BeatNote beatNote);
        public abstract void OnNoteCleanedUp(BeatNote beatNote);

        public virtual void ResetState()
        {
        }
    }
}