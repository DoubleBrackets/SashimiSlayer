using NaughtyAttributes;
using UnityEngine;

namespace Beatmapping.Notes
{
    [CreateAssetMenu(fileName = "BeatNoteType", menuName = "BeatMapping/BeatNoteType")]
    public class BeatNoteTypeSO : ScriptableObject
    {
        [field: SerializeField]
        public float HitboxRadius { get; private set; }

        [field: ShowAssetPreview]
        [field: SerializeField]
        public BeatNote Prefab { get; private set; }
    }
}