using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Utilities.Exceptions {
    /// <summary>
    /// Exception thrown when performing actions on an empty collection
    /// </summary>
    [Serializable]
    public class EmptyCollectionException : Exception {
        public EmptyCollectionException() {
        }

        public EmptyCollectionException(string message) : base(message) {
        }

        public EmptyCollectionException(string message, Exception innerException) : base(message, innerException) {
        }

        protected EmptyCollectionException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}