using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Utilities.Exceptions {
    /// <summary>
    /// Exception thrown on errors related to a failing database-connection
    /// </summary>
    [Serializable]
    public class DbConnectionException : Exception {
        public DbConnectionException() {
        }

        public DbConnectionException(string message) : base(message) {
        }

        public DbConnectionException(string message, Exception innerException) : base(message, innerException) {
        }

        protected DbConnectionException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}