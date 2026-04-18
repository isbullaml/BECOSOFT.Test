using System;

namespace BECOSOFT.Data.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class IgnoreCopyAttribute : Attribute {
        /// <summary>
        /// Call constructor of object.
        /// By default, the property is default initialised.
        /// </summary>
        public bool CallConstructor { get; set; } = true;
    }
}
