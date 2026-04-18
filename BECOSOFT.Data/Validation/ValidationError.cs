using System;

namespace BECOSOFT.Data.Validation {
    public class ValidationError : IEquatable<ValidationError> {
        public string Property { get; }
        public string Error { get; }
        public ISubErrorList SubErrors { get; }

        public ValidationError(string property, string error)
            : this(property, error, new SubErrorList(0)) {
        }

        public ValidationError(string property, string error, ISubErrorList subErrors) {
            Property = property;
            Error = error;
            SubErrors = subErrors;
        }

        public bool Equals(ValidationError other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Property == other.Property
                   && Error == other.Error;
        }

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) {
                return false;
            }
            if (ReferenceEquals(this, obj)) {
                return true;
            }
            if (obj.GetType() != this.GetType()) {
                return false;
            }
            return Equals((ValidationError)obj);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (Property != null ? Property.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Error != null ? Error.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(ValidationError left, ValidationError right) {
            return Equals(left, right);
        }

        public static bool operator !=(ValidationError left, ValidationError right) {
            return !Equals(left, right);
        }
    }
}