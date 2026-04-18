namespace BECOSOFT.Data.Validation {
    public class ValidationSubError {
        /// <summary>
        /// Index of the entity, always -1 when the parent entity is a single entity.
        /// </summary>
        public int Index { get; }

        public IErrorList Errors { get; }

        public ValidationSubError(IErrorList errors) : this(-1, errors) {
        }

        public ValidationSubError(int index, IErrorList errors) {
            Index = index;
            Errors = errors;
        }
    }
}