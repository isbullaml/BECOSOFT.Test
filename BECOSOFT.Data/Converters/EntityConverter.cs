using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BECOSOFT.Data.Converters {
    /// <summary>
    /// Class for converting entities
    /// </summary>
    internal static class EntityConverter {
        private const string ColumnDivider = "_";
        private static readonly ConcurrentDictionary<Type, EntityTypeInfo> EntityTypeInfo = new ConcurrentDictionary<Type, EntityTypeInfo>();

        internal static List<PropertyMapping> GetMapper<T>(string aliasPrefix, Dictionary<string, int> columnIndices) {
            return GetMapper(typeof(T), aliasPrefix, columnIndices);
        }

        internal static List<PropertyMapping> GetMapper(Type type, string aliasPrefix, Dictionary<string, int> columnIndices) {
            var result = new List<PropertyMapping>();
            var entityInfo = GetEntityTypeInfo(type);
            var useDividedPrefix = entityInfo.IsBaseEntity || !aliasPrefix.IsNullOrWhiteSpace();
            var dividedPrefix = aliasPrefix + ColumnDivider;
            foreach (var property in entityInfo.Properties) {
                if (property.IsLinkedEntity || property.AreLinkedEntities || property.IsLinkedBaseResult || property.AreLinkedBaseResults) {
                    continue;
                }

                var propertyMapping = new PropertyMapping(property);
                if (property.IsBaseChild) {
                    var childMappings = GetMapper(property.PropertyType, aliasPrefix, columnIndices);
                    if (childMappings.IsEmpty()) {
                        continue;
                    }
                    propertyMapping.SetChildMappings(childMappings);
                } else {
                    string column;
                    if (useDividedPrefix) {
                        column = dividedPrefix + property.LowerCaseColumnName;
                    } else {
                        column = property.LowerCaseColumnName;
                    }
                    if (!columnIndices.TryGetValue(column, out var index)) {
                        if (!columnIndices.TryGetValue(property.LowerCaseColumnName, out index)) {
                            if (!property.IsPrimaryKey || (property.IsPrimaryKey && !columnIndices.TryGetValue(dividedPrefix + "id", out index) && !columnIndices.TryGetValue("id", out index))) {
                                continue;
                            }
                        }
                    }
                    propertyMapping.Index = index;
                }
                result.Add(propertyMapping);
            }
            return result;
        }

        internal static T ConvertEntity<T>(object[] valueContainer, List<PropertyMapping> mappings) {
            var entity = TypeActivator<T>.Instance();
            foreach (var mapping in mappings) {
                var propertyInfo = mapping.PropertyInfo;
                if (mapping.ChildMappings != null) {
                    var baseChild = mapping.BaseChildProjector(valueContainer, mapping.ChildMappings);
                    propertyInfo.Setter(entity, baseChild);
                    continue;
                }
                propertyInfo.Setter(entity, valueContainer[mapping.Index]);
            }

            return entity;
        }

        /// <summary>
        /// Retrieve the entity type info of a type
        /// </summary>
        /// <param name="type">The type</param>
        /// <returns>The entity type info</returns>
        internal static EntityTypeInfo GetEntityTypeInfo(Type type) {
            if (!EntityTypeInfo.TryGetValue(type, out var entityTypeInfo)) {
                entityTypeInfo = new EntityTypeInfo(type);
                EntityTypeInfo.TryAdd(type, entityTypeInfo);
            }
            return entityTypeInfo;
        }

        /// <summary>
        /// Get a treenode of all linked entities of the type
        /// </summary>
        /// <param name="type">The type</param>
        /// <param name="includeInverseLinked">Value indicating whether inverse-linked entites should be included</param>
        /// <returns>The treenode of linken entities</returns>
        internal static TreeNode<EntityTreeNode> GetLinkedEntitiesTree(Type type, bool includeInverseLinked) {
            var entityTypeInfo = GetEntityTypeInfo(type);
            return GetLinkedEntitiesTree(entityTypeInfo, includeInverseLinked);
        }

        /// <summary>
        /// Get a treenode of all linked entities of the type
        /// </summary>
        /// <param name="entityTypeInfo">The <see cref="EntityTypeInfo"/></param>
        /// <param name="includeInverseLinked">Value indicating whether inverse-linked entites should be included</param>
        /// <returns>The treenode of linken entities</returns>
        internal static TreeNode<EntityTreeNode> GetLinkedEntitiesTree(EntityTypeInfo entityTypeInfo, bool includeInverseLinked) {
            var result = new TreeNode<EntityTreeNode>(new EntityTreeNode(entityTypeInfo));
            GetLinkedEntities(result, includeInverseLinked);
            return result;
        }

        private static void GetLinkedEntities(ITreeNode<EntityTreeNode> node, bool includeInverseLinked) {
            foreach (var linkedEntity in node.Value.EntityTypeInfo.LinkedEntitiesProperties) {
                var counter = node.GetLevelCount(node.Level + 1) + 1;
                var propertyEntityTypeInfo = GetEntityTypeInfo(linkedEntity.BaseEntityType);
                var newNode = node.AddChild(new EntityTreeNode(counter, propertyEntityTypeInfo, linkedEntity, EntityTreeNodeType.LinkedEntities));
                GetLinkedEntities(newNode, includeInverseLinked);
            }

            foreach (var linkedEntityProperty in node.Value.EntityTypeInfo.LinkedEntityProperties) {
                var counter = node.GetLevelCount(node.Level + 1) + 1;
                var propertyEntityTypeInfo = GetEntityTypeInfo(linkedEntityProperty.BaseEntityType);
                var newNode = node.AddChild(new EntityTreeNode(counter, propertyEntityTypeInfo, linkedEntityProperty, EntityTreeNodeType.LinkedEntity));
                GetLinkedEntities(newNode, includeInverseLinked);
            }

            foreach (var linkedBaseResultsProperty in node.Value.EntityTypeInfo.LinkedBaseResultsProperties) {
                var counter = node.GetLevelCount(node.Level + 1) + 1;
                var propertyEntityTypeInfo = GetEntityTypeInfo(linkedBaseResultsProperty.BaseEntityType);
                var newNode = node.AddChild(new EntityTreeNode(counter, propertyEntityTypeInfo, linkedBaseResultsProperty, EntityTreeNodeType.LinkedEntities));
                GetLinkedEntities(newNode, includeInverseLinked);
            }

            foreach (var linkedBaseResultProperty in node.Value.EntityTypeInfo.LinkedBaseResultProperties) {
                var counter = node.GetLevelCount(node.Level + 1) + 1;
                var propertyEntityTypeInfo = GetEntityTypeInfo(linkedBaseResultProperty.BaseEntityType);
                var newNode = node.AddChild(new EntityTreeNode(counter, propertyEntityTypeInfo, linkedBaseResultProperty, EntityTreeNodeType.LinkedEntity));
                GetLinkedEntities(newNode, includeInverseLinked);
            }

            if (!includeInverseLinked) {
                return;
            }

            GetInverseLinkedEntities(node);
        }

        private static void GetInverseLinkedEntities(ITreeNode<EntityTreeNode> node) {
            // inverse linked entity properties are added without recursively searching for sub entities
            var inverseLinkedEntityProperties = node.Value.EntityTypeInfo.InverseLinkedEntityProperties;
            foreach (var linkedEntityProperty in inverseLinkedEntityProperties) {
                var counter = node.GetLevelCount(node.Level + 1) + 1;
                var propertyEntityTypeInfo = GetEntityTypeInfo(linkedEntityProperty.BaseEntityType);
                var newNode = node.AddChild(new EntityTreeNode(counter, propertyEntityTypeInfo, linkedEntityProperty, EntityTreeNodeType.InverseLinkedEntity));
                GetInverseLinkedEntities(newNode);
            }

            foreach (var linkedBaseChildProperty in node.Value.EntityTypeInfo.LinkedBaseChildProperties) {
                var baseChildPropertyEntityTypeInfo = GetEntityTypeInfo(linkedBaseChildProperty.BaseEntityType);

                foreach (var linkedEntityProperty in baseChildPropertyEntityTypeInfo.InverseLinkedEntityProperties) {
                    var counter = node.GetLevelCount(node.Level + 1) + 1;
                    var propertyEntityTypeInfo = GetEntityTypeInfo(linkedEntityProperty.BaseEntityType);
                    var newNode = node.AddChild(new EntityTreeNode(counter, propertyEntityTypeInfo, linkedEntityProperty, EntityTreeNodeType.InverseLinkedEntity));
                    GetLinkedEntities(newNode, true);
                }
            }
        }
    }
}