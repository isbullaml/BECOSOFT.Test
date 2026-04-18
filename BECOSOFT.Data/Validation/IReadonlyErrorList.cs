using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public interface IReadOnlyErrorList : IReadOnlyList<ValidationError> {
        bool Contains(ValidationError error);

        /// <summary>
        /// Returns all <see cref="ValidationError"/> objects and <see cref="ValidationSubError"/> objects as a single list of <see cref="ValidationError"/>.
        /// </summary>
        /// <returns></returns>
        IReadOnlyList<ValidationError> GetAllErrors();
    }
    public interface IReadOnlySubErrorList : IReadOnlyList<ValidationSubError> {
    }
}
