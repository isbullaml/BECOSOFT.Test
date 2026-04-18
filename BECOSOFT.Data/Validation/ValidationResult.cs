using System.Diagnostics;
using System.Linq;
using BECOSOFT.Utilities.Extensions.Collections;

namespace BECOSOFT.Data.Validation {
    [DebuggerDisplay("Errors: {Errors.Count}, IsValid: {IsValid()}")]
    public sealed class ValidationResult<T> where T : class {
        public readonly IReadOnlyErrorList Errors;
        public readonly T ValidatedEntity;

        public ValidationResult(T entity, IErrorList errors) {
            ValidatedEntity = entity;
            Errors = errors;
        }

        public ValidationResult(T entity, IReadOnlyErrorList errors) {
            ValidatedEntity = entity;
            Errors = errors;
        }

        public ValidationResult(T entity) : this(entity, new ErrorList()) {
        }

        public bool IsValid() {
            return !Errors.HasAny();
        }

        public bool ContainsProperty(string property) {
            return Errors.Any(e => e.Property.Equals(property));
        }

        public override string ToString() {
            return $"Valid? {IsValid()} | Errors: {Errors.Count}";
        }
    }

    public static class ValidationResult {
        public static ValidationResult<T> Create<T>(T entity) where T : class {
            return new ValidationResult<T>(entity);
        }
    }
}