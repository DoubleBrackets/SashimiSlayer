using UnityEngine;

namespace ValueSO.Core
{
    /// <summary>
    ///     ScriptableObject container for a boolean value with change notification
    /// </summary>
    [CreateAssetMenu(fileName = "BoolValue", menuName = "ValueSO/Bool Value")]
    public class BoolValueSO : ValueSO<bool>
    {
    }
}