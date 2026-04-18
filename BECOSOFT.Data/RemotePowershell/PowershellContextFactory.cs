using BECOSOFT.Data.Models.Powershell;
using System.Management.Automation.Runspaces;

namespace BECOSOFT.Data.RemotePowershell {
    /// <summary>
    /// Contextfactory for remote Powershell
    /// </summary>
    internal class PowershellContextFactory : IPowershellContextFactory {
        /// <summary>
        /// The connectionfactory
        /// </summary>
        private readonly IPowershellConnectionFactory _connectionFactory;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="connectionFactory">The connectionfactory</param>
        public PowershellContextFactory(IPowershellConnectionFactory connectionFactory) {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// Creates a remote Powershell context from a Powershell-Info object
        /// </summary>
        /// <param name="powershellInfo">The Powershell-Info</param>
        /// <returns>The context</returns>
        public IPowershellContext CreateContext(PowershellInfo powershellInfo) {
            var connection = _connectionFactory.CreateConnection(powershellInfo);
            var runspace = RunspaceFactory.CreateRunspace(connection);
            runspace.Open();
            return new PowershellContext(runspace);
        }
    }
}