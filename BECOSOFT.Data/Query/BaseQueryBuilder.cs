using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace BECOSOFT.Data.Query {
    /// <summary>
    /// Class for building queries
    /// </summary>
    public abstract class BaseQueryBuilder : IQueryBuilderFactoryService {
        protected static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        protected readonly IOfflineTableExistsRepository OfflineTableExistsRepository;

        protected BaseQueryBuilder(IOfflineTableExistsRepository tableExistsRepository) {
            OfflineTableExistsRepository = tableExistsRepository;
        }

        /// <summary>
        /// The info to build the query
        /// </summary>
        public QueryInfo Info { get; private set; }

        /// <summary>
        /// The alias for the base-entity
        /// </summary>
        public const string BaseLevelAlias = "ENT0";

        /// <summary>
        /// The prefix for all other entities linked to the base-entity
        /// </summary>
        public const string LevelAlias = "ENT";

        public int AmountPerBatch { get; protected set; }

        public abstract QueryType Type { get; }

        public BaseQueryBuilder Initialize(QueryInfo info) {
            Info = info;
            InitializeInternal(info);
            return this;
        }

        protected virtual void InitializeInternal(QueryInfo info) {
        }

        /// <summary>
        /// Prepares a command and sets the parameters
        /// </summary>
        /// <param name="command">The command to prepare</param>
        public void PrepareCommand(DatabaseCommand command) {
            Preparation();
            var generatedQuery = GenerateQuery();
            command.BulkCopyTempTables = Info.BulkCopyTempTables;
            command.TempTables = Info.TempTables;
            command.TablePart = Info.TablePart;
            command.CommandText = generatedQuery.ToString();
            SetParameters(command);
        }

        /// <summary>
        /// This method provides a way to prepare data before <see cref="GenerateQuery"/> is called.
        /// </summary>
        protected virtual void Preparation() {
        }

        /// <summary>
        /// This method returns the query that will be executed.
        /// </summary>
        /// <returns>A <see cref="StringBuilder"/> containing the resulting query.</returns>
        protected abstract StringBuilder GenerateQuery();

        /// <summary>
        /// This method provides a way to set all parameters on the <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="command">The command to set the parameters on</param>
        protected virtual void SetParameters(DatabaseCommand command) {
        }

        /// <summary>
        /// This method provides a way to set all parameters on the <see cref="DatabaseCommand"/>.
        /// </summary>
        /// <param name="command">The command to set the parameters on</param>
        /// <param name="entity">The entity containing the values</param>
        public virtual void SetParameters(DatabaseCommand command, object entity) {
        }

        public virtual Tuple<int, Dictionary<Tuple<FieldType, Type>, HashSet<object>>> CalculateAmountPerBatch<TEntity>(IEnumerable<TEntity> entities) {
            return Tuple.Create(AmountPerBatch, default(Dictionary<Tuple<FieldType, Type>, HashSet<object>>));
        }

        /// <summary>
        /// Gets the formatted table-name with the table-part
        /// </summary>
        /// <param name="tableName">The unformatted table-name</param>
        /// <returns>The formatted table-name</returns>
        protected string GetTableName(string tableName) {
            return Info.TablePart.IsNullOrWhiteSpace() ? tableName : string.Format(tableName, Info.TablePart);
        }

        protected string GetPrimaryKey(string primaryKeyColumn) {
            if (primaryKeyColumn.IsNullOrEmpty()) { return null; }
            return Info.TablePart.IsNullOrWhiteSpace() ? primaryKeyColumn : string.Format(primaryKeyColumn, Info.TablePart);
        }

        /// <summary>
        /// Adds a query for creating temporary tables
        /// </summary>
        /// <param name="queryBuilder">The builder to add the query to</param>
        protected void AddTempTableQueries(StringBuilder queryBuilder) {
            if (Info.TempTables.IsEmpty()) { return; }
            var sb = new StringBuilder();
            foreach (var tempTable in Info.TempTables) {
                sb.AppendLine(tempTable.GetCreationScript());
                sb.Append(tempTable.GetFillScript()).AppendLine();
            }
            queryBuilder.Append(sb);
        }

        /// <summary>
        /// Adds a query for dropping temporary tables
        /// </summary>
        /// <param name="queryBuilder">The builder to add the query to</param>
        protected void AddDropTempTableQueries(StringBuilder queryBuilder) {
            if (Info.TempTables.IsEmpty() && Info.BulkCopyTempTables.IsEmpty() && Info.DropOnlyTempTables.IsEmpty()) {
                return;
            }
            var sb = new StringBuilder();
            var hashset = new HashSet<string>();
            if (Info.TempTables.HasAny()) {
                foreach (var tempTable in Info.TempTables) {
                    if (hashset.Contains(tempTable.TableName)) { continue; }
                    sb.AppendLine().AppendLine("DROP TABLE {0};", tempTable.TableName);
                    hashset.Add(tempTable.TableName);
                }
            }
            if (Info.BulkCopyTempTables.HasAny()) {
                foreach (var tempTable in Info.BulkCopyTempTables) {
                    if (hashset.Contains(tempTable.TableName)) { continue; }
                    sb.AppendLine().AppendLine("DROP TABLE {0};", tempTable.TableName);
                    hashset.Add(tempTable.TableName);
                }
            }
            if (Info.DropOnlyTempTables.HasAny()) {
                foreach (var tempTable in Info.DropOnlyTempTables) {
                    if (hashset.Contains(tempTable)) { continue; }
                    sb.AppendLine().AppendLine("DROP TABLE {0};", tempTable);
                    hashset.Add(tempTable);
                }
            }
            queryBuilder.Append(sb);
        }

        /// <summary>
        /// Gets a SELECT-statement for a type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="tableAlias">The alias for the table</param>
        /// <param name="alias">The alias for the column</param>
        /// <param name="propertiesToExclude">Exclude these properties from the select statement</param>
        /// <returns>The prepared statement</returns>
        public static string GetPreparedSelect(Type type, string tableAlias, string alias = null, List<EntityPropertyInfo> propertiesToExclude = null) {
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(type);
            if (alias == null) {
                alias = BaseLevelAlias;
            }

            var aliasPart = alias.IsNullOrEmpty() ? "" : alias + "_";
            var tableAliasPart = tableAlias.IsNullOrEmpty() ? "" : tableAlias + ".";
            var propertiesToSelect = entityTypeInfo.SelectProperties;
            var selectList = new List<string>();
            foreach (var property in propertiesToSelect) {
                if (propertiesToExclude.HasAny() && propertiesToExclude.Contains(property)) { continue; }
                if (property.IsBaseChild) {
                    var childProperties = EntityConverter.GetEntityTypeInfo(property.PropertyType).SelectProperties;
                    foreach (var childProperty in childProperties) {
                        if (propertiesToExclude.HasAny() && propertiesToExclude.Contains(childProperty)) { continue; }
                        selectList.Add($"{tableAliasPart}[{childProperty.ColumnName}] AS [{aliasPart}{childProperty.ColumnName}]");
                    }
                } else {
                    selectList.Add($"{tableAliasPart}[{property.ColumnName}] AS [{aliasPart}{property.ColumnName}]");
                }
            }
            return string.Join(",", selectList);
        }

        /// <summary>
        /// Gets a SELECT-statement for a type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="tableAlias">The alias for the table</param>
        /// <param name="highestParentEntityTypeInfo">The <see cref="EntityTypeInfo"/> of the highest parent</param>
        /// <param name="level"></param>
        /// <param name="foreignKeyColumn">Specifies the name of the foreignKeyColumn to retrieve the correct node from the LinkedEntitiesTree,
        /// Useful if a model has multiple inverse linked entities of the same type.</param>
        /// <returns>The prepared statement</returns>
        public static string GetPreparedSelect(Type type, string tableAlias, EntityTypeInfo highestParentEntityTypeInfo, int level, string foreignKeyColumn = "") {
            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(highestParentEntityTypeInfo, true);
            TreeNode<EntityTreeNode> node;
            if (level == 0) {
                node = linkedEntitiesTree.Flatten().FirstOrDefault(e => e.Level == level);
            } else {
                if (foreignKeyColumn.HasValue()) {
                    node = linkedEntitiesTree.Flatten().FirstOrDefault(e => e.Level == level && e.Value.EntityTypeInfo.EntityType == type && e.Value.EntityPropertyInfo.ForeignKeyColumn.EqualsIgnoreCase(foreignKeyColumn));
                } else {
                    node = linkedEntitiesTree.Flatten().FirstOrDefault(e => e.Level == level && e.Value.EntityTypeInfo.EntityType == type);
                }
            }
            var tableAliasPart = tableAlias.IsNullOrEmpty() ? "" : tableAlias + ".";
            var propertiesToSelect = node.Value.EntityTypeInfo.SelectProperties;
            var selectList = new List<string>();
            foreach (var property in propertiesToSelect) {
                if (property.IsBaseChild) {
                    var childProperties = EntityConverter.GetEntityTypeInfo(property.PropertyType).SelectProperties;
                    foreach (var childProperty in childProperties) {
                        selectList.Add($"{tableAliasPart}[{childProperty.ColumnName}] AS [{GetPropertyAlias(node, childProperty)}]");
                    }
                } else {
                    selectList.Add($"{tableAliasPart}[{property.ColumnName}] AS [{GetPropertyAlias(node, property)}]");
                }
            }
            return string.Join(",", selectList);
        }

        /// <summary>
        /// Gets the full column alias for a property
        /// </summary>
        /// <param name="selector">The property selector</param>
        /// <param name="level">The level on which the property is</param>
        /// <param name="highestParentType">The <see cref="Type"/> of the highest parent</param>
        /// <param name="tablePart"></param>
        /// <returns>The prepared statement</returns>
        public static string GetPropertyAlias<T>(Expression<Func<T, object>> selector, int level, Type highestParentType, string tablePart) {
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(highestParentType);
            var propertyInfo = selector.GetPropertyWithParent();
            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(entityTypeInfo, true);
            var flatTree = linkedEntitiesTree.Flatten();
            TreeNode<EntityTreeNode> node;
            if (level == 0) {
                node = flatTree.FirstOrDefault(e => e.Level == level);
            } else {
                node = flatTree.FirstOrDefault(e => e.Level == level && e.Value.EntityPropertyInfo.Equals(propertyInfo.Item1));
            }

            var property = node.Value.EntityTypeInfo.GetPropertyInfo(propertyInfo.Item2.ColumnName, tablePart);
            return GetPropertyAlias(node, property);
        }

        /// <summary>
        /// Gets the full column alias for a property
        /// </summary>
        /// <param name="node"></param>
        /// <param name="property">The property</param>
        /// <returns>The prepared statement</returns>
        private static string GetPropertyAlias(TreeNode<EntityTreeNode> node, EntityPropertyInfo property) {
            var infix = node.Level == 0 ? "" : $"_{node.Value.Index}_{node.Value.EntityTypeInfo.EntityType.GetNameWithoutGenerics()}";
            var alias = $"{LevelAlias}{node.Level}{infix}";
            var aliasPart = alias.IsNullOrEmpty() ? "" : alias + "_";
            return $"{aliasPart}{property.ColumnName}";
        }
    }
}