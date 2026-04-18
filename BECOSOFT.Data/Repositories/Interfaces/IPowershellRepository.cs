using BECOSOFT.Data.Models.Powershell;

namespace BECOSOFT.Data.Repositories.Interfaces {
    /// <summary>
    /// Repository for executing Powershell scripts
    /// </summary>
    internal interface IPowershellRepository : IBaseRepository {
        /// <summary>
        /// Event for progress
        /// </summary>
        event PowershellRepository.ProgressEventHandler ProgressHandler;

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="powershellInfo">The Powershell-Info object</param>
        /// <param name="script">The script-object</param>
        /// <param name="withProgress">Value indicating whether progress-event should be called</param>
        PowershellResult Execute(PowershellInfo powershellInfo, PowershellScript script, bool withProgress = false);
    }
}