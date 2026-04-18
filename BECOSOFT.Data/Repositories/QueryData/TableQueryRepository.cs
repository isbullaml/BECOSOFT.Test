using BECOSOFT.Data.Caching;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Data.Repositories.Interfaces.QueryData;
using BECOSOFT.Utilities.Cache;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Exceptions;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using NLog;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.Text;

namespace BECOSOFT.Data.Repositories.QueryData {

    /// <inheritdoc cref="ITableQueryRepository"/>
    internal class TableQueryRepository : ITableQueryRepository {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IDatabaseQueryRepository _databaseQueryRepository;
        private static string CacheKey => CacheKeyGenerator.GenerateCacheKey(typeof(TableQueryResult));

        public TableQueryRepository(IDbConnectionFactory connectionFactory,
                                    IDatabaseQueryRepository databaseQueryRepository) {
            _connectionFactory = connectionFactory;
            _databaseQueryRepository = databaseQueryRepository;
        }

        public IMemoryCacheWrapper GetCache() {
            var connectionString = _connectionFactory.Connection;
            return MemoryCacheHolder.GetCache(connectionString, CacheKey);
        }

        public bool TableExists<T>(string tablePart = null, string database = null) where T : IEntity {
            var type = typeof(T);
            return TableExists(type, tablePart, database);
        }

        public bool TableExists(Type type, string tablePart = null, string database = null) {
            Check.IsValidTableConsuming(type, tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(type);
            var tableDefinition = entityInfo.TableDefinition;
            if (tableDefinition.Schema == Schema.Unknown) {
                Logger.Error($"{type.FullName} is missing a TableAttribute or ResultTableAttribute.");
                throw new UndefinedTableDefinitionException("Invalid schema given");
            }
            return TableExists(tableDefinition.Schema, tableDefinition.TableName, tablePart, database);
        }

        public bool TableExists(Schema schema, string tableName, string tablePart = null, string database = null) {
            if (schema == Schema.Unknown) {
                Logger.Error("Invalid schema given.");
                throw new UndefinedTableDefinitionException("Invalid schema given");
            }
            if (schema == Schema.Dbo && tableName.IsNullOrEmpty()) {
                return true;
            }
            var cleanDatabase = TableHelper.Clean(database);
            var dbPart = "";
            if (cleanDatabase.HasValue()) {
                if (!_databaseQueryRepository.HasDatabaseAccess(database)) {
                    throw new DbAccessException($"No access to {database}");
                }
                dbPart = $"[{cleanDatabase}].";
            }
            var query = $"SELECT 1 AS ENT0_{Entity.GetColumn<TableQueryResult>(r => r.Exists)} FROM {dbPart}INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Table";
            var parameters = new List<SqlParameter> {
                new SqlParameter("@Schema", schema.ToSql()),
                new SqlParameter("@Table", tableName.FormatWith(tablePart))
            };

            return Execute(Func, Key);

            bool Func() => HasRows(query, parameters);

            string Key() {
                var fullQuery = QueryDumper.GetCommandText(query, parameters).ToString();
                return HashHelper.GetMd5StringFromString(fullQuery);
            }
        }

        public bool ViewExists<T>(string tablePart = null, string database = null) where T : IEntity {
            var type = typeof(T);
            return ViewExists(type, tablePart, database);
        }

        public bool ViewExists(Type type, string tablePart = null, string database = null) {
            Check.IsValidTableConsuming(type, tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(type);
            var tableDefinition = entityInfo.TableDefinition;
            if (tableDefinition.Schema == Schema.Unknown) {
                Logger.Error($"{type.FullName} is missing a TableAttribute or ResultTableAttribute.");
                throw new UndefinedTableDefinitionException("Invalid schema given");
            }
            return ViewExists(tableDefinition.Schema, tableDefinition.TableName, tablePart, database);
        }

        public bool ViewExists(Schema schema, string tableName, string tablePart = null, string database = null) {
            if (schema == Schema.Unknown) {
                Logger.Error("Invalid schema given.");
                throw new UndefinedTableDefinitionException("Invalid schema given");
            }
            if (schema == Schema.Dbo && tableName.IsNullOrEmpty()) {
                return true;
            }
            var cleanDatabase = TableHelper.Clean(database);
            var dbPart = "";
            if (cleanDatabase.HasValue()) {
                if (!_databaseQueryRepository.HasDatabaseAccess(database)) {
                    throw new DbAccessException($"No access to {database}");
                }
                dbPart = $"[{cleanDatabase}].";
            }
            var query = $"SELECT 1 AS ENT0_{Entity.GetColumn<TableQueryResult>(r => r.Exists)} FROM {dbPart}INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Table";
            var parameters = new List<SqlParameter> {
                new SqlParameter("@Schema", schema.ToSql()),
                new SqlParameter("@Table", tableName.FormatWith(tablePart))
            };

            return Execute(Func, Key);

            bool Func() => HasRows(query, parameters);

            string Key() {
                var fullQuery = QueryDumper.GetCommandText(query, parameters).ToString();
                return HashHelper.GetMd5StringFromString(fullQuery);
            }
        }

        public void RegisterTableExists(ReplicatedTableEntry tableEntry) {
            if (!tableEntry.DidCheck) { return; }
            var tableDefinition = tableEntry.TableDefinition;
            foreach (var queryResult in tableEntry.QueryResults) {
                var tablePart = queryResult.TablePart;
                var query = $"SELECT 1 AS ENT0_{Entity.GetColumn<TableQueryResult>(r => r.Exists)} FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Table";
                var parameters = new List<SqlParameter> {
                    new SqlParameter("@Schema", tableDefinition.Schema.ToSql()),
                    new SqlParameter("@Table", tableDefinition.TableName.FormatWith(tablePart)),
                };

                Execute(Func, Key);
                continue;

                bool Func() => !(queryResult.IsMissing ?? false);

                string Key() {
                    var fullQuery = QueryDumper.GetCommandText(query, parameters).ToString();
                    return HashHelper.GetMd5StringFromString(fullQuery);
                }
            }
        }

        public bool ColumnExists<T, TProp>(Expression<Func<T, TProp>> columnSelector, string tablePart = null, string database = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var column = columnSelector.GetProperty();
            if (column == null || column.ColumnName.IsNullOrWhiteSpace()) {
                return false;
            }

            return ColumnExists<T>(column.ColumnName, tablePart, database);
        }

        public bool ColumnExists<T>(string column, string tablePart = null, string database = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tableDefinition = entityInfo.TableDefinition;

            return ColumnExists(tableDefinition.Schema, tableDefinition.TableName, column, tablePart, database);
        }

        public bool ColumnExists(Schema schema, string tableName, string column, string tablePart = null, string database = null) {
            var cleanDatabase = TableHelper.Clean(database);
            var dbPart = "";
            if (cleanDatabase.HasValue()) {
                if (!_databaseQueryRepository.HasDatabaseAccess(database)) {
                    throw new DbAccessException($"No access to {database}");
                }
                dbPart = $"[{cleanDatabase}].";
            }
            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var query = $"SELECT 1 AS ENT0_{Entity.GetColumn<TableQueryResult>(r => r.Exists)} FROM {dbPart}sys.columns WHERE Name = @Column AND Object_ID = Object_ID(@FullTable)";
            var parameters = new List<SqlParameter> {
                new SqlParameter("@Column", column),
                new SqlParameter("@FullTable", dbPart + fullTable),
            };

            return Execute(Func, Key);

            bool Func() => HasRows(query, parameters);

            string Key() {
                var fullQuery = QueryDumper.GetCommandText(query, parameters).ToString();
                return HashHelper.GetMd5StringFromString(fullQuery);
            }
        }

        public bool HasRows<T>(string tablePart = null, string database = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tableDefinition = entityInfo.TableDefinition;

            return HasRows(tableDefinition.Schema, tableDefinition.TableName, tablePart, database);
        }

        public bool HasRows(Schema schema, string tableName, string tablePart = null, string database = null) {
            var cleanDatabase = TableHelper.Clean(database);
            var dbPart = "";
            if (cleanDatabase.HasValue()) {
                if (!_databaseQueryRepository.HasDatabaseAccess(database)) {
                    throw new DbAccessException($"No access to {database}");
                }
                dbPart = $"[{cleanDatabase}].";
            }
            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var query = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM {dbPart}{fullTable}) THEN 1 ELSE 0 END AS ENT0_{Entity.GetColumn<TableQueryResult>(r => r.Exists)}";

            return Execute(Func, Key);

            bool Func() => Exists(query, null);

            string Key() {
                var fullQuery = QueryDumper.GetCommandText(query, null).ToString();
                return HashHelper.GetMd5StringFromString(fullQuery);
            }
        }

        public Dictionary<TypeTablePartDefinition, bool> HasRows(KeyValueList<Type, string> typeTablePartValues, string database = null) {
            if (typeTablePartValues.IsEmpty()) {
                return new Dictionary<TypeTablePartDefinition, bool>(0);
            }
            var cleanDatabase = TableHelper.Clean(database);
            var dbPart = "";
            if (cleanDatabase.HasValue()) {
                if (!_databaseQueryRepository.HasDatabaseAccess(database)) {
                    throw new DbAccessException($"No access to {database}");
                }
                dbPart = $"[{cleanDatabase}].";
            }
            var query = new StringBuilder();
            query.AppendLine(" CREATE TABLE #TempTypeTablePartEntry([TypeName] NVARCHAR(500), [TablePart] NVARCHAR(255), HasRows BIT) ");

            var mapping = new Dictionary<Tuple<string, string>, Type>();
            foreach (var typeTablePartValue in typeTablePartValues) {
                var type = typeTablePartValue.Key;
                var entityTypeInfo = EntityConverter.GetEntityTypeInfo(type);
                var fullTableName = entityTypeInfo.TableDefinition.GetFullTable(typeTablePartValue.Value);
                query.AppendLine(" INSERT INTO #TempTypeTablePartEntry([TypeName], [TablePart], HasRows) ");
                var tablePartSqlString = typeTablePartValue.Value == null ? "NULL" : $"'{typeTablePartValue.Value.Replace("'", "''")}'";
                query.AppendLine(" SELECT '{0}' AS [TypeName], {1} AS [TablePart], CASE WHEN EXISTS(SELECT 1 FROM {3}{2}) THEN 1 ELSE 0 END ", type.FullName, tablePartSqlString, fullTableName, dbPart);
                mapping[Tuple.Create(type.FullName, typeTablePartValue.Value)] = type;
            }

            query.AppendLine(" SELECT * FROM #TempTypeTablePartEntry ");

            var result = new Dictionary<TypeTablePartDefinition, bool>();
            using (var connection = _connectionFactory.CreateConnection()) {
                var command = connection.CreateCommand();
                command.CommandText = query.ToString();
                using (var reader = command.ExecuteReader()) {
                    while (reader.Read()) {
                        var typeName = reader.GetString(0);
                        var tablePart = reader.IsDBNull(1) ? null : reader.GetString(1);
                        var hasRows = reader.GetBoolean(2);
                        var type = mapping.TryGetValueWithDefault(Tuple.Create(typeName, tablePart));
                        var ttpd = new TypeTablePartDefinition(type, tablePart);
                        result[ttpd] = hasRows;
                        CacheTypeTablePartDefinitionHasRowsResult(ttpd, hasRows, dbPart);
                    }
                }
            }
            foreach (var kvp in mapping) {
                var ttpd = new TypeTablePartDefinition(kvp.Value, kvp.Key.Item2);
                if (result.ContainsKey(ttpd)) { continue; }
                CacheTypeTablePartDefinitionHasRowsResult(ttpd, false, dbPart);
                result[ttpd] = false;
            }

            return result;
        }

        private void CacheTypeTablePartDefinitionHasRowsResult(TypeTablePartDefinition definitionResult, bool hasRows, string dbPart) {
            var schema = definitionResult.TableDefinition.Schema;
            var tableName = definitionResult.TableDefinition.TableName;
            var tablePart = definitionResult.TablePart;
            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var query = $"SELECT CASE WHEN EXISTS(SELECT 1 FROM {dbPart}{fullTable}) THEN 1 ELSE 0 END AS ENT0_{Entity.GetColumn<TableQueryResult>(r => r.Exists)}";

            var fullQuery = QueryDumper.GetCommandText(query, null).ToString();
            var key = HashHelper.GetMd5StringFromString(fullQuery);
            GetCache().Retrieve(key, () => hasRows);
        }

        public bool HasIdentity<T>(string tablePart = null, string database = null) where T : IEntity {
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var tableDefinition = entityInfo.TableDefinition;

            return HasIdentity(tableDefinition.Schema, tableDefinition.TableName, tablePart, database);
        }

        public bool HasIdentity(Schema schema, string tableName, string tablePart = null, string database = null) {
            var cleanDatabase = TableHelper.Clean(database);
            var dbPart = "";
            if (cleanDatabase.HasValue()) {
                if (!_databaseQueryRepository.HasDatabaseAccess(database)) {
                    throw new DbAccessException($"No access to {database}");
                }
                dbPart = $"[{cleanDatabase}].";
            }
            var fullTable = TableHelper.GetCombined(schema, tableName, tablePart);
            var query = $"SELECT 1 AS ENT0_{Entity.GetColumn<TableQueryResult>(r => r.Exists)} FROM {dbPart}sys.identity_columns WHERE Object_ID = Object_ID(@FullTable)";
            var parameters = new List<SqlParameter> {
                new SqlParameter("@FullTable", dbPart + fullTable),
            };

            return Execute(Func, Key);

            bool Func() => Exists(query, parameters);

            string Key() {
                var fullQuery = QueryDumper.GetCommandText(query, parameters).ToString();
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

        private bool HasRows(string query, List<SqlParameter> parameters) {
            using (var connection = _connectionFactory.CreateConnection()) {
                var command = connection.CreateCommand();
                command.CommandText = query;
                if (parameters.HasAny()) {
                    foreach (var sqlParameter in parameters) {
                        command.Parameters.Add(sqlParameter.GetClone());
                    }
                }
                using (var reader = command.ExecuteReader()) {
                    if (reader is SqlDataReader sqlDataReader) {
                        return sqlDataReader.HasRows;
                    }
                    var didSeeRows = false;
                    while (reader.Read()) {
                        didSeeRows = true;
                    }
                    return didSeeRows;
                }
            }
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
