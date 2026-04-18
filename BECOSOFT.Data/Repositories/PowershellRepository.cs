using BECOSOFT.Data.Models.Powershell;
using BECOSOFT.Data.RemotePowershell;
using BECOSOFT.Data.Repositories.Interfaces;

namespace BECOSOFT.Data.Repositories {
    /// <summary>
    /// Repository for executing PS-scripts on remote servers
    /// </summary>
    internal class PowershellRepository : IPowershellRepository {
        private readonly IPowershellContextFactory _psContextFactory;

        /// <summary>
        /// Updates progress
        /// </summary>
        /// <param name="percentage">Percentage completed</param>
        public delegate void ProgressEventHandler(int percentage);

        internal PowershellRepository(IPowershellContextFactory psContextFactory) {
            _psContextFactory = psContextFactory;
        }

        /// <summary>
        /// Event for progress
        /// </summary>
        public event ProgressEventHandler ProgressHandler;

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="powershellInfo">The Powershell-Info object</param>
        /// <param name="script">The script-object</param>
        /// <param name="withProgress">Value indicating whether progress-event should be called</param>
        public PowershellResult Execute(PowershellInfo powershellInfo, PowershellScript script, bool withProgress = false) {
            using (var context = GetContext(powershellInfo)) {
                if (withProgress) {
                    context.ProgressHandler += UpdateProgress;
                }
                return context.Execute(script, withProgress);
            }
        }

        /// <summary>
        /// Updates progress
        /// </summary>
        /// <param name="percentage">Percentage completed</param>
        private void UpdateProgress(int percentage) {
            ProgressHandler?.Invoke(percentage);
        }

        /// <summary>
        /// Retrieves a PS-context
        /// </summary>
        /// <param name="powershellInfo">The Powershell-Info object</param>
        /// <returns>The context</returns>
        private IPowershellContext GetContext(PowershellInfo powershellInfo) {
            return _psContextFactory.CreateContext(powershellInfo);
        }
    }
}