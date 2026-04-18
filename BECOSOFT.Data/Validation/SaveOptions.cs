using BECOSOFT.Data.Exceptions;
using System;

namespace BECOSOFT.Data.Validation {
    public abstract class SaveOptions : ISaveOptions {
        /// <summary>
        /// Implement this method to provide default options.
        /// Return an instance of the implemented type.
        /// </summary>
        /// <returns></returns>
        protected abstract SaveOptions GetInstanceWithDefaultOptions();

        public virtual ISaveOptions GetSubOptions(Type subType) {
            return null;
        }

        public SaveOptions GetDefault() {
            var instance = GetInstanceWithDefaultOptions();
            if (instance.GetType() != GetType()) {
                throw new SaveOptionsTypeMismatchException();
            }
            return instance;
        }
    }
}