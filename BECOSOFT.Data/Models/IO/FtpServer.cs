using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Data.Models.IO {
    /// <summary>
    /// <see cref="FtpServer"/> contains the connection properties for an FTP-server.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class FtpServer {
        private int? _port;
        private const int DefaultFtpPort = 21;
        private const int DefaultSftpPort = 22;
        public FtpServerType ServerType { get; set; }
        public string Host { get; set; }

        /// <summary>
        /// Returns the value of <see cref="_port"/> (if it has a value set) else it returns the <see cref="DefaultFtpPort"/> or <see cref="DefaultSftpPort"/>, depending on the <see cref="ServerType"/>.
        /// The setter of this property sets the <see cref="_port"/> (no extra logic).
        /// </summary>
        public int Port {
            get {
                if (_port.HasValue) {
                    return _port.Value;
                }
                return ServerType == FtpServerType.Ftp ? DefaultFtpPort : DefaultSftpPort;
            }
            set => _port = value;
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public List<string> PrivateKeyFiles { get; set; }
        public string RootFolder { get; set; } = "";
        public bool EnableSsl { get; set; }
        public bool UseActive { get; set; }

        public FtpServer(string host, string username, string password) {
            Host = host;
            Username = username;
            Password = password;
            PrivateKeyFiles = new List<string>();
        }

        public override string ToString() {
            return $"{ServerType}, '{Host}':{Port}, Root: {RootFolder}, User: {Username}, SSL? {EnableSsl}, UseActive? {UseActive}";
        }

        private string DebuggerDisplay => ToString();
    }

    public enum FtpServerType {
        Ftp,
        Sftp
    }
}