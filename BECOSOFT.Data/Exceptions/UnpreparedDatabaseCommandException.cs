using BECOSOFT.Utilities.Annotations;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    public class UnpreparedDatabaseCommandException : Exception {
        public UnpreparedDatabaseCommandException() {
        }

        public UnpreparedDatabaseCommandException(string message) : base(message) {
        }

        public UnpreparedDatabaseCommandException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnpreparedDatabaseCommandException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
