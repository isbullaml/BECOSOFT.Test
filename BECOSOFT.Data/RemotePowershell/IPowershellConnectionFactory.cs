using BECOSOFT.Data.Models.Powershell;
using System.Management.Automation.Runspaces;

namespace BECOSOFT.Data.RemotePowershell {
    /// <summary>
    /// Interface for a connectionfactory for remote Powershell
    /// </summary>
    internal interface IPowershellConnectionFactory {
        /// <summary>
        /// Creates a remote Powershell connection from a Powershell-Info object
        /// </summary>
        /// <param name="powershellInfo">The Powershell-Info</param>
        /// <returns>The connection</returns>
        WSManConnectionInfo CreateConnection(PowershellInfo powershellInfo);
    }
}