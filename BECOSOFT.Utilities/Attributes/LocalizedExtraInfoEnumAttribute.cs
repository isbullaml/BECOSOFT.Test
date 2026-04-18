using System;
using System.ComponentModel;
using System.Reflection;

namespace BECOSOFT.Utilities.Attributes {
    /// <inheritdoc />
    [AttributeUsage(AttributeTargets.Field)]
    public class LocalizedExtraInfoEnumAttribute : Attribute {
        private readonly string _name;
        private readonly string _info;
        private PropertyInfo _nameProperty;
        private PropertyInfo _infoProperty;
        private Type _resourceType;

        /// <summary>
        /// Localized attribute for enums with extra info
        /// </summary>
        /// <param name="name"></param>
        /// <param name="info"></param>
        internal LocalizedExtraInfoEnumAttribute(string name, string info) {
            _name = name;
            _info = info;
        }

        /// <summary>
        /// The name of the resource type
        /// </summary>
        public Type ResourceType {
            get { return _resourceType; }
            set {
                _resourceType = value;
                _nameProperty = _resourceType.GetProperty(_name,
                                                          BindingFlags.Static | BindingFlags.Public |
                                                          BindingFlags.NonPublic);
                _infoProperty = _resourceType.GetProperty(_info,
                                                          BindingFlags.Static | BindingFlags.Public |
                                                          BindingFlags.NonPublic);
            }
        }

        public string DisplayName => _nameProperty == null
                                                  ? _name
                                                  : (string)_nameProperty.GetValue(_nameProperty.DeclaringType, null);

        public string DisplayInfo => _infoProperty == null
                                         ? _info
                                         : (string)_infoProperty.GetValue(_infoProperty.DeclaringType, null);
    }
}