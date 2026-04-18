using System;

namespace BECOSOFT.Utilities.Attributes {

    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizedEnumAttribute : LocalizedAttribute {
        public LocalizedEnumAttribute(string displayNameKey) : base(displayNameKey) {
        }
    }
}
