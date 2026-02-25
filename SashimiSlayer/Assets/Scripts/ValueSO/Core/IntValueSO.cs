using UnityEngine;

namespace ValueSO.Core
{
    /// <summary>
    ///     ScriptableObject container for an integer value with change notification
    /// </summary>
    [CreateAssetMenu(fileName = "IntValue", menuName = "ValueSO/Int Value")]
    public class IntValueSO : ValueSO<int>
    {
    }
}