using System;
using System.ComponentModel;
using System.Reflection;

namespace BECOSOFT.Utilities.Attributes {
    /// <summary>
    /// Attribute indicating the enum has translations
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizedAttribute : DescriptionAttribute {
        private PropertyInfo _nameProperty;
        private Type _resourceType;

        private static readonly BindingFlags BindingFlags = BindingFlags.Static | BindingFlags.Public |
                                                            BindingFlags.NonPublic;

        /// <summary>
        /// The display name of the key
        /// </summary>
        public string DisplayNameKey => DescriptionValue;

        public LocalizedAttribute(string displayNameKey) : base(displayNameKey) {
        }

        /// <summary>
        /// The name of the resource type
        /// </summary>
        public Type NameResourceType {
            get => _resourceType;
            set {
                _resourceType = value;
                _nameProperty = _resourceType.GetProperty(Description, BindingFlags);
            }
        }

        /// <summary>
        /// The description of the resource type
        /// </summary>
        public override string Description {
            get {
                if (_nameProperty == null) {
                    return base.Description;
                }
                return (string)_nameProperty.GetValue(_nameProperty.DeclaringType, null);
            }
        }
    }
}