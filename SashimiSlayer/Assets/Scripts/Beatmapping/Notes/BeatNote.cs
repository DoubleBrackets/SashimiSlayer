using System;
using System.Collections.Generic;
using System.Linq;
using Base;
using Beatmapping.Interactions;
using Beatmapping.Tooling;
using Events.Core;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Serialization;

namespace Beatmapping.Notes
{
    /// <summary>
    ///     Represents a single sequenced note with some interactions.
    ///     Supports scrubbing (arbitrary tick timing)
    /// </summary>
    public partial class BeatNote : DescMono, IInteractionUser
    {
        public delegate void TickEventHandler(
            NoteTickInfo tickInfo);

        public delegate void InteractionAttemptEventHandler(
            int interactionIndex,
            NoteInteraction.AttemptResult result);

        public delegate void InteractionFinalResultEventHandler(
            NoteTickInfo tickInfo,
            NoteInteraction.FinalResult finalResult);

        /// <summary>
        ///     Fixed time between end and cleanup
        /// </summary>
        private const double CleanupTime = 2.5f;

        // Serialized fields
        [SerializeField]
        private Transform _hitboxTransform;

        [Header("Events (Out)")]

        [SerializeField]
        private NoteInteractionFinalResultEvent _noteInteractionFinalResultEvent;

        [SerializeField]
        private ObjectSlicedEvent _objectSlicedEvent;

        [FormerlySerializedAs("_beatNoteListeners")]
        [SerializeField]
        private List<BeatNoteModule> _beatNoteModules;

        public Vector2 StartPosition { get; private set; }

        public Vector2 EndPosition { get; private set; }

        // Events

        /// <summary>
        ///     Note initialized
        /// </summary>
        public event Action OnInitialize;

        /// <summary>
        ///     Note started. TODO: Doesn't trigger properly on looping...
        /// </summary>
        public event TickEventHandler OnNoteStart;

        /// <summary>
        ///     First tick of first interaction segment
        /// </summary>
        public event TickEventHandler OnFirstInteractionTick;

        /// <summary>
        ///     Note ended (not cleaned up)
        /// </summary>
        public event TickEventHandler OnNoteEnd;

        /// <summary>
        ///     Protag blocked this note
        /// </summary>
        public event InteractionAttemptEventHandler OnBlockedByProtag;

        /// <summary>
        ///     Protag successfully sliced this note
        /// </summary>
        public event InteractionAttemptEventHandler OnSlicedByProtag;

        /// <summary>
        ///     Protag was hit by an interaction
        /// </summary>
        public event InteractionFinalResultEventHandler OnProtagFailBlock;

        /// <summary>
        ///     Protag missed a slice interaction on this note
        /// </summary>
        public event InteractionFinalResultEventHandler OnProtagMissedHit;

        /// <summary>
        ///     Event invoked when an interaction's final result occurs
        /// </summary>
        public event InteractionFinalResultEventHandler OnInteractionFinalResult;

        /// <summary>
        ///     Event invoked when the note is ready to be cleaned up.
        ///     Listened to by the BeatNoteService, which performs the cleanup
        /// </summary>
        public event Action<BeatNote> OnReadyForCleanup;

        public event TickEventHandler OnTick;

        // State
        private NoteTickInfo _noteTickInfo;
        private NoteTickInfo _prevTickInfo;
        private List<NoteTimeSegment> _noteTimeSegments;
        private List<NoteInteraction> _allInteractions;

        private float _hitboxRadius;
        private int _damageDealtToPlayer;

        /// <summary>
        ///     When the note starts being "active". The note can be initialized before this time
        /// </summary>
        private double _noteStartTime;

        /// <summary>
        ///     When the note ends being "active". The note object can still be alive after this time
        /// </summary>
        private double _noteEndTime;

        private bool _isFirstTick;
        private bool _isFirstInteraction;

        public void OnDestroy()
        {
            foreach (BeatNoteModule listener in _beatNoteModules)
            {
                listener.OnNoteCleanedUp(this);
            }
        }

        private void OnDrawGizmos()
        {
            DrawDebug();
        }

        public IEnumerable<IInteractionUser.InteractionUsage> GetInteractionUsages()
        {
            var positionUsage = new List<IInteractionUser.InteractionUsage>();

            foreach (BeatNoteModule listener in _beatNoteModules)
            {
                IEnumerable<IInteractionUser.InteractionUsage> usages = listener.GetInteractionUsages();
                if (usages == null)
                {
                    continue;
                }

                positionUsage.AddRange(usages);
            }

            return positionUsage;
        }

        /// <summary>
        ///     Initialize the note with the given interactions and positions.
        ///     All time parameters are expected to be in beatmap timespace
        /// </summary>
        public void Initialize(
            List<NoteInteraction> noteInteractions,
            Vector2 noteStartPos,
            Vector2 noteEndPos,
            double noteStartTime,
            double noteEndTime,
            double initializeTime,
            float hitboxRadius,
            int damageDealtToPlayer
        )
        {
            _allInteractions = new List<NoteInteraction>(noteInteractions);
            _noteStartTime = noteStartTime;
            _noteEndTime = noteEndTime;
            _hitboxRadius = hitboxRadius;
            _damageDealtToPlayer = damageDealtToPlayer;
            StartPosition = noteStartPos;
            EndPosition = noteEndPos;

            // Default values
            _isFirstTick = true;
            _isFirstInteraction = true;

            // Build timing segments
            _noteTimeSegments = BuildNoteTimeSegments(noteInteractions, noteStartTime, noteEndTime, initializeTime);

            foreach (BeatNoteModule listener in _beatNoteModules)
            {
                listener.OnNoteInitialized(this);
            }

            OnInitialize?.Invoke();
        }

        /// <summary>
        ///     Reset the internal state of the note so it can be looped or pooled.
        /// </summary>
        public void ResetState()
        {
            // Currently the only state is the interaction state
            foreach (NoteInteraction interaction in _allInteractions)
            {
                interaction.ResetState();
            }

            foreach (BeatNoteModule listener in _beatNoteModules)
            {
                listener.ResetState();
            }

            _isFirstTick = true;
            _isFirstInteraction = true;
        }

        private List<NoteTimeSegment> BuildNoteTimeSegments(List<NoteInteraction> interactions,
            double noteStartTime,
            double noteEndTime,
            double initializeTime)
        {
            var timeSegments = new List<NoteTimeSegment>();

            // Initiationalize time can't be after the note starts
            // But with the current logic it usually is 1 frame after due to how Timeline behaviors work
            if (initializeTime > noteStartTime)
            {
                initializeTime = noteStartTime;
            }

            // Segment ranging from initiation to when the note "starts"
            timeSegments.Add(new NoteTimeSegment
            {
                SegmentStartTime = initializeTime,
                Interaction = null,
                Type = TimeSegmentType.Spawn
            });

            double nextTime = noteStartTime;

            foreach (NoteInteraction interaction in interactions)
            {
                timeSegments.Add(new NoteTimeSegment
                {
                    SegmentStartTime = nextTime,
                    Interaction = interaction,
                    Type = TimeSegmentType.Interaction
                });
                nextTime = interaction.TargetTime;
            }

            // Segment ranging from previous segment to ending
            timeSegments.Add(new NoteTimeSegment
            {
                SegmentStartTime = nextTime,
                Interaction = null,
                Type = TimeSegmentType.PreEnding
            });

            // Segment ranging from ending to final cleanup
            timeSegments.Add(new NoteTimeSegment
            {
                SegmentStartTime = noteEndTime,
                Interaction = null,
                Type = TimeSegmentType.Ending
            });

            // Terminating segment
            timeSegments.Add(new NoteTimeSegment
            {
                SegmentStartTime = noteEndTime + CleanupTime,
                Interaction = null,
                Type = TimeSegmentType.CleanedUp
            });

            return timeSegments;
        }

        [Button("Detect Listeners")]
        private void DetectListeners()
        {
            BeatNoteModule[] listeners = GetComponentsInChildren<BeatNoteModule>();

            _beatNoteModules = listeners.Where(a => a != null).ToList();

            foreach (BeatNoteModule listener in _beatNoteModules)
            {
                if (_beatNoteModules.Contains(listener))
                {
                    continue;
                }

                _beatNoteModules.Add(listener);
            }
        }

        /// <summary>
        ///     Get the position right before the current interaction
        /// </summary>
        /// <param name="interactionIndex"></param>
        /// <returns></returns>
        public Vector2 GetPreviousPosition(int interactionIndex)
        {
            // No interactions before this one, use the start position
            if (interactionIndex == 0)
            {
                return StartPosition;
            }

            if (interactionIndex > _allInteractions.Count)
            {
                Debug.LogWarning($"Interaction index {interactionIndex - 1} out of bounds");
                return Vector2.zero;
            }

            NoteInteraction prevSegment = _allInteractions[interactionIndex - 1];
            List<Vector2> prevPositions = prevSegment.Positions;

            if (prevPositions.Count == 0)
            {
                Debug.LogWarning($"Unable to find previous position for interaction {interactionIndex}");
                return Vector2.zero;
            }

            return prevPositions[^1];
        }

        public Vector2 GetFinalInteractionPosition()
        {
            if (_allInteractions.Count == 0)
            {
                return StartPosition;
            }

            NoteInteraction finalInteraction = _allInteractions[^1];
            List<Vector2> finalPositions = finalInteraction.Positions;

            if (finalPositions.Count == 0)
            {
                return StartPosition;
            }

            return finalPositions[^1];
        }

        public Vector2 GetInteractionPosition(int interactionIndex, int positionIndex)
        {
            if (interactionIndex >= _allInteractions.Count)
            {
                Debug.LogWarning($"Interaction index {interactionIndex} out of bounds");
                return Vector2.zero;
            }

            List<Vector2> positions = _allInteractions[interactionIndex].Positions;

            if (positionIndex >= positions.Count)
            {
                Debug.LogWarning($"Position index {positionIndex} out of bounds for interaction {interactionIndex}");
                return Vector2.zero;
            }

            return positions[positionIndex];
        }

        public Vector2 SetInteractionPosition(int interactionIndex, int positionIndex, Vector2 position)
        {
            if (interactionIndex >= _allInteractions.Count)
            {
                Debug.LogWarning($"Interaction index {interactionIndex} out of bounds");
                return Vector2.zero;
            }

            List<Vector2> positions = _allInteractions[interactionIndex].Positions;

            if (positionIndex >= positions.Count)
            {
                Debug.LogWarning($"Position index {positionIndex} out of bounds for interaction {interactionIndex}");
                return Vector2.zero;
            }

            positions[positionIndex] = position;
            return position;
        }

        private int GetInteractionIndex(NoteInteraction interaction)
        {
            return _allInteractions.IndexOf(interaction);
        }

        private void DrawDebug()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(_hitboxTransform.transform.position, _hitboxRadius);

/*#if UNITY_EDITOR
            if (_noteTickInfo.InteractionIndex != -1)
            {
                var style = new GUIStyle
                {
                    fontSize = 12,
                    normal = { textColor = Color.red }
                };

                Handles.Label(_hitboxTransform.transform.position + Vector3.up * 2,
                    $"\nIndex {_currentInteractionIndex}" +
                    $"\nType: {CurrentInteraction.Type}" +
            }
#endif*/
        }
    }
}