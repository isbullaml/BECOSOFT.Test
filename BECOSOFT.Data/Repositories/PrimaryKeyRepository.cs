using BECOSOFT.Data.Context;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BECOSOFT.Data.Repositories {
    internal sealed class PrimaryKeyRepository : ReadonlyRepository<PrimaryKeyEntity>, IPrimaryKeyRepository {
        private readonly IDbConnectionFactory _dbConnectionFactory;

        internal PrimaryKeyRepository(IDbContextFactory dbContextFactory, 
                                      IDbConnectionFactory dbConnectionFactory,
                                      IDatabaseCommandFactory databaseCommandFactory) 
            : base(dbContextFactory, databaseCommandFactory) {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public HashSet<int> GetIDs(Type type, HashSet<int> ids, string tablePart = null) {
            if (ids.IsEmpty()) { return new HashSet<int>(); }
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(type);
            var primaryKeyInfo = entityTypeInfo.PrimaryKeyInfo;
            if (primaryKeyInfo == null) { return new HashSet<int>(); }

            var parameterQuery = new ParametrizedQuery();
            var query = new StringBuilder();
            query.AppendLine(" SELECT DISTINCT pk.{0} AS {1}", primaryKeyInfo.ColumnName.FormatWith(tablePart), Entity.GetColumn<PrimaryKeyEntity>(p => p.Id));
            query.AppendLine(" FROM {0} pk ", entityTypeInfo.TableDefinition.FullTableName.FormatWith(tablePart));
            if (ids.Count == 1) {
                parameterQuery.AddParameter("@id", ids.First());
                query.AppendLine(" WHERE pk.{0} = @id ", primaryKeyInfo.ColumnName.FormatWith(tablePart));
            } else {
                var tempTable = parameterQuery.AddTempTable(ids.ToList());
                query.AppendLine(" INNER JOIN {0} t on t.tempValue = pk.{1}", tempTable.TableName, primaryKeyInfo.ColumnName.FormatWith(tablePart));
            }

            parameterQuery.SetQuery(query);
            var primaryKeys = Query(parameterQuery);
            return primaryKeys.Select(pk => pk.Id).ToSafeHashSet();
        }

        public HashSet<int> GetIDs<T>(HashSet<int> ids, string tablePart = null) {
            return GetIDs(typeof(T), ids, tablePart);
        }

        public HashSet<int> GetIDs(ParametrizedQuery query) {
            var command = DatabaseCommandFactory.Custom(query);
            using (var context = GetContext()) {
                var primaryKeys = context.QueryConvertible<int>(command);
                return primaryKeys.ToHashSet();
            }
        }

        public PrimaryKeyContainer GetIDs(PrimaryKeyContainer container) {
            var result = new PrimaryKeyContainer();
            var command = CreateContainerCommand(container, result, out var typesPresent);
            if (command == null) {
                return result;
            }

            using (var context = GetContext()) {
                using (var reader = context.ExecuteReader(command)) {
                    if (reader == null) {
                        foreach (var primaryKeyType in typesPresent) {
                            result.Add(primaryKeyType.Value, new HashSet<int>());
                        }
                        return result;
                    }
                    do {
                        var ids = new HashSet<int>();
                        var colName = reader.GetName(0);
                        var type = typesPresent.TryGetValueWithDefault(colName);
                        if (type == null) { continue; }
                        if (reader.HasRows) {
                            while (reader.Read()) {
                                ids.Add(reader.GetValue(0).To<int>());
                            }
                        }
                        result.Add(type, ids);
                    } while (reader.NextResult());
                }
            }

            return result;
        }

        internal DatabaseCommand CreateContainerCommand(PrimaryKeyContainer container, PrimaryKeyContainer result, out Dictionary<string, PrimaryKeyType> typesPresent) {
            typesPresent = new Dictionary<string, PrimaryKeyType>(0);
            if (container.IsEmpty()) {
                return null;
            }
            var parametrizedQuery = new ParametrizedQuery();
            var query = new StringBuilder();
            foreach (var toCheck in container) {
                var idList = toCheck.Value.ToList();
                if (idList.IsEmpty()) {
                    result.Add(toCheck.Key, new HashSet<int>());
                    continue;
                }
                var entityInfo = EntityConverter.GetEntityTypeInfo(toCheck.Key.Type);
                if (!entityInfo.IsBaseEntity) {
                    result.Add(toCheck.Key, new HashSet<int>());
                    continue;
                }
                Check.IsValidTableConsuming(toCheck.Key.Type, toCheck.Key.TablePart);
                var primaryKeyColumnName = entityInfo.PrimaryKeyInfo.ColumnName;
                var columnAlias = $"ENT_{entityInfo.EntityType.Name}_{toCheck.Key.TablePart}";
                query.AppendLine(" SELECT e.{0} AS [{1}]", primaryKeyColumnName, columnAlias);
                query.AppendLine(" FROM {0} e", entityInfo.TableDefinition.GetFullTable(toCheck.Key.TablePart));
                if (idList.Count <= 10) {
                    query.AppendLine(" WHERE e.{0} IN (", primaryKeyColumnName);
                    query.AppendLine(string.Join(",", idList));
                    query.Append(")");
                } else {
                    var tempTableName = parametrizedQuery.AddTempTable(idList).TableName;
                    query.AppendLine(" INNER JOIN {0} t ON t.tempValue = e.{1}", tempTableName, primaryKeyColumnName);
                }
                typesPresent.Add(columnAlias, toCheck.Key);
            }
            if (typesPresent.IsEmpty()) {
                return null;
            }
            parametrizedQuery.SetQuery(query);
            return DatabaseCommandFactory.Custom(parametrizedQuery);
        }
    }
}