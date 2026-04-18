using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public sealed class ErrorList : List<ValidationError>, IErrorList {
        public ValidationError Add(string property, string error) {
            var validationError = new ValidationError(property, error);
            Add(validationError);
            return validationError;
        }

        public ValidationError Add(string property, string error, ISubErrorList subErrors) {
            var validationError = new ValidationError(property, error, subErrors);
            Add(validationError);
            return validationError;
        }

        public void AddRange(IReadOnlyErrorList errors) {
            base.AddRange(errors);
        }

        public IReadOnlyList<ValidationError> GetAllErrors() {
            var result = new List<ValidationError>();

            foreach (var validationError in this) {
                result.Add(new ValidationError(validationError.Property, validationError.Error));
                AddErrors(validationError.Property, validationError, result);
            }

            return result;
        }

        private static void AddErrors(string parentProperty, ValidationError error, List<ValidationError> result) {
            if (error.SubErrors.IsEmpty()) { return; }
            foreach (var subError in error.SubErrors) {
                var property = subError.Index == -1 ? parentProperty : $"{parentProperty}[{subError.Index}]";
                foreach (var validationError in subError.Errors) {
                    var prop = $"{property}.{validationError.Property}";
                    result.Add(new ValidationError(prop, validationError.Error));
                    AddErrors(validationError.Property, validationError, result);
                }
            }
        }
    }
}