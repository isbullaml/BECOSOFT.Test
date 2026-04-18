using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Utilities.Exceptions {
    /// <summary>
    /// Exception thrown on errors related to invalid access
    /// </summary>
    [Serializable]
    public class DbAccessException : Exception {
        public DbAccessException() {
        }

        public DbAccessException(string message) : base(message) {
        }

        public DbAccessException(string message, Exception innerException) : base(message, innerException) {
        }

        protected DbAccessException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}