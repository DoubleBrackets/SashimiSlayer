using UnityEngine;

namespace Events.Basic
{
    [CreateAssetMenu(menuName = "Events/Basic/VoidEvent")]
    public class VoidEvent : SOEvent
    {
        public void Raise()
        {
            _internalVoidEvent?.Invoke();
        }
    }
}