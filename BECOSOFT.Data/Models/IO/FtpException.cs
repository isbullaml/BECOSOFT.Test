using System;
using System.Runtime.Serialization;

namespace BECOSOFT.Data.Models.IO {
    public class FtpException : Exception {
        public FtpFile File { get; }

        public FtpException() {
            File = null;
        }

        public FtpException(FtpFile file, string message) : base(message) {
            File = file;
        }

        public FtpException(FtpFile file, string message, Exception innerException) : base(message, innerException) {
            File = file;
        }

        protected FtpException(FtpFile file, SerializationInfo info, StreamingContext context) : base(info, context) {
            File = file;
        }
    }
}