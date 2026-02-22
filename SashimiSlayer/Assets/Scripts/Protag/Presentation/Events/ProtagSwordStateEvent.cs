using Events;
using Protag.Core;
using UnityEngine;

namespace Protag.Presentation.Events
{
    [CreateAssetMenu(menuName = "Events/Protag/ProtagSwordStateEvent")]
    public class ProtagSwordStateEvent : SOEvent<Protaganist.ProtagSwordState>
    {
    }
}