using BECOSOFT.Data.Converters;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BECOSOFT.Data.Models.Base {
    /// <summary>
    /// Filter class contains basic functionality to keep track whether the properties of the class have been set.
    /// </summary>
    public abstract class Filter : IValidatable {
        private bool _isSet;
        private HashSet<string> _setProperties;

        /// <summary>
        /// IsSet tells the caller if any of the observed properties are set.
        /// </summary>
        public bool IsSet {
            get => _isSet || HasSetChildren();
            protected set => _isSet = value;
        }

        /// <summary>
        /// Set the new value of the property. If the value is different from the current, the value will be set and IsSet will be set to true
        /// </summary>
        /// <typeparam name="T">Property type</typeparam>
        /// <param name="field">Property</param>
        /// <param name="newValue">New value</param>
        protected void SetPropertyField<T>(ref T field, T newValue, [CallerMemberName] string callerName = "") {
            if (EqualityComparer<T>.Default.Equals(field, newValue)) {
                return;
            }
            field = newValue;
            if (_setProperties == null) {
                _setProperties = new HashSet<string>();
            }
            if (EqualityComparer<T>.Default.Equals(newValue, default)) {
                _setProperties.Remove(callerName);
            } else {
                _setProperties.Add(callerName);
            }
            IsSet = true;
        }

        public IReadOnlyCollection<string> GetSetProperties() {
            return _setProperties ?? new HashSet<string>(0);
        }

        public void ClearSetProperties() {
            _setProperties?.Clear();
        }

        public virtual bool IsValid() {
            return IsSet;
        }

        private bool HasSetChildren() {
            var info = EntityConverter.GetEntityTypeInfo(GetType());
            foreach (var property in info.Properties.Where(p => p.IsFilter)) {
                var childType = EntityConverter.GetEntityTypeInfo(property.PropertyType);
                if (!childType.IsFilter) { continue; }
                var baseChild = (Filter) property.Getter(this);
                if (baseChild?.IsSet ?? false) {
                    return true;
                }
            }

            return false;
        }
    }
}
