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
    public class ControlSchemeDisplaySelector : MonoBehaviour, IValueSOObserver
    {
        [Serializable]
        private class ControlSchemeDisplay
        {
            public ControlSchemes controlScheme;
            public GameObject[] objects;
        }

        [SerializeField]
        private List<ControlSchemeDisplay> _controlSchemeDisplays;

        [Header("ValueSO (Read)")]

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
            for (var i = 0; i < _controlSchemeDisplays.Count; i++)
            {
                bool isMatchingControlPrompt = _controlSchemeDisplays[i].controlScheme == controlScheme;
                foreach (GameObject obj in _controlSchemeDisplays[i].objects)
                {
                    obj.SetActive(isMatchingControlPrompt);
                }
            }
        }
    }
}