using System;
using System.Reflection;

namespace BECOSOFT.Data.Validation.Attributes {
    public class LinkedEntityValidationAttribute : Attribute, IValidationAttribute {
        private Func<string> _propertyResourceAccessor;
        private string _propertyResourceName;
        private Type _propertyResourceType;

        public Type LinkedEntityType { get; }
        public bool AllowZeroID { get; set; }
        public bool SkipOnUpdate { get; set; }

        public string PropertyResourceName {
            get { return _propertyResourceName; }
            set {
                _propertyResourceName = value;
                _propertyResourceAccessor = null;
                SetPropertyResourceAccessor();
            }
        }

        public Type PropertyResourceType {
            get { return _propertyResourceType; }
            set {
                _propertyResourceType = value;
                _propertyResourceAccessor = null;
                SetPropertyResourceAccessor();

            }
        }

        public LinkedEntityValidationAttribute(Type linkedEntityType) {
            LinkedEntityType = linkedEntityType;
        }

        public string GetPropertyResource() {
            if (_propertyResourceAccessor != null) {
                return _propertyResourceAccessor();
            }
            SetPropertyResourceAccessor();
            if (_propertyResourceAccessor == null) {
                return null;
            }
            return _propertyResourceAccessor();
        }

        private void SetPropertyResourceAccessor() {
            if (_propertyResourceType == null || _propertyResourceName == null) {
                return;
            }
            var property = _propertyResourceType.GetProperty(_propertyResourceName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (property == null) {
                return;
            }
            var getMethod = property.GetGetMethod(true);
            if (getMethod == null || !getMethod.IsAssembly && !getMethod.IsPublic) {
                return;
            }
            _propertyResourceAccessor = () => (string)property.GetValue(null);
        }
    }
}