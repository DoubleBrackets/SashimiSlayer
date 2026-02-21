using Beatmapping.Interactions;
using Events;
using Events.Core;
using Framework.Services;
using GameInput.Haptics;
using GameInput.Interface;
using UnityEngine;

namespace Core.Protag
{
    /// <summary>
    ///     Handles hooking up events to the appropriate rumble
    /// </summary>
    public class ProtagRumble : MonoBehaviour
    {
        [Header("Rumbles")]

        [SerializeField]
        private RumbleFeedbackSO _damagedRumbleFeedback;

        [SerializeField]
        private RumbleFeedbackSO _perfectHitRumbleFeedback;

        [SerializeField]
        private RumbleFeedbackSO _imperfectHitRumbleFeedback;

        [SerializeField]
        private RumbleFeedbackSO _blockRumbleFeedback;

        [Header("Events (In)")]

        [SerializeField]
        private VoidEvent _onDamageTakenEvent;

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        [SerializeField]
        private SliceResultEvent _sliceResultEvent;

        private IUserInput _userInput;

        private void Awake()
        {
            _userInput = ServiceLocator.GetService<IUserInput>();
            _onDamageTakenEvent.AddListener(HandleDamageTaken);
            _noteInteractionFinalResultEvent.AddListener(HandleNoteInteractionFinalResult);
            _sliceResultEvent.AddListener(HandleSliceResult);
        }

        private void OnDestroy()
        {
            _onDamageTakenEvent.RemoveListener(HandleDamageTaken);
            _noteInteractionFinalResultEvent.RemoveListener(HandleNoteInteractionFinalResult);
            _sliceResultEvent.RemoveListener(HandleSliceResult);
        }

        private void HandleDamageTaken()
        {
            _userInput.AddRumble(_damagedRumbleFeedback);
        }

        private void HandleNoteInteractionFinalResult(NoteInteraction.FinalResult result)
        {
            if (result.InteractionType == NoteInteraction.InteractionType.Slice)
            {
                if (result.TimingResult.IsPerfect())
                {
                    _userInput.AddRumble(_perfectHitRumbleFeedback);
                }
                else if (result.TimingResult.IsLate() || result.TimingResult.IsEarly())
                {
                    _userInput.AddRumble(_imperfectHitRumbleFeedback);
                }
            }
            else
            {
                if (result.Successful)
                {
                    _userInput.AddRumble(_blockRumbleFeedback);
                }
            }
        }

        private void HandleSliceResult(SliceResultData result)
        {
            if (result.SlicedObjectType == SliceResultData.SlicedObject.MenuItem)
            {
                _userInput.AddRumble(_perfectHitRumbleFeedback);
            }
        }
    }
}