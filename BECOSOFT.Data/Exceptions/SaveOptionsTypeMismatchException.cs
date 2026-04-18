using BECOSOFT.Utilities.Annotations;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    public class SaveOptionsTypeMismatchException : Exception {
        public SaveOptionsTypeMismatchException() {
        }

        public SaveOptionsTypeMismatchException(string message) : base(message) {
        }

        public SaveOptionsTypeMismatchException(string message, Exception innerException) : base(message, innerException) {
        }

        protected SaveOptionsTypeMismatchException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}