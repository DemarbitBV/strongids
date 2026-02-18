using System;

namespace Demarbit.StrongIds
{
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public class StrongIdAttribute : Attribute
    {
        public BackingType BackingType { get; }

        public StrongIdAttribute(BackingType backingType = BackingType.Guid)
        {
            BackingType = backingType;
        }
    }
}