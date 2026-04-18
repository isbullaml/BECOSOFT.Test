using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception thrown on errors related to saving lists with an invalid count
    /// </summary>
    [Serializable]
    public class InvalidListCountException : Exception {
        public InvalidListCountException() {
        }

        public InvalidListCountException(string message) : base(message) {
        }

        public InvalidListCountException(string message, Exception innerException) : base(message, innerException) {
        }

        protected InvalidListCountException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
