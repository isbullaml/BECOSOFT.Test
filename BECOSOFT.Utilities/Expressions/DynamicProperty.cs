using System;

namespace BECOSOFT.Utilities.Expressions {
    public class DynamicProperty {
        public string Name { get; }
        public Type Type { get; }

        public DynamicProperty(string name, Type type) {
            if (name == null) { throw new ArgumentNullException(nameof(name)); }
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            Name = name;
            Type = type;
        }
    }
}