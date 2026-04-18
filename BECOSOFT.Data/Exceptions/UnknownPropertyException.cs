using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception thrown when the property is not known
    /// </summary>
    [Serializable]
    public class UnknownPropertyException : Exception {
        public UnknownPropertyException() {
        }

        public UnknownPropertyException(string message) : base(message) {
        }

        public UnknownPropertyException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnknownPropertyException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}