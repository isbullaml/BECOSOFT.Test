using BECOSOFT.Data.Models;

namespace BECOSOFT.Data.Context.Remote {
    /// <summary>
    /// Interface for a contextfactory for remote databases
    /// </summary>
    internal interface IRemoteDbContextFactory {
        /// <summary>
        /// Creates a context from SQL-info
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <returns>The context</returns>
        IRemoteDbContext CreateContext(SqlInfo sqlInfo);
    }
}