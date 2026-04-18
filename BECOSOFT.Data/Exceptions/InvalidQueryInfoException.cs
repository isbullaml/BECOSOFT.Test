using BECOSOFT.Data.Models;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception thrown on errors related to an invalid <see cref="QueryInfo"/>
    /// </summary>
    [Serializable]
    public class InvalidQueryInfoException : Exception {
        public InvalidQueryInfoException() {
        }

        public InvalidQueryInfoException(string message) : base(message) {
        }

        public InvalidQueryInfoException(string message, Exception innerException) : base(message, innerException) {
        }

        protected InvalidQueryInfoException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
