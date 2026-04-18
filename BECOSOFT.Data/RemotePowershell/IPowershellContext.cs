using BECOSOFT.Data.Models.Powershell;
using System;

namespace BECOSOFT.Data.RemotePowershell {
    /// <summary>
    /// Interface for context for executing Powershell queries on remote servers
    /// </summary>
    internal interface IPowershellContext : IDisposable {
        /// <summary>
        /// Event for progress
        /// </summary>
        event PowershellContext.ProgressEventHandler ProgressHandler;

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="script">The script-object</param>
        /// <param name="withProgress">Value indicating whether progress-event should be called</param>
        PowershellResult Execute(PowershellScript script, bool withProgress = false);
    }
}