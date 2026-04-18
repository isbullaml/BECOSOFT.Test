using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace BECOSOFT.Data.Query.Builders {
    /// <summary>
    /// Base query builder for a SELECT-statement
    /// </summary>
    [SuppressMessage("ReSharper", "ArgumentsStyleLiteral")]
    internal abstract class SelectBaseQueryBuilder : BaseQueryBuilder {
        private readonly QueryTranslator _translator;
        private readonly IJoinQueryBuilder _joinQueryBuilder;

        protected SelectBaseQueryBuilder(IJoinQueryBuilder joinQueryBuilder,
                                         IOfflineTableExistsRepository tableExistsRepository)
            : base(tableExistsRepository) {
            _translator = new QueryTranslator();
            _joinQueryBuilder = joinQueryBuilder;
        }

        protected StringBuilder GenerateSelectQuery() {
            var query = new StringBuilder();
            if (Info.Expression != null) {
                _translator.Translate(Info, parameterPrefix: BaseLevelAlias);
                Info.TempTables = _translator.TempTables;
                Info.BulkCopyTempTables = _translator.BulkCopyTempTables;
                Info.IsDistinct = Info.IsDistinct || _translator.Distinct;
            }
            var typeInfo = Info.TypeInfo;
            if (!OfflineTableExistsRepository.TableExists(typeInfo.EntityType, Info.TablePart)) {
                Logger.Warn("Table {0} does not exist", typeInfo.TableDefinition.GetFullTable(Info.TablePart));
                query.AppendLine(" ");
                return query;
            }
            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(typeInfo.EntityType, true);
            var withSkipTake = _translator.Take.HasValue || _translator.Skip.HasValue;
            AddTempTableQueries(query);
            if (!Info.PremadeQuery.IsNullOrWhiteSpace()) {
                query.AppendLine(Info.PremadeQuery);
            }
            var isSimpleWhere = false;
            var whereClause = GetWhereClause(_translator, ref isSimpleWhere);
            var fullTableName = GetTableName(typeInfo.TableDefinition.FullTableName);
            var primaryKeyColumnName = GetPrimaryKey(typeInfo.PrimaryKeyInfo?.ColumnName ?? typeInfo.TablePrimaryKey);
            var isBaseEntityWithoutPrimaryKey = typeInfo.IsBaseEntity && (typeInfo.PrimaryKeyInfo?.ColumnName.IsNullOrWhiteSpace() ?? true);
            var isBaseResultWithoutPrimaryKey = typeInfo.IsBaseResult && typeInfo.TablePrimaryKey.IsNullOrWhiteSpace();
            if (isBaseEntityWithoutPrimaryKey || isBaseResultWithoutPrimaryKey) {
                var simpleSelect = GetNonPrimaryKeySelectQuery(fullTableName, whereClause);
                query.AppendLine(simpleSelect);
                AddDropTempTableQueries(query);
                return query;
            }
            var orderBy = _translator.GetOrderBy(BaseLevelAlias, primaryKeyColumnName);

            var joinPartsLinkedEntity = _joinQueryBuilder.AddLinkedEntitiesJoinParts(linkedEntitiesTree, true, true, Info.TablePart);

            if (withSkipTake) {
                query.AppendLine("CREATE TABLE #IDTable(id INT NOT NULL, RowNumber BIGINT NOT NULL)");
                query.AppendLine("INSERT INTO #IDTable(id, RowNumber) ");
                query.AppendLine("SELECT {1}.{2}, ROW_NUMBER() OVER(ORDER BY {0})", orderBy, BaseLevelAlias, primaryKeyColumnName);
                query.AppendLine("FROM(");
                query.AppendLine("SELECT DISTINCT {0}.*", BaseLevelAlias);
            } else if (!typeInfo.HasLinkedEntityProperties && !typeInfo.HasLinkedBaseResultProperties) {
                query.AppendLine($"SELECT COUNT({primaryKeyColumnName}) AS PagerTotalItemCount, 0 AS PagerCurrentSkip, 0 AS PagerCurrentTake FROM {fullTableName} {BaseLevelAlias}");
                if (!whereClause.IsNullOrWhiteSpace()) {
                    query.AppendLine("WHERE ");
                    query.AppendLine(whereClause);
                }

                query.AppendLine("SELECT ");
                var selectParts = GetAliasedSelectParts(linkedEntitiesTree, BaseLevelAlias, "");
                query.Append(string.Join($"{Environment.NewLine}, ", selectParts)).AppendLine();
            } else {
                query.AppendLine("CREATE TABLE #IDTable(id INT NOT NULL)");
                query.AppendLine("INSERT INTO #IDTable(id) ");
                query.AppendLine("SELECT DISTINCT {0}.{1}", BaseLevelAlias, primaryKeyColumnName);
            }

            query.AppendLine("FROM {0} {1}", fullTableName, BaseLevelAlias);
            if (!whereClause.IsNullOrWhiteSpace()) {
                if (!isSimpleWhere && joinPartsLinkedEntity.HasAny()) {
                    query.Append(string.Join(Environment.NewLine, joinPartsLinkedEntity)).AppendLine();
                }
                query.AppendLine("WHERE ");
                query.AppendLine(whereClause);
            }

            if (!withSkipTake && !typeInfo.HasLinkedEntityProperties && !typeInfo.HasLinkedBaseResultProperties && !_translator.AliasedOrderBy.IsNullOrWhiteSpace()) {
                query.AppendLine($"ORDER BY {_translator.AliasedOrderBy}");
            }

            if (!typeInfo.HasLinkedEntityProperties && !typeInfo.HasLinkedBaseResultProperties && !withSkipTake) {
                AddDropTempTableQueries(query);
                return query;
            }

            if (withSkipTake) {
                var safeSkipValue = _translator.Skip ?? 0;
                var safeTakeValue = _translator.Take ?? 0;
                var minValue = safeSkipValue * safeTakeValue + 1;
                var maxValue = (safeSkipValue + 1) * safeTakeValue;
                query.AppendLine(") {0}", BaseLevelAlias);
                query.AppendLine(" CREATE NONCLUSTERED INDEX ID_INDEX ON #IDTable(id)");
                query.AppendLine("");
                query.AppendLine("CREATE TABLE #LimitedIDTable(id INT NOT NULL, RowNumber BIGINT NOT NULL)");
                query.AppendLine("INSERT INTO #LimitedIDTable(id, RowNumber) ");
                query.AppendLine("SELECT id, RowNumber FROM #IDTable");
                if (maxValue == 0) {
                    query.AppendLine("WHERE RowNumber >= {0}", minValue);
                } else {
                    query.AppendLine("WHERE RowNumber BETWEEN {0} AND {1}", minValue, maxValue);
                }

                query.AppendLine(" CREATE NONCLUSTERED INDEX ID_INDEX ON #LimitedIDTable(id)");
            } else {
                query.AppendLine(" CREATE NONCLUSTERED INDEX ID_INDEX ON #IDTable(id)");
            }

            // Select total entity count, skip and take values
            query.AppendLine("SELECT COUNT(id) AS PagerTotalItemCount, {0} AS PagerCurrentSkip, {1} AS PagerCurrentTake FROM #IDTable", _translator.Skip ?? 0, _translator.Take ?? 0);

            string tempOrderBy = null;
            if (withSkipTake) {
                tempOrderBy = "ORDER BY RowNumber";
            } else if (!_translator.AliasedOrderBy.IsNullOrWhiteSpace()) {
                tempOrderBy = $"ORDER BY {_translator.AliasedOrderBy}";
            }
            var builders = new List<StringBuilder>();
            var tempTableNamesToDrop = new List<string>();

            string parentIDTableName;
            if (typeInfo.InverseLinkedEntityProperties.HasAny() || typeInfo.InverseLinkedBaseChildProperties.HasAny()) {
                query.AppendLine("SELECT DISTINCT t.*");
                foreach (var inverseLinkedProperty in typeInfo.InverseLinkedEntityProperties) {
                    var foreignKeyColumnName = typeInfo.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null)?.EscapedColumnName;
                    if (foreignKeyColumnName == null) {
                        var parentTypeInfo = inverseLinkedProperty.Parent.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null);
                        foreignKeyColumnName = parentTypeInfo.EscapedColumnName;
                    }
                    query.AppendLine(", e.{0} AS [{0}]", foreignKeyColumnName);
                }
                foreach (var inverseLinkedProperty in typeInfo.InverseLinkedBaseChildProperties) {
                    var foreignKeyColumnName = typeInfo.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null)?.EscapedColumnName;
                    if (foreignKeyColumnName == null) {
                        var parentTypeInfo = inverseLinkedProperty.Parent.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null);
                        foreignKeyColumnName = parentTypeInfo.EscapedColumnName;
                    }
                    query.AppendLine(", e.{0} AS [{0}]", foreignKeyColumnName);
                }
                query.AppendLine("INTO #BaseTable");
                query.AppendLine("FROM {0} t", withSkipTake ? "#LimitedIDTable" : "#IDTable");
                query.AppendLine("INNER JOIN {0} e on e.{1} = t.id", fullTableName, primaryKeyColumnName);
                query.AppendLine(" CREATE NONCLUSTERED INDEX BaseTable_ID_INDEX ON #BaseTable(id)");
                tempTableNamesToDrop.Add("#BaseTable");
                parentIDTableName = "#BaseTable";
            } else {
                parentIDTableName = withSkipTake ? "#LimitedIDTable" : "#IDTable";
            }

            GetSelects(linkedEntitiesTree, builders, tempTableNamesToDrop, parentIDTableName, withSkipTake, tempOrderBy);

            var mainQuery = builders[0];
            query.AppendLine(mainQuery.ToString());
            for (var i = 1; i < builders.Count; i++) {
                var builder = builders[i];
                query.AppendLine(builder.ToString());
            }

            query.AppendLine("DROP TABLE #IDTable");
            if (withSkipTake) {
                query.AppendLine("DROP TABLE #LimitedIDTable");
            }
            foreach (var tempTableName in tempTableNamesToDrop) {
                query.AppendLine("DROP TABLE {0}", tempTableName);
            }
            AddDropTempTableQueries(query);
            return query;
        }

        private string GetNonPrimaryKeySelectQuery(string fullTableName, string whereClause) {
            var simpleQuery = new StringBuilder();
            simpleQuery.AppendLine("SELECT ");
            simpleQuery.Append(string.Join($"{Environment.NewLine}, ", GetSelectParts())).AppendLine();
            simpleQuery.AppendLine("FROM {0} {1}", fullTableName, BaseLevelAlias);
            if (!whereClause.IsNullOrWhiteSpace()) {
                simpleQuery.AppendLine("WHERE ");
                simpleQuery.AppendLine(whereClause);
            }
            return simpleQuery.ToString();
        }

        private void GetSelects(ITreeNode<EntityTreeNode> node, List<StringBuilder> builders, List<string> tempTableNamesToDrop,
                                string parentIDTableName, bool withSkipTake, string orderBy = null) {
            var typeInfo = node.Value.EntityTypeInfo;
            // Select entity with base child and (inverse) linked entity properties of the entity
            var fullTableName = GetTableName(typeInfo.TableDefinition.FullTableName);
            if (Info.SelectedProperties.HasAny()) {
                var simpleQuery = new StringBuilder();
                var primaryKey = GetPrimaryKey(typeInfo.PrimaryKeyInfo?.ColumnName ?? typeInfo.TablePrimaryKey);
                simpleQuery.AppendLine("SELECT {0}", Info.IsDistinct ? "DISTINCT " : "");
                if (withSkipTake) {
                    simpleQuery.AppendLine("RowNumber, ");
                }
                simpleQuery.Append(string.Join($"{Environment.NewLine}, ", GetSelectParts())).AppendLine();
                simpleQuery.AppendLine("FROM {0} {1}", fullTableName, BaseLevelAlias);
                if (withSkipTake) {
                    simpleQuery.AppendLine("INNER JOIN {0} ON {1}.{2} = {0}.id", parentIDTableName, BaseLevelAlias, primaryKey);
                }

                simpleQuery.AppendLine("WHERE {0}.{1} IN (SELECT id FROM {2})", BaseLevelAlias, primaryKey, parentIDTableName);
                simpleQuery.AppendLine(orderBy);
                builders.Add(simpleQuery);
                return;
            }
            var isBaseNode = node.Level == 0;
            var query = new StringBuilder();
            var alias = builders.IsEmpty() ? BaseLevelAlias : $"{LevelAlias}{node.Level}_{node.Value.Index}";
            var entityTypeName = isBaseNode ? "" : node.Value.EntityTypeInfo.EntityType.Name + "_";
            var selectParts = GetAliasedSelectParts(node, alias, entityTypeName);
            var linkedEntityNodes = node.Where(childNode => childNode.Value.Type != EntityTreeNodeType.Base).ToList();
            query.AppendLine("SELECT {0}", Info.IsDistinct ? "DISTINCT " : "");
            query.Append(string.Join($"{Environment.NewLine}, ", selectParts)).AppendLine();
            query.AppendLine("FROM {0} {1} ", fullTableName, alias);
            query.AppendLine("INNER JOIN {0} ON {1}.{2} = {0}.id", parentIDTableName, alias, GetPrimaryKey(typeInfo.PrimaryKeyInfo?.ColumnName ?? typeInfo.TablePrimaryKey));
            if (!orderBy.IsNullOrWhiteSpace()) {
                query.AppendLine(orderBy);
            }
            builders.Add(query);
            foreach (var linkedEntityNode in linkedEntityNodes) {
                var linkedEntityType = linkedEntityNode.Value.EntityTypeInfo;
                if (!OfflineTableExistsRepository.TableExists(linkedEntityType.EntityType, Info.TablePart)) {
                    continue;
                }
                var tempTable = $"#{LevelAlias}{linkedEntityNode.Level}_{linkedEntityNode.Value.Index}IDTable";
                var foreignKeyColumnName = typeInfo.GetPropertyInfo(linkedEntityNode.Value.EntityPropertyInfo.ForeignKeyColumn, null)?.EscapedColumnName;
                if (foreignKeyColumnName == null) {
                    if (linkedEntityNode.Value.EntityPropertyInfo.IsInverseLinkedEntity) {
                        var parentTypeInfo = linkedEntityNode.Value.EntityPropertyInfo.Parent.GetPropertyInfo(linkedEntityNode.Value.EntityPropertyInfo.ForeignKeyColumn, null);
                        foreignKeyColumnName = parentTypeInfo.EscapedColumnName;
                    } else {
                        foreignKeyColumnName = linkedEntityNode.Value.EntityPropertyInfo.ForeignKeyColumn;
                    }
                }
                var idSelector = "id";
                if (linkedEntityNode.Value.EntityPropertyInfo.IsInverseLinkedEntity) {
                    var basePropertyInfo = linkedEntityNode.Value.EntityTypeInfo.PrimaryKeyInfo ?? linkedEntityNode.Value.EntityTypeInfo.TablePrimaryKeyInfo;
                    idSelector = foreignKeyColumnName;
                    foreignKeyColumnName = basePropertyInfo?.ColumnName ?? foreignKeyColumnName;
                }
                foreignKeyColumnName = GetPrimaryKey(foreignKeyColumnName);
                query.AppendLine("SELECT DISTINCT t.{0} as id", GetPrimaryKey(linkedEntityType.PrimaryKeyInfo?.ColumnName ?? linkedEntityType.TablePrimaryKey));
                foreach (var inverseLinkedProperty in linkedEntityType.InverseLinkedEntityProperties) {
                    var inverseLinkedColName = linkedEntityType.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null)?.EscapedColumnName;
                    if (inverseLinkedColName == null) {
                        var parentTypeInfo = inverseLinkedProperty.Parent.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null);
                        inverseLinkedColName = parentTypeInfo.EscapedColumnName;
                    }
                    query.AppendLine(", t.{0} AS [{0}]", inverseLinkedColName);
                }
                foreach (var inverseLinkedProperty in linkedEntityType.InverseLinkedBaseChildProperties) {
                    var inverseLinkedColName = linkedEntityType.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null)?.EscapedColumnName;
                    if (inverseLinkedColName == null) {
                        var parentTypeInfo = inverseLinkedProperty.Parent.GetPropertyInfo(inverseLinkedProperty.ForeignKeyColumn, null);
                        inverseLinkedColName = parentTypeInfo.EscapedColumnName;
                    }
                    query.AppendLine(", t.{0} AS [{0}]", inverseLinkedColName);
                }
                query.AppendLine("INTO {0}", tempTable);
                query.AppendLine("FROM {0} t", GetTableName(linkedEntityType.TableDefinition.FullTableName));
                query.AppendLine("INNER JOIN {2} e ON e.{1} = t.{0}", foreignKeyColumnName, idSelector, parentIDTableName);
                tempTableNamesToDrop.Add(tempTable);
                GetSelects(linkedEntityNode, builders, tempTableNamesToDrop, tempTable, withSkipTake);
            }
        }

        private List<string> GetAliasedSelectParts(ITreeNode<EntityTreeNode> node, string alias, string entityTypeName) {
            var selectParts = new List<string>();
            foreach (var property in node.Value.EntityTypeInfo.SelectProperties) {
                if (property.IsBaseChild) {
                    var childProperties = EntityConverter.GetEntityTypeInfo(property.PropertyType).SelectProperties;
                    selectParts.AddRange(childProperties.Select(childProperty => $"{alias}.[{childProperty.ColumnName}] AS [{alias}_{entityTypeName}{childProperty.ColumnName}]"));
                } else {
                    if (property.HasFormatSpecifier) {
                        selectParts.Add($"{alias}.[{property.ColumnName.FormatWith(Info.TablePart)}] AS [{alias}_{entityTypeName}{property.PropertyName}]");
                    } else {
                        var colAlias = property.IsPrimaryKey ? property.PropertyName : property.ColumnName;
                        selectParts.Add($"{alias}.[{property.ColumnName}] AS [{alias}_{entityTypeName}{colAlias}]");
                    }
                }
            }

            return selectParts;
        }

        /// <inheritdoc />
        protected override void SetParameters(DatabaseCommand command) {
            var parameters = command.GetParameterDictionary();
            if (Info.Entity != null) {
                if (!Info.ColumnName.IsNullOrWhiteSpace()) {
                    var propertyInfo = Info.TypeInfo.GetPropertyInfo(Info.ColumnName, Info.TablePart);
                    if (propertyInfo == null) {
                        throw new ArgumentException();
                    }
                    SqlParameterHelper.AddParameter(parameters, propertyInfo, Info.Entity, Info.TablePart);
                } else if (Info.TypeInfo.PrimaryKeyInfo != null) {
                    var propertyInfo = Info.TypeInfo.PrimaryKeyInfo;
                    SqlParameterHelper.AddParameter(parameters, propertyInfo, Info.Entity, Info.TablePart);
                }
            }
            foreach (var translatorParameter in _translator.Parameters) {
                var dbType = DbTypeConverter.GetSqlTypeFromType(translatorParameter.Type);
                SqlParameterHelper.AddParameter(parameters, translatorParameter.Name, dbType, translatorParameter.Value);
            }
            if (Info.ParameterList.HasAny()) {
                foreach (var pair in Info.ParameterList) {
                    if (pair.Value == null) {
                        SqlParameterHelper.AddParameter(parameters, new SqlParameter(pair.Key, DBNull.Value));
                    } else {
                        var param = new SqlParameter(pair.Key, DbTypeConverter.GetSqlTypeFromType(pair.Value.GetType())) {
                            Value = pair.Value
                        };
                        SqlParameterHelper.AddParameter(parameters, param);
                    }
                }
            }
            command.AddParameters(parameters, true);
        }

        private string GetWhereClause(QueryTranslator translator, ref bool isSimpleWhere) {
            string whereClause = null;
            if (Info.Expression != null) {
                whereClause = translator.WhereClause;
            } else if (Info.Entity != null) {
                var columnName = Info.ColumnName;
                if (string.IsNullOrWhiteSpace(columnName)) {
                    columnName = GetPrimaryKey(Info.TypeInfo.PrimaryKeyInfo.ColumnName);
                }
                whereClause = $"{BaseLevelAlias}.[{columnName}] = @{columnName}";
                isSimpleWhere = true;
            }
            if (!Info.BaseTableIDWhereClause.IsNullOrWhiteSpace()) {
                if (whereClause.IsNullOrWhiteSpace()) {
                    whereClause = Info.BaseTableIDWhereClause;
                } else {
                    whereClause = $"(({whereClause}) AND ({Info.BaseTableIDWhereClause}))";
                }
            }
            return whereClause;
        }

        private IEnumerable<string> GetSelectParts() {
            var selectParts = new List<string>();
            var typeInfoSelectPropertyNames = Info.SelectedProperties.Select(s => s.PropertyName).ToSafeHashSet();
            var selectedProperties = new List<EntityPropertyInfo>();

            if (typeInfoSelectPropertyNames.HasAny()) {
                foreach (var selectPropertyName in typeInfoSelectPropertyNames) {
                    foreach (var property in Info.TypeInfo.Properties) {
                        if (property.IsBaseChild) {
                            var baseChildPropertyInfo = EntityConverter.GetEntityTypeInfo(property.PropertyType);
                            var propertyInfo = baseChildPropertyInfo.GetPropertyInfo(selectPropertyName, Info.TablePart);
                            if (propertyInfo != null) {
                                selectedProperties.Add(propertyInfo);
                            }
                        } else if (property.PropertyName.ToLowerInvariant().Equals(selectPropertyName.ToLowerInvariant())) {
                            selectedProperties.Add(property);
                        }
                    }
                }
            } else {
                selectedProperties = Info.TypeInfo.SelectProperties.ToList();
            }

            foreach (var property in selectedProperties) {
                if (property.IsBaseChild) {
                    var childProperties = EntityConverter.GetEntityTypeInfo(property.PropertyType).SelectProperties;
                    selectParts.AddRange(childProperties.Select(childProperty => $"{BaseLevelAlias}.[{childProperty.ColumnName}] AS [{BaseLevelAlias}_{childProperty.ColumnName}]"));
                } else {
                    if (property.ColumnName.IsNullOrWhiteSpace()) { continue; }
                    selectParts.Add($"{BaseLevelAlias}.[{property.ColumnName}] AS [{BaseLevelAlias}_{property.ColumnName}]");
                }
            }
            return selectParts;
        }
    }
}