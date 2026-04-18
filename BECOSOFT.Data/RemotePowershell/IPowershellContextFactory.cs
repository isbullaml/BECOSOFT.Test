using BECOSOFT.Data.Models.Powershell;

namespace BECOSOFT.Data.RemotePowershell {
    /// <summary>
    /// Interface for a contextfactory for remote Powershell
    /// </summary>
    internal interface IPowershellContextFactory {
        /// <summary>
        /// Creates a remote Powershell context from a Powershell-Info object
        /// </summary>
        /// <param name="powershellInfo">The Powershell-Info</param>
        /// <returns>The context</returns>
        IPowershellContext CreateContext(PowershellInfo powershellInfo);
    }
}