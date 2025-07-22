using Base;
using Beatmapping;
using Beatmapping.Tooling;
using Events;
using Events.Core;
using Feel;
using GameInput;
using UnityEngine;

namespace Core.Protag
{
    public class Protaganist : DescMono
    {
        public struct ProtagSwordState
        {
            public SharedTypes.BlockPoseStates BlockPose;
            public SharedTypes.SheathState SheathState;
            public SharedTypes.BlockPoseStates RecentHitBlockPose;
            public Vector3 SwordPosition;
            public float SwordAngle;

            public float DistanceToSwordPlane(Vector3 position)
            {
                Vector3 swordPlaneNormal = Quaternion.Euler(0, 0, SwordAngle) * Vector3.up;
                Vector3 swordPlanePoint = SwordPosition;

                Vector3 pointOnPlane = position - swordPlanePoint;
                float distance = Mathf.Abs(Vector3.Dot(pointOnPlane, swordPlaneNormal));
                return distance;
            }
        }

        [Header("Events (Out)")]

        [SerializeField]
        private FloatEvent _healthChangeEvent;

        [SerializeField]
        private FloatEvent _maxHealthChangeEvent;

        [SerializeField]
        private VoidEvent _damageTakenEvent;

        [SerializeField]
        private ProtagSwordStateEvent _blockSuccessEvent;

        [SerializeField]
        private ProtagSwordStateEvent _blockSuccessTopEvent;

        [SerializeField]
        private ProtagSwordStateEvent _blockSuccessMidEvent;

        [SerializeField]
        private ProtagSwordStateEvent _blockSuccessBotEvent;

        [SerializeField]
        private ProtagSwordStateEvent _tryBlockEvent;

        [SerializeField]
        private ProtagSwordStateEvent _trySliceEvent;

        [SerializeField]
        private ProtagSwordStateEvent _swordStateChangeEvent;

        [SerializeField]
        private VoidEvent _playerDeadEvent;

        [SerializeField]
        private ProtagSwordStateEvent _unsheatheEvent;

        [SerializeField]
        private ProtagSwordStateEvent _sheatheEvent;

        [SerializeField]
        private ProtagSwordStateEvent _successfulSliceEvent;

        [Header("Events (In)")]

        [SerializeField]
        private BeatmapEvent _beatmapLoadedEvent;

        [SerializeField]
        private VoidEvent _onDrawDebugGuiEvent;

        [SerializeField]
        private Vector2Event _protagSetSwordPivot;

        [SerializeField]
        private SliceResultEvent _sliceResultEvent;

        [Header("Screen Shake")]

        [SerializeField]
        private ScreenShakeSO _damageScreenShake;

        [SerializeField]
        private ScreenShakeSO _blockScreenShake;

        [SerializeField]
        private ScreenShakeSO _sliceScreenShake;

        public static Protaganist Instance { get; private set; }

        /// <summary>
        ///     The position that notes move towards
        /// </summary>
        public Vector3 NoteTargetPosition { get; set; }

        public Vector3 SwordPosition => _currentSwordState.SwordPosition;

        private BaseUserInputProvider _inputProvider;

        private float _health;
        private float _maxHealth;

        private ProtagSwordState _currentSwordState;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            _inputProvider = InputService.Instance;

            _health = _maxHealth;
            _healthChangeEvent.Raise(_health);

            _inputProvider.OnSheathStateChanged += OnSheathStateChanged;
            _inputProvider.OnBlockPoseChanged += OnPoseStateChanged;
            _beatmapLoadedEvent.AddListener(HandleBeatmapLoaded);
            _onDrawDebugGuiEvent.AddListener(HandleDrawDebugGUI);
            _protagSetSwordPivot.AddListener(SetSwordPosition);
            _sliceResultEvent.AddListener(HandleSliceResult);
        }

        private void Update()
        {
            _currentSwordState.SwordAngle = _inputProvider.GetSwordAngle();
            _swordStateChangeEvent.Raise(_currentSwordState);
        }

        private void OnDestroy()
        {
            _inputProvider.OnSheathStateChanged -= OnSheathStateChanged;
            _inputProvider.OnBlockPoseChanged -= OnPoseStateChanged;
            _beatmapLoadedEvent.RemoveListener(HandleBeatmapLoaded);
            _onDrawDebugGuiEvent.RemoveListener(HandleDrawDebugGUI);
            _protagSetSwordPivot.RemoveListener(SetSwordPosition);
            _sliceResultEvent.RemoveListener(HandleSliceResult);
        }

        private void HandleDrawDebugGUI()
        {
            GUILayout.Label($"Sword angle: {_currentSwordState.SwordAngle}");
            GUILayout.Label($"Sword position: {_currentSwordState.SwordPosition}");
            GUILayout.Label($"Sheath state: {_currentSwordState.SheathState}");
            GUILayout.Label($"Pose state: {_inputProvider.GetBlockPose()}");
        }

        private void HandleBeatmapLoaded(BeatmapConfigSo beatmap)
        {
            _maxHealth = beatmap.PlayerMaxHealth;
            _health = _maxHealth;
            _maxHealthChangeEvent.Raise(_maxHealth);
            _healthChangeEvent.Raise(_health);
        }

        private void OnPoseStateChanged(SharedTypes.BlockPoseStates blockPoseStates)
        {
            _currentSwordState.BlockPose = blockPoseStates;
            _tryBlockEvent.Raise(_currentSwordState);
        }

        private void OnSheathStateChanged(SharedTypes.SheathState newState)
        {
            SharedTypes.SheathState oldState = _currentSwordState.SheathState;
            _currentSwordState.SheathState = newState;

            // Sword is sheathed from unsheathed means a slice
            if (newState == SharedTypes.SheathState.Unsheathed
                && oldState == SharedTypes.SheathState.Sheathed)
            {
                _trySliceEvent.Raise(_currentSwordState);
            }

            if (newState == SharedTypes.SheathState.Unsheathed && oldState == SharedTypes.SheathState.Sheathed)
            {
                _unsheatheEvent.Raise(_currentSwordState);
            }
            else if (newState == SharedTypes.SheathState.Sheathed && oldState == SharedTypes.SheathState.Unsheathed)
            {
                _sheatheEvent.Raise(_currentSwordState);
            }
        }

        public void SetSwordPosition(Vector2 position)
        {
            _currentSwordState.SwordPosition = position;
        }

        public void TakeDamage(float damage)
        {
            ScreenShakeService.Instance.ShakeScreen(_damageScreenShake);

            if (_health <= 0)
            {
                return;
            }

            if (!BeatmappingUtilities.ProtagInvincible)
            {
                _health -= damage;
            }

            _healthChangeEvent.Raise(_health);
            _damageTakenEvent.Raise();

            if (_health <= 0)
            {
                _playerDeadEvent.Raise();
            }
        }

        public void SuccessfulBlock(SharedTypes.BlockPoseStates pose)
        {
            switch (pose)
            {
                case SharedTypes.BlockPoseStates.TopPose:
                    _blockSuccessTopEvent.Raise(_currentSwordState);
                    break;
                case SharedTypes.BlockPoseStates.MidPose:
                    _blockSuccessMidEvent.Raise(_currentSwordState);
                    break;
            }

            _blockSuccessEvent.Raise(_currentSwordState);

            ScreenShakeService.Instance.ShakeScreen(_blockScreenShake);
        }

        private void HandleSliceResult(SliceResultData data)
        {
            if (data.SliceCount > 0)
            {
                _successfulSliceEvent.Raise(_currentSwordState);
                ScreenShakeService.Instance.ShakeScreen(_sliceScreenShake);
            }
        }

        public void SetSlashPosition(Vector3 position)
        {
            _currentSwordState.SwordPosition = position;
            _swordStateChangeEvent.Raise(_currentSwordState);
        }

        public float DistanceToSwordPlane(Vector2 pos)
        {
            return _currentSwordState.DistanceToSwordPlane(pos);
        }
    }
}