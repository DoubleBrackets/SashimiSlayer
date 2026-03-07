using UnityEngine;

namespace ValueSO.Core
{
    /// <summary>
    ///     ScriptableObject container for a string value with change notification
    /// </summary>
    [CreateAssetMenu(fileName = "StringValue", menuName = "ValueSO/String Value")]
    public class StringValueSO : ValueSO<string>
    {
    }
}