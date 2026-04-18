using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Exceptions {
    /// <summary>
    /// Exception thrown when the table-definition does not exist
    /// </summary>
    [Serializable]
    public class UndefinedTableDefinitionException : Exception {
        public UndefinedTableDefinitionException() {
        }

        public UndefinedTableDefinitionException(string message) : base(message) {
        }

        public UndefinedTableDefinitionException(string message, Exception innerException) : base(message, innerException) {
        }

        protected UndefinedTableDefinitionException(SerializationInfo info, StreamingContext context) : base(info, context) {
        }
    }
}
