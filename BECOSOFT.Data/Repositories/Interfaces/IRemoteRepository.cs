using BECOSOFT.Data.Models;

namespace BECOSOFT.Data.Repositories.Interfaces {
    /// <summary>
    /// Interface for a repository for executing queries on remote servers
    /// </summary>
    internal interface IRemoteRepository : IBaseRepository {
        /// <summary>
        /// Event for progress
        /// </summary>
        event RemoteRepository.ProgressEventHandler ProgressHandler;

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <param name="query">The query-object</param>
        void Query(SqlInfo sqlInfo, ParametrizedQuery query);

        /// <summary>
        /// Execute a query on a remote server with progress
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <param name="query">The query-object</param>
        void ProgressQuery(SqlInfo sqlInfo, ParametrizedQuery query);
    }
}