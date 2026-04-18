using System;

namespace BECOSOFT.Data.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class BulkCopyableColumnAttribute : Attribute {
        public int Index { get; }

        public BulkCopyableColumnAttribute(int index) {
            Index = index;
        }
    }
}