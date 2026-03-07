using UnityEngine;

namespace ValueSO.Core
{
    /// <summary>
    ///     ScriptableObject container for a float value with change notification
    /// </summary>
    [CreateAssetMenu(fileName = "FloatValue", menuName = "ValueSO/Float Value")]
    public class FloatValueSO : ValueSO<float>
    {
    }
}