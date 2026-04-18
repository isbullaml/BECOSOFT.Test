using BECOSOFT.Utilities.Annotations;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception thrown when missing a reader
    /// </summary>
    [Serializable]
    public class MissingReaderException : Exception {
        public MissingReaderException() {
        }

        public MissingReaderException(string message) : base(message) {
        }

        public MissingReaderException(string message, Exception innerException) : base(message, innerException) {
        }

        protected MissingReaderException([NotNull] SerializationInfo info, StreamingContext context)
            : base(info, context) {
        }
    }
}