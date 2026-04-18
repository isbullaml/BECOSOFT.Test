using BECOSOFT.Utilities.Annotations;
using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    public class UnmergableParametrizedQueryException : Exception {
        public UnmergableParametrizedQueryException() {
        }

        public UnmergableParametrizedQueryException(string message) : base(message) {
        }

        public UnmergableParametrizedQueryException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UnmergableParametrizedQueryException([NotNull] SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}