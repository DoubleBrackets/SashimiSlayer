using Core.Protag;
using Cysharp.Threading.Tasks;
using Events;
using Events.Core;
using UnityEngine;
using UnityEngine.Events;

namespace Menus
{
    /// <summary>
    ///     Generic component for triggering an event when the player slices it
    /// </summary>
    public class SliceTriggeredButton : MonoBehaviour
    {
        public enum CanvasType
        {
            Worldspace,
            CameraOverlay,
            ScreenOverlay
        }

        [Header("Listening Events")]

        [SerializeField]
        private ProtagSwordStateEvent _protagSliceEvent;

        [SerializeField]
        private ProtagSwordStateEvent _protagSwordStateEvent;

        [Header("Invoking Events")]

        [SerializeField]
        private VoidEvent _buttonSlicedEvent;

        [SerializeField]
        private ObjectSlicedEvent _objectSlicedEvent;

        [SerializeField]
        private SliceResultEvent _sliceResultEvent;

        [SerializeField]
        private UnityEvent _buttonSlicedUnityEvent;

        [SerializeField]
        private UnityEvent _buttonSlicedDelayedEvent;

        [SerializeField]
        private UnityEvent _buttonHoveredEvent;

        [SerializeField]
        private UnityEvent _buttonUnhoveredEvent;

        [Header("Config")]

        [SerializeField]
        private float _radius;

        [SerializeField]
        private bool _singleUse;

        [SerializeField]
        private float _delay;

        [SerializeField]
        private CanvasType _isInCanvasSpace;

        private bool _used;

        private bool _hovered;

        private void Awake()
        {
            _protagSliceEvent.AddListener(OnProtagSlice);
            _protagSwordStateEvent.AddListener(OnProtagSwordState);
        }

        private void OnDestroy()
        {
            _protagSliceEvent.RemoveListener(OnProtagSlice);
            _protagSwordStateEvent.RemoveListener(OnProtagSwordState);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }

        private void OnProtagSwordState(Protaganist.ProtagSwordState obj)
        {
            Vector2 pos = CameraCanvasPositionToWorld(transform.position);

            float dist = Protaganist.Instance.DistanceToSwordPlane(pos);

            if (dist < _radius)
            {
                _hovered = true;
                _buttonHoveredEvent?.Invoke();
            }
            else
            {
                _hovered = false;
                _buttonUnhoveredEvent?.Invoke();
            }
        }

        private void OnProtagSlice(Protaganist.ProtagSwordState swordState)
        {
            if (_used && _singleUse)
            {
                return;
            }

            if (_hovered)
            {
                Vector2 pos = CameraCanvasPositionToWorld(transform.position);

                _objectSlicedEvent.Raise(new ObjectSlicedData
                {
                    Position = pos
                });

                // Stupid hack to make the event work since there's no centralized slice manager for non-notes
                _sliceResultEvent.Raise(new SliceResultData(1, SliceResultData.SlicedObject.MenuItem));
                _used = true;
                _buttonSlicedUnityEvent?.Invoke();
                TriggerDelayedEvent(_delay).Forget();
            }
        }

        private async UniTaskVoid TriggerDelayedEvent(float delay)
        {
            await UniTask.Delay((int)(delay * 1000));
            _buttonSlicedDelayedEvent?.Invoke();
            if (_buttonSlicedEvent != null)
            {
                _buttonSlicedEvent.Raise();
            }
        }

        private Vector2 CameraCanvasPositionToWorld(Vector2 cameraCanvasPosition)
        {
            if (_isInCanvasSpace == CanvasType.CameraOverlay || _isInCanvasSpace == CanvasType.Worldspace)
            {
                // Camera overlay canvases are really in world space
                // Since we're using orthographic we can just return the position directly
                return cameraCanvasPosition;
            }

            Camera camera = Camera.main;
            return camera.ScreenToWorldPoint(cameraCanvasPosition);
        }
    }
}