using BECOSOFT.Data.Validation.Attributes;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Migrator {
    public class MigrationException : Exception {
        public MigrationException() {
        }

        public MigrationException(string message) : base(message) {
        }

        public MigrationException(string message, Exception innerException) : base(message, innerException) {
        }

        protected MigrationException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}