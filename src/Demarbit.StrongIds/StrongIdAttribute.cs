using System;

namespace Demarbit.StrongIds
{
    /// <summary>
    /// StrongId attribute definition
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class StrongIdAttribute : Attribute
    {
        /// <summary>
        /// The backing type of the StrongId
        /// </summary>
        public BackingType BackingType { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="backingType"></param>
        public StrongIdAttribute(BackingType backingType = BackingType.Guid)
        {
            BackingType = backingType;
        }
    }
}