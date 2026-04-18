using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public sealed class SubErrorList : List<ValidationSubError>, ISubErrorList {
        public SubErrorList() {
        }

        public SubErrorList(int capacity) : base(capacity) {
        }

        /// <summary>
        /// Adds the <paramref name="errors"/> to the current <see cref="SubErrorList"/>, if the <paramref name="index"/> is already present, the <paramref name="errors"/> are appended.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="errors"></param>
        public void Add(int index, IErrorList errors) {
            var existingIndex = FindIndex(se => se.Index == index);
            if (existingIndex == -1) {
                var subError = new ValidationSubError(index, errors);
                Add(subError);
            } else {
                this[existingIndex].Errors.AddRange(errors);
            }
        }

        /// <summary>
        /// Adds the <paramref name="errors"/> to the current <see cref="SubErrorList"/>, if the <paramref name="index"/> is already present, the <paramref name="errors"/> are appended.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="errors"></param>
        public void Add(int index, IReadOnlyErrorList errors) {
            var errorList = new ErrorList();
            errorList.AddRange(errors);
            Add(index, errorList);
        }

        /// <summary>
        /// Adds the <paramref name="errors"/> to the current <see cref="SubErrorList"/>, if the index is already present, the <paramref name="errors"/> are appended.
        /// </summary>
        /// <param name="errors">Dictionary of index and error lists</param>
        public void AddRange(IDictionary<int, IErrorList> errors) {
            foreach (var pair in errors) {
                Add(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Adds the <paramref name="errors"/> to the current <see cref="SubErrorList"/>, if the index is already present, the <paramref name="errors"/> are appended.
        /// </summary>
        /// <param name="errors">Dictionary of index and error lists</param>
        public void AddRange(IDictionary<int, IReadOnlyErrorList> errors) {
            foreach (var pair in errors) {
                var errorList = pair.Value as IErrorList;
                if (errorList == null) {
                    errorList = new ErrorList();
                    errorList.AddRange(pair.Value);
                }
                Add(pair.Key, errorList);
            }
        }
    }
}