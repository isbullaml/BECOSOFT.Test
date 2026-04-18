using BECOSOFT.Data.Caching;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Cache;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace BECOSOFT.Data.Repositories.QueryData {
    internal class DatabaseQueryRepository : IDatabaseQueryRepository {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDbConnectionFactory _connectionFactory;
        private static string CacheKey => CacheKeyGenerator.GenerateCacheKey(typeof(DatabaseQueryResult));

        internal DatabaseQueryRepository(IDbConnectionFactory connectionFactory) {
            _connectionFactory = connectionFactory;
        }

        private IMemoryCacheWrapper GetCache() {
            var connectionString = _connectionFactory.Connection;
            return MemoryCacheHolder.GetCache(connectionString, CacheKey);
        }

        public bool DatabaseExists(string database) {
            var cleanDatabase = TableHelper.Clean(database);
            if (cleanDatabase.IsNullOrWhiteSpace()) { return false; }
            var query = $"SELECT CASE WHEN DB_ID('{cleanDatabase}') IS NULL THEN 0 ELSE 1 END AS ENT0_{Entity.GetColumn<DatabaseQueryResult>(r => r.Exists)}";
            return ExecuteDatabaseQuery(query);
        }

        public bool HasDatabaseAccess(string database) {
            var cleanDatabase = TableHelper.Clean(database);
            if (cleanDatabase.IsNullOrWhiteSpace()) { return false; }
            var query = $"SELECT CASE WHEN DB_ID('{cleanDatabase}') IS NULL THEN 0 ELSE HAS_DBACCESS('{cleanDatabase}') END AS ENT0_{Entity.GetColumn<DatabaseQueryResult>(r => r.Exists)}";
            return ExecuteDatabaseQuery(query);
        }

        public bool HasConnection(int timeout = 1000) {
            try {
                var cancellationTokenSource = new CancellationTokenSource();
                cancellationTokenSource.CancelAfter(timeout);
                var cancellationToken = cancellationTokenSource.Token;

                var connectionTask = Task.Run(OpenConnection, cancellationToken);
                Task.WaitAll(connectionTask);
                return true;

                async Task<bool> OpenConnection() {
                    using (var sqlConnection = new SqlConnection(_connectionFactory.Connection)) {
                        await sqlConnection.OpenAsync(cancellationToken);
                    }
                    return true;
                }
            } catch (AggregateException e) {
                if (e.InnerExceptions.HasAny()) {
                    foreach (var innerEx in e.InnerExceptions) {
                        var x = innerEx;
                        do {
                            if (x is SqlException || x is TaskCanceledException) {
                                return false;
                            }
                            x = x?.InnerException;
                        } while (x != null);
                    }
                }

                Logger.Error(e);
                throw;
            } catch (TaskCanceledException) {
                return false;
            } catch (SqlException) {
                return false;
            }
        }

        public bool HasAssembly(string assemblyName) {
            if (assemblyName.IsNullOrWhiteSpace()) { return false; }
            var query = $@"
SELECT CASE 
		WHEN (
				SELECT 1
				FROM sys.assembly_files a
				WHERE a.name = '%{assemblyName.SqlEscape()}%'
				) IS NOT NULL
			THEN 1
		ELSE 0
		END AS ENT0_{Entity.GetColumn<DatabaseQueryResult>(r => r.Exists)}";
            return ExecuteDatabaseQuery(query);
        }

        public bool HasAssemblyFunction( string assemblyName, string functionName) {
            if (assemblyName.IsNullOrWhiteSpace() || functionName.IsNullOrWhiteSpace()) { return false; }
            var query = $@"
SELECT CASE 
		WHEN (
				SELECT 1
				FROM sys.assembly_files a
				INNER JOIN sys.assembly_modules m ON m.assembly_id = a.assembly_id
				WHERE a.name LIKE '%{assemblyName.SqlEscape()}%' AND m.assembly_method = '{functionName.SqlEscape()}'
				) IS NOT NULL
			THEN 1
		ELSE 0
		END AS ENT0_{Entity.GetColumn<DatabaseQueryResult>(r => r.Exists)}";
            return ExecuteDatabaseQuery(query);
        }

        public DatabaseSize GetSize(string database) {
            var cleanDatabase = TableHelper.Clean(database);
            if (cleanDatabase.IsNullOrWhiteSpace()) { return DatabaseSize.Empty; }
            var query = $@"
SELECT CAST(SUM(CASE WHEN df.Type = 0 THEN size * 8.0/1024.0 ELSE 0 END) AS FLOAT) AS {Entity.GetColumn<DatabaseSize>(ds => ds.RawDatabaseSize)}
     , CAST(SUM(CASE WHEN df.Type = 0 THEN (df.size - CAST(FILEPROPERTY(df.name, 'SpaceUsed') AS INT)) * 8.0/1024.0 ELSE 0 END) AS FLOAT) AS {Entity.GetColumn<DatabaseSize>(ds => ds.RawDatabaseFreeSpace)}
	 , CAST(SUM(CASE WHEN df.Type = 1 THEN size * 8.0/1024.0 ELSE 0 END) AS FLOAT) AS {Entity.GetColumn<DatabaseSize>(ds => ds.RawLogSize)}
     , CAST(SUM(CASE WHEN df.Type = 1 THEN (df.size - CAST(FILEPROPERTY(df.name, 'SpaceUsed') AS INT)) * 8.0/1024.0 ELSE 0 END) AS FLOAT) AS {Entity.GetColumn<DatabaseSize>(ds => ds.RawLogFreeSpace)}
FROM sys.database_files df
";
            using (var connection = _connectionFactory.CreateConnection()) {
                var command = connection.CreateCommand();
                command.CommandText = query;
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var rawDatabaseSize = reader.GetDouble(0);
                        var rawDatabaseFreeSpace = reader.GetDouble(1);
                        var rawLogSize = reader.GetDouble(2);
                        var rawLogFreeSpace = reader.GetDouble(3);
                        return new DatabaseSize {
                            RawDatabaseSize = rawDatabaseSize,
                            RawDatabaseFreeSpace = rawDatabaseFreeSpace,
                            RawLogSize = rawLogSize,
                            RawLogFreeSpace = rawLogFreeSpace,
                        };
                    }
                }
            }
            return DatabaseSize.Empty;
        }

        private bool ExecuteDatabaseQuery(string query) {
            return Execute(Func, Key);

            bool Func() => Exists(query, null);

            string Key() {
                var fullQuery = QueryDumper.GetCommandText(query, null).ToString();
                return HashHelper.GetMd5StringFromString(fullQuery);
            }
        }

        /// <summary>
        /// If the cache is enabled for the repository:
        /// Executes the provided <see cref="func"/> when the cache does not contain an entry for the provided <see cref="keyFunc"/>.
        /// If the cache is disabled (default) for the repository:
        /// Executes the <see cref="func"/> and returns the results.
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        /// <param name="func"></param>
        /// <param name="keyFunc"></param>
        /// <returns>Either the cached results or the function result.</returns>
        private TU Execute<TU>(Func<TU> func, Func<string> keyFunc) {
            var key = keyFunc();
            return GetCache().Retrieve(key, func);
        }

        private bool Exists(string query, List<SqlParameter> parameters) {
            using (var connection = _connectionFactory.CreateConnection()) {
                var command = connection.CreateCommand();
                command.CommandText = query;
                if (parameters.HasAny()) {
                    foreach (var sqlParameter in parameters) {
                        command.Parameters.Add(sqlParameter.GetClone());
                    }
                }
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        return reader.GetValue(0).To<bool>();
                    }
                }
            }
            return false;
        }
    }
}