using Core.Protag;
using UnityEngine;

namespace Events.Core
{
    /// <summary>
    ///     Event for when a slice occurs, either in gameplay or when navigating menus
    /// </summary>
    [CreateAssetMenu(menuName = "Events/Core/SliceResultEvent")]
    public class SliceResultEvent : SOEvent<SliceResultData>
    {
    }
}