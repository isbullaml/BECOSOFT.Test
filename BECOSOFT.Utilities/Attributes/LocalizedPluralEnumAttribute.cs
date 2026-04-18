using System;

namespace BECOSOFT.Utilities.Attributes {

    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizedPluralEnumAttribute : LocalizedAttribute {
        public LocalizedPluralEnumAttribute(string displayNameKey) : base(displayNameKey) {
        }
    }
}
