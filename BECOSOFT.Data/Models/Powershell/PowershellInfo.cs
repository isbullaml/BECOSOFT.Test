using System.Management.Automation.Runspaces;

namespace BECOSOFT.Data.Models.Powershell {
    /// <summary>
    /// Settings-object for a PS-connection
    /// </summary>
    public class PowershellInfo {
        /// <summary>
        /// The port
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// The password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// The username
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The address
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// The Powershell Shell URI
        /// </summary>
        public string PsShellUri { get; set; }

        /// <summary>
        /// The authentication mechanism
        /// </summary>
        public AuthenticationMechanism AuthenticationMechanism { get; set; }

        /// <summary>
        /// Value indicating whether to skip the Certificate Authority check (for self-signed certificates)
        /// </summary>
        public bool SkipCACheck { get; set; }

        /// <summary>
        /// Value indicating whether to skip the Common Name check (for self-signed certificates)
        /// </summary>
        public bool SkipCNCheck { get; set; }

        /// <summary>
        /// Value indicating whether encryption should be turned off
        /// </summary>
        public bool NoEncryption { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public PowershellInfo() {
            Port = 5986;
            PsShellUri = @"http://schemas.microsoft.com/powershell/Microsoft.PowerShell";
            AuthenticationMechanism = AuthenticationMechanism.Basic;
            SkipCACheck = false;
            SkipCNCheck = false;
            NoEncryption = true;
        }
    }
}