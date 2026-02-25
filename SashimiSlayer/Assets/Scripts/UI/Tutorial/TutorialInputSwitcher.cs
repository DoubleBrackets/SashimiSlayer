using System;
using System.Collections.Generic;
using GameInput.Interface;
using GameInput.ValueSO;
using UnityEngine;
using ValueSO;

namespace UI.Tutorial
{
    /// <summary>
    ///     Handled control scheme changes in the tutorial scene
    /// </summary>
    public class TutorialInputSwitcher : MonoBehaviour, IValueSOObserver
    {
        [Serializable]
        private class SpriteRendererArray
        {
            public SpriteRenderer[] sprites;
        }

        [SerializeField]
        private List<SpriteRendererArray> _sprites;

        [SerializeField]
        private ControlSchemeValueSO _controlSchemeValueSO;

        private void Awake()
        {
            _controlSchemeValueSO.AddListener(this, HandleControlSchemeChanged, true);
        }

        private void OnDestroy()
        {
            _controlSchemeValueSO.RemoveListener(this);
        }

        private void HandleControlSchemeChanged(ControlSchemes controlScheme)
        {
            for (var i = 0; i < _sprites.Count; i++)
            {
                bool isMatchingControlPrompt = i == (int)controlScheme;
                foreach (SpriteRenderer sprite in _sprites[i].sprites)
                {
                    sprite.enabled = isMatchingControlPrompt;
                }
            }
        }
    }
}