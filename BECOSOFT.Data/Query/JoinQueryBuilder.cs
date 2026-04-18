using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using System.Collections.Generic;

namespace BECOSOFT.Data.Query {
    internal class JoinQueryBuilder : IJoinQueryBuilder {
        private readonly IOfflineTableExistsRepository _tableExistsRepository;

        internal JoinQueryBuilder(IOfflineTableExistsRepository tableExistsRepository) {
            _tableExistsRepository = tableExistsRepository;
        }

        /// <summary>
        /// The prefix for all other entities linked to the base-entity
        /// </summary>
        internal const string LevelAlias = "ENT";

        public List<string> AddLinkedEntitiesJoinParts(ITreeNode<EntityTreeNode> linkedEntitiesTree, bool includeLinkedEntities, bool includeLinkedEntity,
                                                       string tablePart) {
            var joinParts = new List<string>();
            foreach (var node in linkedEntitiesTree) {
                var entityTypeInfo = node.Parent.Value.EntityTypeInfo;
                if (node.Value.Type == EntityTreeNodeType.InverseLinkedEntity) { continue; }
                if (includeLinkedEntities) {
                    AddJoinFromLinkedProperties(linkedEntitiesTree, joinParts, node, entityTypeInfo.LinkedEntitiesProperties, tablePart);
                    AddJoinFromLinkedProperties(linkedEntitiesTree, joinParts, node, entityTypeInfo.LinkedBaseResultsProperties, tablePart);
                }
                if (includeLinkedEntity) {
                    AddJoinFromLinkedProperties(linkedEntitiesTree, joinParts, node, entityTypeInfo.LinkedEntityProperties, tablePart);
                    AddJoinFromLinkedProperties(linkedEntitiesTree, joinParts, node, entityTypeInfo.LinkedBaseResultProperties, tablePart);
                }
                joinParts.AddRange(AddLinkedEntitiesJoinParts(node, includeLinkedEntities, includeLinkedEntity, tablePart));
            }
            return joinParts;
        }

        private void AddJoinFromLinkedProperties(ITreeNode<EntityTreeNode> linkedEntitiesTree,
                                                 List<string> joinParts, ITreeNode<EntityTreeNode> node, IEnumerable<EntityPropertyInfo> properties,
                                                 string tablePart) {
            foreach (var entitiesProperty in properties) {
                if (entitiesProperty.BaseEntityType != node.Value.EntityTypeInfo.EntityType
                    || !entitiesProperty.ForeignKeyColumn.Equals(node.Value.EntityPropertyInfo.ForeignKeyColumn)) {
                    continue;
                }
                var didJoin = AddToJoinParts(linkedEntitiesTree, joinParts, node, entitiesProperty, tablePart);
                if (!didJoin) { break; }
                foreach (var childNode in node) {
                    AddJoinFromLinkedProperties(node, joinParts, childNode, childNode.Parent.Value.EntityTypeInfo.InverseLinkedEntityProperties, tablePart);
                }
            }
        }

        private bool AddToJoinParts(ITreeNode<EntityTreeNode> linkedEntitiesTree,
                                    ICollection<string> joinParts, ITreeNode<EntityTreeNode> node,
                                    EntityPropertyInfo entitiesProperty, string tablePart) {
            var entityTypeInfo = node.Value.EntityTypeInfo;
            if (!_tableExistsRepository.TableExists(entityTypeInfo.EntityType, tablePart)) {
                return false;
            }
            var tableName = GetTableName(entityTypeInfo.TableDefinition.FullTableName, tablePart);
            var level = linkedEntitiesTree.Level;
            var parentIndex = node.Parent.Value.Index.ToString();
            var nodeIndex = node.Value.Index;
            var infix = $"{(level == 0 ? "" : "_" + parentIndex)}.";
            string join;
            var nodeParentEntityTypeInfo = node.Parent.Value.EntityTypeInfo;
            if (entitiesProperty.IsInverseLinkedEntity) {
                var basePropertyInfo = nodeParentEntityTypeInfo.GetPropertyInfo(entitiesProperty.ForeignKeyColumn, null);
                var colName = basePropertyInfo?.ColumnName;
                string primaryKeyColumnName;
                if (entityTypeInfo.IsBaseEntity) {
                    primaryKeyColumnName = entityTypeInfo.PrimaryKeyInfo.ColumnName;
                } else {
                    primaryKeyColumnName = entityTypeInfo.TablePrimaryKeyInfo.ColumnName;
                }
                join = $"LEFT JOIN {tableName} {LevelAlias}{node.Level}_{nodeIndex} " +
                       $"ON {LevelAlias}{node.Level}_{nodeIndex}.{GetPrimaryKey(primaryKeyColumnName, tablePart)} " +
                       $"= {LevelAlias}{level}{infix}{colName}";
            } else {
                string primaryKeyColumnName;
                if (entityTypeInfo.IsBaseEntity) {
                    if (nodeParentEntityTypeInfo.IsBaseResult && (node.Value.EntityPropertyInfo.AreLinkedBaseResults || node.Value.EntityPropertyInfo.IsLinkedBaseResult)) {
                        primaryKeyColumnName = node.Value.EntityPropertyInfo.ForeignKeyColumn;
                    } else {
                        primaryKeyColumnName = nodeParentEntityTypeInfo.PrimaryKeyInfo.ColumnName;
                    }
                } else {
                    primaryKeyColumnName = nodeParentEntityTypeInfo.GetPropertyInfo(entitiesProperty.PrimaryKeyColumn, tablePart).ColumnName;
                }
                join = $"LEFT JOIN {tableName} {LevelAlias}{node.Level}_{nodeIndex} " +
                       $"ON {LevelAlias}{node.Level}_{nodeIndex}.{entitiesProperty.ForeignKeyColumn} " +
                       $"= {LevelAlias}{level}{infix}{GetPrimaryKey(primaryKeyColumnName, tablePart)}";
            }
            joinParts.Add(join);
            return true;
        }

        private static string GetTableName(string tableName, string tablePart) {
            return tablePart.IsNullOrWhiteSpace() ? tableName : string.Format(tableName, tablePart);
        }

        private static string GetPrimaryKey(string primaryKeyColumn, string tablePart) {
            if (primaryKeyColumn.IsNullOrEmpty()) { return null; }
            return tablePart.IsNullOrWhiteSpace() ? primaryKeyColumn : string.Format(primaryKeyColumn, tablePart);
        }
    }
}