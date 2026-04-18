using System;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Container-class for a parameter
    /// </summary>
    internal class ParameterContainer {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The type of the parameter
        /// </summary>
        public Type Type { get; set; }
        /// <summary>
        /// The value of the parameter
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// Checks if two <see cref="ParameterContainer"/>-objects are equal
        /// </summary>
        /// <param name="other">The other container</param>
        /// <returns>True if they are equal, false if they are not equal</returns>
        protected bool Equals(ParameterContainer other) {
            return Type == other.Type && Equals(Value, other.Value);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }

            if (ReferenceEquals(this, obj)) {
                return true;
            }

            if (obj.GetType() != GetType()) {
                return false;
            }

            return Equals((ParameterContainer) obj);
        }

        /// <inheritdoc />
        public override int GetHashCode() {
            unchecked {
                return ((Type != null ? Type.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }
    }
}
