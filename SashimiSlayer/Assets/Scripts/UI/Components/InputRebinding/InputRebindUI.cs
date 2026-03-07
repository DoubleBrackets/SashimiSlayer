using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Events.Basic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace UI.Components.InputRebinding
{
    /// <summary>
    ///     UI handling interactive rebinding. Acts directly on the input asset, and emits an event when rebinding.
    ///     Code mostly taken from the RebindActionUI in InputSystem samples, with UI simplified
    /// </summary>
    public class InputRebindUI : MonoBehaviour
    {
        [Header("Config")]

        [SerializeField]
        private InputActionReference _actionRef;

        [SerializeField]
        private int _bindingIndex;

        [SerializeField]
        private InputBinding.DisplayStringOptions _displayStringOptions;

        [Header("UI References")]

        [SerializeField]
        private Button _rebindButton;

        [SerializeField]
        private Button _resetButton;

        [SerializeField]
        private TMP_Text _currentBindingText;

        [SerializeField]
        private TMP_Text _interactiveRebindingText;

        [SerializeField]
        private TMP_Text _timeoutText;

        [SerializeField]
        private CanvasGroup _reseterCanvasGroup;

        [SerializeField]
        private float _rebindTimeout;

        [Header("Event (Out)")]

        [SerializeField]
        private VoidEvent _onInputBindingOverride;

        private InputActionRebindingExtensions.RebindingOperation _rebindOperation;
        private float _rebindStartTime;

        private string _interactiveRebindTextValue;

        private void Awake()
        {
            InputSystem.onActionChange += OnActionChange;
            _resetButton.onClick.AddListener(ResetToDefault);
            _rebindButton.onClick.AddListener(StartInteractiveRebind);

            _interactiveRebindTextValue = _interactiveRebindingText.text;

            UpdateReseterCanvasGroup();
            UpdateCurrentBindingText();
        }

        private void OnDestroy()
        {
            InputSystem.onActionChange -= OnActionChange;
            _resetButton.onClick.RemoveListener(ResetToDefault);
            _rebindButton.onClick.RemoveListener(StartInteractiveRebind);
        }

        private void OnValidate()
        {
            UpdateCurrentBindingText();
        }

        private void Update()
        {
            if (_rebindOperation != null)
            {
                UpdateTimeoutInfo(true, _rebindStartTime);
            }
        }

        private void UpdateCurrentBindingText()
        {
            var displayString = string.Empty;

            InputAction action = _actionRef.action;
            if (action != null)
            {
                displayString = action.GetBindingDisplayString(
                    _bindingIndex,
                    out string deviceLayoutName,
                    out string controlPath,
                    _displayStringOptions);
            }

            if (_currentBindingText != null)
            {
                _currentBindingText.text = displayString;
            }
        }

        private void OnActionChange(object obj, InputActionChange change)
        {
            if (change != InputActionChange.BoundControlsChanged)
            {
                return;
            }

            var action = obj as InputAction;
            InputActionMap actionMap = action?.actionMap ?? obj as InputActionMap;
            InputActionAsset actionAsset = actionMap?.asset ?? obj as InputActionAsset;

            InputAction referencedAction = _actionRef.action;
            if (referencedAction == action ||
                referencedAction.actionMap == actionMap ||
                referencedAction.actionMap?.asset == actionAsset)
            {
                UpdateCurrentBindingText();
                UpdateReseterCanvasGroup();
            }
        }

        /// <summary>
        ///     Remove currently applied binding overrides.
        /// </summary>
        public void ResetToDefault()
        {
            InputAction action = _actionRef.action;
            int bindingIndex = _bindingIndex;

            if (action.bindings[bindingIndex].isComposite)
            {
                // It's a composite. Remove overrides from part bindings.
                for (int i = bindingIndex + 1; i < action.bindings.Count && action.bindings[i].isPartOfComposite; ++i)
                {
                    action.RemoveBindingOverride(i);
                }
            }
            else
            {
                action.RemoveBindingOverride(bindingIndex);
            }

            _onInputBindingOverride.Raise();
            UpdateCurrentBindingText();
        }

        /// <summary>
        ///     Initiate an interactive rebind that lets the player actuate a control to choose a new binding
        ///     for the action.
        /// </summary>
        private void StartInteractiveRebind()
        {
            Debug.Log(
                $"Interactive rebind started for action '{_actionRef.action.name}' and binding index '{_bindingIndex}'.");

            InputAction action = _actionRef.action;
            int bindingIndex = _bindingIndex;

            // If the binding is a composite, we need to rebind each part in turn.
            if (action.bindings[bindingIndex].isComposite)
            {
                int firstPartIndex = bindingIndex + 1;
                if (firstPartIndex < action.bindings.Count && action.bindings[firstPartIndex].isPartOfComposite)
                {
                    PerformInteractiveRebind(action, firstPartIndex, true);
                }
            }
            else
            {
                PerformInteractiveRebind(action, bindingIndex);
            }
        }

        private void PerformInteractiveRebind(InputAction action, int bindingIndex, bool allCompositeParts = false)
        {
            // Null out and cancel any existing rebind operation.
            _rebindOperation?.Cancel();

            // Extract enabled state to allow restoring enabled state after rebind completes
            bool actionWasEnabledPriorToRebind = action.enabled;

            // Do the same for input system ui input module
            var uiModule = EventSystem.current.GetComponent<InputSystemUIInputModule>();
            List<InputActionReference> toDisable = new List<InputActionReference>
            {
                uiModule.move,
                uiModule.submit
            }.Where(a => a.action.enabled).ToList();

            foreach (InputActionReference inputActionReference in toDisable)
            {
                inputActionReference.action.Disable();
            }

            void CleanUp()
            {
                _rebindOperation?.Dispose();
                _rebindOperation = null;

                UpdateRebindInfo(false);
                UpdateTimeoutInfo(false, 0);

                // Restore action enabled state based on state prior to rebind
                if (actionWasEnabledPriorToRebind)
                {
                    action.actionMap.Enable();
                }

                ReenableNavigationInput(toDisable).Forget();
            }

            // An "InvalidOperationException: Cannot rebind action x while it is enabled" will
            // be thrown if rebinding is attempted on an action that is enabled.
            //
            // On top of disabling the target action while rebinding, it is recommended to
            // disable any actions (or action maps) that could interact with the rebinding UI
            // or gameplay - it would be undesirable for rebinding to cause the player
            // character to jump.
            //
            // In this example, we explicitly disable both the UI input action map and
            // the action map containing the target action if it was initially enabled.
            if (actionWasEnabledPriorToRebind)
            {
                action.actionMap.Disable();
            }

            // Only allow rebinding of controls that are part of the same group, i.e devices in the binding's control scheme
            InputBinding baseBinding = action.bindings[bindingIndex];
            string group = baseBinding.groups;

            // Configure the rebind.
            _rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
                .OnCancel(operation =>
                {
                    UpdateCurrentBindingText();
                    CleanUp();
                })
                // We want matching events to be suppressed during rebinding (this is also default).
                //.WithMatchingEventsBeingSuppressed()
                // Since this sample has no interactable UI during rebinding we also want to suppress non-matching events.
                //.WithNonMatchingEventsBeingSuppressed()
                // We want device state to update but not actions firing during rebinding.
                .WithActionEventNotificationsBeingSuppressed()
                // We use a timeout to illustrate that its possible to skip cancel buttons and let rebind timeout.
                .WithTimeout(_rebindTimeout)
                .WithBindingGroup(group)
                .OnComplete(operation =>
                {
                    UpdateCurrentBindingText();
                    CleanUp();
                    _onInputBindingOverride.Raise();

                    // If there's more composite parts we should bind, initiate a rebind
                    // for the next part.
                    if (allCompositeParts)
                    {
                        int nextBindingIndex = bindingIndex + 1;
                        if (nextBindingIndex < action.bindings.Count &&
                            action.bindings[nextBindingIndex].isPartOfComposite)
                        {
                            PerformInteractiveRebind(action, nextBindingIndex, true);
                        }
                    }
                });

            // If it's a part binding, show the name of the part in the UI.
            var partName = default(string);
            if (action.bindings[bindingIndex].isPartOfComposite)
            {
                partName = $"Binding '{action.bindings[bindingIndex].name}'. ";
            }

            // Update rebind overlay information, if we have one.
            if (_interactiveRebindingText != null)
            {
                _rebindStartTime = Time.realtimeSinceStartup;
                UpdateRebindInfo(true, partName);
                UpdateTimeoutInfo(true, _rebindStartTime);
            }

            _rebindOperation.Start();
        }

        private async UniTaskVoid ReenableNavigationInput(List<InputActionReference> toEnable)
        {
            await UniTask.DelayFrame(1);
            foreach (InputActionReference inputActionReference in toEnable)
            {
                inputActionReference.action.Enable();
            }
        }

        private void UpdateRebindInfo(bool isRebinding, string partName = "")
        {
            _interactiveRebindingText.text =
                isRebinding ? $"Press any input to rebind {partName}" : _interactiveRebindTextValue;
        }

        private void UpdateTimeoutInfo(bool isRebinding, float startTime)
        {
            float timeRemaining = _rebindTimeout - (Time.realtimeSinceStartup - startTime);
            _timeoutText.text = isRebinding ? $"Cancels in : {Mathf.RoundToInt(timeRemaining)}s" : string.Empty;
        }

        private void UpdateReseterCanvasGroup()
        {
            bool anyBindingOverrides =
                _actionRef.action.bindings.Aggregate(false, (b, binding) => b || binding.hasOverrides);

            _reseterCanvasGroup.alpha = anyBindingOverrides ? 1 : 0.5f;
        }
    }
}