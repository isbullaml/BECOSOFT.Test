using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception thrown on errors related to failing conversion of a result
    /// </summary>
    [Serializable]
    public class FailedResultConversionException : Exception {
        public FailedResultConversionException() {
        }

        public FailedResultConversionException(string message) : base(message) {
        }

        public FailedResultConversionException(string message, Exception innerException) : base(message, innerException) {
        }

        protected FailedResultConversionException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}