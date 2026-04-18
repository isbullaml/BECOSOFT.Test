using BECOSOFT.Utilities.Annotations;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    public class UnknownSchemaException : Exception {
        public UnknownSchemaException() {
        }

        public UnknownSchemaException(string message) : base(message) {
        }

        public UnknownSchemaException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnknownSchemaException([NotNull] SerializationInfo info, StreamingContext context) : base(info,
            context) {
        }
    }
}