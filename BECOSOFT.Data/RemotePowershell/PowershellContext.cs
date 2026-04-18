using BECOSOFT.Data.Models.Powershell;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace BECOSOFT.Data.RemotePowershell {
    /// <summary>
    /// Context for executing Powershell queries on remote servers
    /// </summary>
    internal class PowershellContext : IPowershellContext {
        /// <summary>
        /// The runspace
        /// </summary>
        private Runspace Runspace { get; }

        /// <summary>
        /// Updates progress
        /// </summary>
        /// <param name="percentage">Percentage completed</param>
        public delegate void ProgressEventHandler(int percentage);

        /// <summary>
        /// Event for progress
        /// </summary>
        public event ProgressEventHandler ProgressHandler;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="runspace">The runspace</param>
        public PowershellContext(Runspace runspace) {
            Runspace = runspace;
        }

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="script">The script-object</param>
        /// <param name="withProgress">Value indicating whether progress-event should be called</param>
        public PowershellResult Execute(PowershellScript script, bool withProgress = false) {
            using (var psh = PowerShell.Create()) {
                psh.Runspace = Runspace;
                if (withProgress) {
                    psh.Streams.Progress.DataAdded += UpdateProgress;
                }
                psh.AddScript(script.Script).AddParameters(script.Parameters);
                var results = psh.Invoke();
                return new PowershellResult {
                    Objects = results.ToList(),
                };
            }
        }

        /// <summary>
        /// Updates progress
        /// </summary>
        /// <param name="sender">The object</param>
        /// <param name="eventArgs">The event arguments</param>
        private void UpdateProgress(object sender, DataAddedEventArgs eventArgs) {
            var progress = ((PSDataCollection<ProgressRecord>) sender)[eventArgs.Index];
            if (progress.PercentComplete != -1) {
                ProgressHandler?.Invoke(progress.PercentComplete);
            }
        }

        /// <summary>
        /// Dispose the context
        /// </summary>
        /// <param name="disposing">Value indiciating whether it is disposing</param>
        private void Dispose(bool disposing) {
            if (disposing) {
                Runspace?.Dispose();
            }
        }

        /// <summary>
        /// Dispose the context
        /// </summary>
        public void Dispose() {
            Dispose(true);
        }
    }
}
