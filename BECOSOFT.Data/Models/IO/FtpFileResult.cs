using BECOSOFT.Utilities.Extensions;
using System;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.IO {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpFileResult {
        private bool _success;
        private string _error;
        public FtpFile File { get; }

        public string Error {
            get => _error;
            set {
                Timestamp = DateTime.Now;
                Success = value.IsNullOrWhiteSpace();
                _error = value;
            }
        }

        public bool Success {
            get => _success;
            set {
                Timestamp = DateTime.Now;
                _success = value;
            }
        }

        public DateTime Timestamp { get; private set; }

        public FtpFileResult(FtpFile file) {
            File = file;
        }

        private string DebuggerDisplay => $"File: {File.ServerFile}, Timestamp: {Timestamp:dd/MM/yyyy HH:mm:ss.fff}, Success? {Success}, Error: {Error}";
    }
}