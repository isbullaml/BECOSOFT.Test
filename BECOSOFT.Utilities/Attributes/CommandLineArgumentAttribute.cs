using System;

namespace BECOSOFT.Utilities.Attributes {
    [AttributeUsage(AttributeTargets.Property)]
    public class CommandLineArgumentAttribute : Attribute {
        public string ArgumentName { get; }

        public CommandLineArgumentAttribute(string argumentName) {
            ArgumentName = argumentName;
        }
    }
}