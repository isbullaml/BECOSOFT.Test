using BECOSOFT.Utilities.Annotations;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception class for errors related to unsupported functionality
    /// </summary>
    public class UnsupportedException : Exception {
        public UnsupportedException() {
        }

        public UnsupportedException(string message) : base(message) {
        }

        public UnsupportedException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnsupportedException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
