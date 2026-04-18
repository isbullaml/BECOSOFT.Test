namespace BECOSOFT.Data.Validation {
    public interface IErrorList : IReadOnlyErrorList {
        void Add(ValidationError error);
        ValidationError Add(string property, string error);
        ValidationError Add(string property, string error, ISubErrorList subErrors);
        void AddRange(IReadOnlyErrorList errors);
        bool Remove(ValidationError error);
    }
}