using System;
using UnityEngine;

namespace EditorUtils.BoldHeader
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
    public class BoldHeaderAttribute : PropertyAttribute
    {
        /// <summary>
        ///     <para>The header text.</para>
        /// </summary>
        public readonly string header;

        /// <summary>
        ///     <para>Add a bold header and line above some fields in the Inspector.</para>
        /// </summary>
        /// <param name="header">The header text.</param>
        public BoldHeaderAttribute(string header)
        {
            this.header = header;
        }
    }
}