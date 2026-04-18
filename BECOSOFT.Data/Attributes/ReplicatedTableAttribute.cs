using System;

namespace BECOSOFT.Data.Attributes {
    [AttributeUsage(AttributeTargets.Class)]
    public class ReplicatedTableAttribute : Attribute {
        /// <summary>
        /// Indicates that this table can be replicated and is not required
        /// </summary>
        public bool IsOptional { get; set; }
    }
}