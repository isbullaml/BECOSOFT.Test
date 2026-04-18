using BECOSOFT.Data.Context.Remote;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;

namespace BECOSOFT.Data.Repositories {
    /// <summary>
    /// Repository for executing queries on remote servers
    /// </summary>
    internal class RemoteRepository : IRemoteRepository {
        /// <summary>
        /// Factory for creating DB-contexts
        /// </summary>
        private readonly IRemoteDbContextFactory _remoteDbContextFactory;

        /// <summary>
        /// Updates progress
        /// </summary>
        /// <param name="percentage">Percentage completed</param>
        public delegate void ProgressEventHandler(int percentage);

        internal RemoteRepository(IRemoteDbContextFactory remoteDbContextFactory) {
            _remoteDbContextFactory = remoteDbContextFactory;
        }

        /// <summary>
        /// Event for progress
        /// </summary>
        public event ProgressEventHandler ProgressHandler;

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <param name="query">The query-object</param>
        public void Query(SqlInfo sqlInfo, ParametrizedQuery query) {
            using (var context = GetContext(sqlInfo)) {
                context.Query(query);
            }
        }

        /// <summary>
        /// Execute a query on a remote server with progress
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <param name="query">The query-object</param>
        public void ProgressQuery(SqlInfo sqlInfo, ParametrizedQuery query) {
            using (var context = GetContext(sqlInfo)) {
                context.ProgressHandler += UpdateProgress;
                context.Query(query);
                context.ProgressHandler -= UpdateProgress;
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
        /// Retrieves a DB-context
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <returns>The context</returns>
        private IRemoteDbContext GetContext(SqlInfo sqlInfo) {
            return _remoteDbContextFactory.CreateContext(sqlInfo);
        }
    }
}