using BECOSOFT.Data.Query;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;

namespace BECOSOFT.Data.Context {
    /// <inheritdoc />
    internal class DbContextFactory : IDbContextFactory {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDatabaseCommandFactory _databaseCommandFactory;
        private readonly IBulkCopyHelper _bulkCopyHelper;
        private readonly IOfflineTableExistsRepository _tableExistsRepository;

        public string Connection => _connectionFactory.Connection;

        public DbContextFactory(IDbConnectionFactory connectionFactory,
                                IDatabaseCommandFactory databaseCommandFactory,
                                IBulkCopyHelper bulkCopyHelper,
                                IOfflineTableExistsRepository tableExistsRepository) {
            _connectionFactory = connectionFactory;
            _databaseCommandFactory = databaseCommandFactory;
            _bulkCopyHelper = bulkCopyHelper;
            _tableExistsRepository = tableExistsRepository;
        }

        /// <inheritdoc />
        public IBaseEntityDbContext CreateBaseEntityContext() {
            var connection = _connectionFactory.CreateConnection();
            return new BaseEntityDbContext(connection, _bulkCopyHelper, _databaseCommandFactory, _tableExistsRepository);
        }

        /// <inheritdoc />
        public IBaseResultDbContext CreateBaseResultContext() {
            var connection = _connectionFactory.CreateConnection();
            return new BaseResultDbContext(connection, _bulkCopyHelper, _tableExistsRepository);
        }
    }
}