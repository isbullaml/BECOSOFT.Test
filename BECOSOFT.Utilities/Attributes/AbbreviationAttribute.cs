using System;

namespace BECOSOFT.Utilities.Attributes {
    [AttributeUsage(AttributeTargets.Field)]
    public class AbbreviationAttribute : Attribute {
        public AbbreviationAttribute(string abbreviation) {
            Abbreviation = abbreviation;
        }

        public string Abbreviation { get; }
    }
}