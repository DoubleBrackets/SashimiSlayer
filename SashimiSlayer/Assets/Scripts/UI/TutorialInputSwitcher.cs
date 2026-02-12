using System;
using System.Collections.Generic;
using CommonTypes;
using Events;
using Framework;
using GameInput.Interface;
using UnityEngine;

/// <summary>
///     Handled control scheme changes in the tutorial scene
/// </summary>
public class TutorialInputSwitcher : MonoBehaviour, IFindsDependencies
{
    [Serializable]
    private class SpriteRendererArray
    {
        public SpriteRenderer[] sprites;
    }

    [SerializeField]
    private List<SpriteRendererArray> _sprites;

    [SerializeField]
    private IntEvent _controlSchemeChangedEvent;

    private IUserInput _userInput;

    public void FindDependencies()
    {
        _userInput = ServiceLocator.GetUserInput();
    }

    private void Awake()
    {
        FindDependencies();
        _controlSchemeChangedEvent.AddListener(HandleControlSchemeChanged);
        HandleControlSchemeChanged((int)_userInput.ControlScheme);
    }

    private void OnDestroy()
    {
        _controlSchemeChangedEvent.RemoveListener(HandleControlSchemeChanged);
    }

    private void HandleControlSchemeChanged(int controlScheme)
    {
        for (var i = 0; i < _sprites.Count; i++)
        {
            bool isMatchingControlPrompt = i == controlScheme;
            foreach (SpriteRenderer sprite in _sprites[i].sprites)
            {
                sprite.enabled = isMatchingControlPrompt;
            }
        }
    }
}