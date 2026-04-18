using BECOSOFT.Data.Models;
using System.Collections.Generic;
using System.Diagnostics;
using BECOSOFT.Utilities.Extensions.Collections;

namespace BECOSOFT.Data.Validation {
    [DebuggerDisplay("Invalid: {InvalidResults.Count}, Valid: {ValidResults.Count}")]
    public sealed class MultiValidationResult<T> where T : class {
        public IReadOnlyList<ValidationResult<T>> Results { get; }
        public IReadOnlyList<ValidationResult<T>> ValidResults { get; }
        public IReadOnlyList<ValidationResult<T>> InvalidResults { get; }
        public IReadonlyPrimaryKeyContainer KeyContainer { get; }

        public MultiValidationResult(IEnumerable<ValidationResult<T>> validationResults) {
            var resultList = validationResults.ToSafeList();
            var validResults = new List<ValidationResult<T>>();
            var invalidResults = new List<ValidationResult<T>>();
            foreach (var validationResult in resultList) {
                if (validationResult.IsValid()) {
                    validResults.Add(validationResult);
                } else {
                    invalidResults.Add(validationResult);
                }
            }
            Results = resultList;
            ValidResults = validResults;
            InvalidResults = invalidResults;
            KeyContainer = null;
        }
        public MultiValidationResult(IEnumerable<ValidationResult<T>> validationResults, IReadonlyPrimaryKeyContainer keyContainer) {
            var resultList = validationResults.ToSafeList();
            var validResults = new List<ValidationResult<T>>();
            var invalidResults = new List<ValidationResult<T>>();
            foreach (var validationResult in resultList) {
                if (validationResult.IsValid()) {
                    validResults.Add(validationResult);
                } else {
                    invalidResults.Add(validationResult);
                }
            }
            Results = resultList;
            ValidResults = validResults;
            InvalidResults = invalidResults;
            KeyContainer = keyContainer;
        }

        public bool IsValid() {
            return InvalidResults.IsEmpty();
        }

        public override string ToString() {
            return $"Valid? {IsValid()} | Total: {Results.Count} | Valid: {ValidResults.Count} | Invalid: {InvalidResults.Count}";
        }

        public static MultiValidationResult<T> Empty() {
            return new MultiValidationResult<T>(new List<ValidationResult<T>>(0), null);
        }
    }
}
