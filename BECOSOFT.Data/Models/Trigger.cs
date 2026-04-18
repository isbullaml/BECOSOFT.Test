using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.Data.Models {
    public class Trigger {
        private readonly string _name;
        private readonly string _placeholder;
        private readonly Func<ISqlScript> _resourceFunc;

        public string GetName(string value) {
            if (!HasTablePart) {
                return _name;
            }
            if (!_name.Contains(_placeholder)) {
                throw new ArgumentException("Placeholder value is not present in the trigger name", nameof(_placeholder));
            }
            if (value.IsNullOrWhiteSpace()) {
                throw new ArgumentException("Value is null or empty.", nameof(value));
            }
            return _name.Replace(_placeholder, value);
        }

        public string GetDefinition(string value) {
            var definition = _resourceFunc().GetQuery();
            if (!HasTablePart) {
                return definition;
            }
            if (!_name.Contains(_placeholder)) {
                throw new ArgumentException("Placeholder value is not present in the trigger definition", nameof(_placeholder));
            }
            if (value.IsNullOrWhiteSpace()) {
                throw new ArgumentException("Value is null or empty.", nameof(value));
            }
            return definition.Replace(_placeholder, value);
        }

        public bool HasTablePart { get; }

        private Trigger(string name, Func<ISqlScript> resourceFunc, string placeholder = null) {
            _name = name;
            _resourceFunc = resourceFunc;
            _placeholder = placeholder;
            HasTablePart = !placeholder.IsNullOrWhiteSpace();
        }

        /// <summary>
        /// Create a <see cref="Trigger"/>-object.
        /// </summary>
        /// <param name="name">Name of the trigger</param>
        /// <param name="scriptFunc">Script retrieval function</param>
        /// <param name="placeholder">Specify the placeholder in the trigger name (will be used to replace the actual dynamic value)</param>
        /// <returns></returns>
        public static Trigger Create(string name, Func<ISqlScript> scriptFunc, string placeholder = null) {
            return new Trigger(name, scriptFunc, placeholder);
        }
    }
}