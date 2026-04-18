using BECOSOFT.Data.Caching;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Utilities.Cache;
using BECOSOFT.Utilities.Extensions;
using System;

namespace BECOSOFT.Data.Repositories.QueryData {
    internal class OfflineTableExistsRepository : IOfflineTableExistsRepository {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ITableQueryRepository _tableQueryRepository;
        private readonly bool _checkTableExistence;
        private static string CacheKey => CacheKeyGenerator.GenerateCacheKey(typeof(TableQueryResult));

        public OfflineTableExistsRepository(IDbConnectionFactory connectionFactory,
                                     ITableQueryRepository tableQueryRepository,
                                     bool checkTableExistence) {
            _connectionFactory = connectionFactory;
            _tableQueryRepository = tableQueryRepository;
            _checkTableExistence = checkTableExistence;
        }

        public bool TableExists(Type type, string tablePart = null) {
            if (!_checkTableExistence) {
                return true; // assume table always exists
            }
            var tablePartToUse = Check.IsTableConsuming(type) ? tablePart?.NullIf("") ?? "?" : null;
            return _tableQueryRepository.TableExists(type, tablePartToUse);
        }

        public bool TableExists<T, TDefining>(string tablePart) where T : TableConsumingEntity<TDefining> where TDefining : TableDefiningEntity {
            return TableExists(typeof(T), tablePart);
        }

        public bool TableExists<T>() where T : BaseEntity {
            return TableExists(typeof(T));
        }

        public IMemoryCacheWrapper GetCache() {
            var connectionString = _connectionFactory.Connection;
            return MemoryCacheHolder.GetCache(connectionString, CacheKey);
        }
    }
}