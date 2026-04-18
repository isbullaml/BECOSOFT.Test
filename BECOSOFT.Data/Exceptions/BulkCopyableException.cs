using BECOSOFT.Utilities.Annotations;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception class for errors related to bulk copyable functionality
    /// </summary>
    public class BulkCopyableException : Exception {
        public BulkCopyableException() {
        }

        public BulkCopyableException(string message) : base(message) {
        }

        public BulkCopyableException(string message, Exception innerException) : base(message, innerException) {
        }

        protected BulkCopyableException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}