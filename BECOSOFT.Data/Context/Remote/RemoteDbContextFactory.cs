using BECOSOFT.Data.Models;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Context.Remote
{
    /// <summary>
    /// Contextfactory for remote databases
    /// </summary>
    internal class RemoteDbContextFactory : IRemoteDbContextFactory {
        /// <summary>
        /// The connection factory
        /// </summary>
        private readonly IRemoteDbConnectionFactory _connectionFactory;

        private readonly IQueryBuilderFactory _queryBuilderFactory;

        private readonly IDatabaseCommandFactory _databaseCommandFactory;
        private readonly IBulkCopyHelper _bulkCopyHelper;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="connectionFactory">The connection factory</param>
        /// <param name="queryBuilderFactory"></param>
        /// <param name="databaseCommandFactory"></param>
        /// <param name="bulkCopyHelper"></param>
        public RemoteDbContextFactory(IRemoteDbConnectionFactory connectionFactory,
                                      IQueryBuilderFactory queryBuilderFactory,
                                      IDatabaseCommandFactory databaseCommandFactory,
                                      IBulkCopyHelper bulkCopyHelper) {
            _connectionFactory = connectionFactory;
            _queryBuilderFactory = queryBuilderFactory;
            _databaseCommandFactory = databaseCommandFactory;
            _bulkCopyHelper = bulkCopyHelper;
        }

        /// <summary>
        /// Creates a context from SQL-info
        /// </summary>
        /// <param name="sqlInfo">The SQL-Info object</param>
        /// <returns>The context</returns>
        public IRemoteDbContext CreateContext(SqlInfo sqlInfo) {
            var connection = _connectionFactory.CreateConnection(sqlInfo);
            return new RemoteDbContext(connection, _queryBuilderFactory, _databaseCommandFactory, _bulkCopyHelper);
        }
    }
}