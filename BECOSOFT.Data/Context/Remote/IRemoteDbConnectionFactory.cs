using BECOSOFT.Data.Models;
using System.Data;

namespace BECOSOFT.Data.Context.Remote {
    /// <summary>
    /// Interface for a connectionfactory for remote databases
    /// </summary>
    internal interface IRemoteDbConnectionFactory {
        /// <summary>
        /// Creates a connection from SQL-Info
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <returns>The connection</returns>
        IDbConnection CreateConnection(SqlInfo sqlInfo);
    }
}