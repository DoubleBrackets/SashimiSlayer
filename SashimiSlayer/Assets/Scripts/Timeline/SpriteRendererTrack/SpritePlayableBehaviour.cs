
using System;
using UnityEngine;
using UnityEngine.Playables;


    // Runtime representation of a TextClip.
    // The Serializable attribute is required to be animated by timeline, and used as a template.
    [Serializable]
    public class SpritePlayableBehaviour : PlayableBehaviour
    {
        public Color color = Color.white;

        public Sprite sprite;
    }

