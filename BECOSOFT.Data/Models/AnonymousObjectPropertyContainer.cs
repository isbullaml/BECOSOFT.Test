using BECOSOFT.Data.Converters;
using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;
using System.Reflection;

namespace BECOSOFT.Data.Models {
    internal class AnonymousObjectPropertyContainer<T> {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly string[] BackingFieldFormats = { "<{0}>i__Field", "<{0}>", "${0}" };
        private readonly Dictionary<string, IFastDelegate> _backingFields = new Dictionary<string, IFastDelegate>();

        public AnonymousObjectPropertyContainer() {
            var type = typeof(T);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var property in properties) {
                var propName = property.Name.ToLowerInvariant();
                if (property.SetMethod == null || !property.CanWrite) {
                    foreach (var format in BackingFieldFormats) {
                        var formatted = format.FormatWith(propName);
                        var found = false;
                        foreach (var f in fields) {
                            if (!formatted.EqualsIgnoreCase(f.Name)) { continue; }
                            var fastField = new FastField(f, includeGet: false, includeSet: true);
                            _backingFields.Add(propName, fastField);
                            found = true;
                            break;
                        }
                        if (found) { break; }
                    }
                } else if(property.SetMethod != null) {
                    _backingFields.Add(propName, new FastProperty(property, false, true));
                }
            }
        }

        public IFastDelegate GetField(string propertyName) {
            IFastDelegate fieldInfo;

            if (!_backingFields.TryGetValue(propertyName.ToLowerInvariant(), out fieldInfo)) {
                return null;
                //throw new NotSupportedException($"Cannot find backing field for {propertyName}");
            }

            return fieldInfo;
        }

        public IFastDelegate GetField(EntityPropertyInfo propertyInfo) {
            return GetField(propertyInfo.PropertyName) ?? GetField(propertyInfo.ColumnName);
        }
    }
}