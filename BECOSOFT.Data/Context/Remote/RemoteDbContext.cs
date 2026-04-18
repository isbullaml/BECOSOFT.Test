using BECOSOFT.Data.Models;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Services.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace BECOSOFT.Data.Context.Remote
{
    /// <summary>
    /// Context for executing SQL queries on remote servers
    /// </summary>
    internal sealed class RemoteDbContext : DbContext, IRemoteDbContext {
        private readonly IQueryBuilderFactory _queryBuilderFactory;
        private readonly IDatabaseCommandFactory _databaseCommandFactory;

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
        /// <param name="dbConnection">The connection</param>
        /// <param name="queryBuilderFactory"></param>
        /// <param name="databaseCommandFactory"></param>
        /// <param name="bulkCopyHelper"></param>
        public RemoteDbContext(IDbConnection dbConnection,
                               IQueryBuilderFactory queryBuilderFactory, 
                               IDatabaseCommandFactory databaseCommandFactory,
                               IBulkCopyHelper bulkCopyHelper)
            : base(dbConnection, bulkCopyHelper){
            _queryBuilderFactory = queryBuilderFactory;
            _databaseCommandFactory = databaseCommandFactory;
        }

        /// <summary>
        /// Execute a query on a remote server
        /// </summary>
        /// <param name="query">The query-object</param>
        public void Query(ParametrizedQuery query) {
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Custom, new QueryInfo {
                PremadeQuery = query.Query,
                ParameterList = query.Parameters
            });
            var command = _databaseCommandFactory.Build(builder);
            command.CommandTimeout = query.Timeout;
            ExecuteNonQuery(command);
        }

        /// <summary>
        /// Execute a query on a remote server with progress
        /// </summary>
        /// <param name="query">The query-object</param>
        public void ProgressQuery(ParametrizedQuery query) {
            var builder = _queryBuilderFactory.GetBuilder(QueryType.Custom, new QueryInfo {
                PremadeQuery = query.Query,
                ParameterList = query.Parameters
            });
            var command = _databaseCommandFactory.Build(builder);
            command.CommandTimeout = query.Timeout;
            command.FireInfoMessageEventOnUserErrors = true;
            command.InfoMessageHandler = UpdateProgress;
            ExecuteNonQuery(command);
            command.InfoMessageHandler = UpdateProgress;
        }

        /// <summary>
        /// Updates progress
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="eventArgs">The event arguments</param>
        private void UpdateProgress(object sender, SqlInfoMessageEventArgs eventArgs) {
            var progress = eventArgs.Errors[0];
            if (progress.Number != 5701) {
                ProgressHandler?.Invoke(progress.State);
            }
        }
    }
}