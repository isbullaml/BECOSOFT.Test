using BECOSOFT.Data.Models;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception thrown when the <see cref="QueryType"/> is not known
    /// </summary>
    [Serializable]
    public class UnknownQueryTypeException : Exception {
        public UnknownQueryTypeException() {
        }

        public UnknownQueryTypeException(string message) : base(message) {
        }

        public UnknownQueryTypeException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnknownQueryTypeException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
