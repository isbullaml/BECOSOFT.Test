using System.Collections.Generic;

namespace BECOSOFT.Data.Validation {
    public interface ISubErrorList : IReadOnlySubErrorList {
        void Add(int index, IErrorList errors);
        void Add(int index, IReadOnlyErrorList errors);
        void AddRange(IDictionary<int, IErrorList> errors);
        void AddRange(IDictionary<int, IReadOnlyErrorList> errors);
    }
}