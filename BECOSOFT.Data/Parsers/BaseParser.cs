using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;

namespace BECOSOFT.Data.Parsers {
    internal abstract class BaseParser<T> where T : IEntity {
        // ReSharper disable once StaticMemberInGenericType
        protected static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Type _type = typeof(T);
        protected const string BaseLevelAlias = "ent0";
        protected readonly IOfflineTableExistsRepository OfflineTableExistsRepository;

        protected BaseParser(IOfflineTableExistsRepository tableExistsRepository) {
            OfflineTableExistsRepository = tableExistsRepository;
        }

        protected abstract List<EntityPropertyInfo> GetLinkedEntitiesProperties(EntityTypeInfo entityTypeInfo);
        protected abstract List<EntityPropertyInfo> GetLinkedEntityProperties(EntityTypeInfo entityTypeInfo);
        protected abstract List<EntityPropertyInfo> GetInverseLinkedEntityProperties(EntityTypeInfo entityTypeInfo);
        protected abstract List<EntityPropertyInfo> GetInverseLinkedBaseChildProperties(EntityTypeInfo entityTypeInfo);

        protected virtual void FinishEntity(T item) {
        }

        internal T Single(DbDataReader reader, string tablePart) {
            var entityTypeFullName = _type.FullName;
            T result = default;
            try {
                Logger.Debug("Begin Single<{0}>", entityTypeFullName);
                result = Select(reader, tablePart).Items.FirstOrDefault();
                return result;
            } finally {
                Logger.Debug("End Single<{0}> with {1}result", entityTypeFullName, result == null ? "no " : "");
            }
        }

        internal IPagedList<T> Select(DbDataReader reader, string tablePart) {
            var entityTypeFullName = _type.FullName;
            IPagedList<T> result = null;
            try {
                Logger.Debug("Begin Select<{0}>", entityTypeFullName);
                if (reader == null) {
                    Logger.Debug("{0} is NULL, returning NULL", nameof(reader));
                    return null;
                }

                result = ParseFromReader(reader, tablePart);
                return result;
            } finally {
                Logger.Debug("End Select<{0}> with {1} result items", entityTypeFullName, result?.Count ?? 0);
            }
        }

        internal IPagedList<TResult> SelectCustom<TResult>(IDataReader reader) where TResult : class {
            var pagedList = new PagedList<TResult>();
            try {
                Logger.Debug("Start SelectCustom<?>");
                if (ReadPageInfo(reader, pagedList)) {
                    reader.NextResult();
                }
                var valueContainer = new object[reader.FieldCount];
                var columnIndices = GetReaderColumnsAndIndices(reader);

                var seenIDs = new HashSet<int>();
                var mappings = EntityConverter.GetMapper<T>(BaseLevelAlias, columnIndices);
                var entityTypeInfo = EntityConverter.GetEntityTypeInfo(_type);
                var propertyContainer = new AnonymousObjectPropertyContainer<TResult>();
                var propertyMappings = mappings.Where(m => m.PropertyInfo != null)
                                               .Union(mappings.Where(m => m.ChildMappings.HasAny()).SelectMany(p => p.ChildMappings))
                                               .ToList();

                var primaryKeyInfo = entityTypeInfo.PrimaryKeyInfo ?? entityTypeInfo.TablePrimaryKeyInfo;
                var primaryKey = propertyMappings.FirstOrDefault(s => s.PropertyInfo.ColumnName.Equals(primaryKeyInfo.ColumnName));
                if (primaryKey == null) {
                    primaryKey = propertyMappings.First(s => s.PropertyInfo.ColumnName.Equals(entityTypeInfo.TablePrimaryKey));
                }
                var primaryKeySetter = propertyContainer.GetField(primaryKey.PropertyInfo)?.SetDelegate;
                var primaryKeyIndex = primaryKey.Index;
                var setters = propertyMappings.Select(m => new { m.Index, Setter = propertyContainer.GetField(m.PropertyInfo)?.SetDelegate })
                                              .Where(s => s.Setter != null && s.Index != primaryKeyIndex).ToList();

                while (reader.Read()) {
                    reader.GetValues(valueContainer);
                    var primaryKeyValue = valueContainer[primaryKeyIndex].To<int>();
                    if (!seenIDs.Add(primaryKeyValue)) { continue; }

                    var instance = TypeActivator<TResult>.Instance();
                    if (primaryKeySetter != null) {
                        primaryKeySetter(instance, primaryKeyValue);
                    }
                    foreach (var setter in setters) {
                        var value = valueContainer[setter.Index];
                        setter.Setter(instance, value);
                    }

                    pagedList.Items.Add(instance);
                }
                return pagedList;
            } finally {
                Logger.Debug("End SelectCustom<?> with {0} result items", pagedList.Count);
            }
        }

        private IPagedList<T> ParseFromReader(DbDataReader reader, string tablePart) {
            var linkedEntitiesTree = EntityConverter.GetLinkedEntitiesTree(_type, true);
            var pagedList = new PagedList<T>();
            if (ReadPageInfo(reader, pagedList)) {
                reader.NextResult();
            }

            var container = new ParseContainer(linkedEntitiesTree, OfflineTableExistsRepository);
            FillParseContainer(container);

            var linkedEntityDelegateCreators = new Dictionary<Type, DelegateCreator.Projector>();
            var foundAnyMappings = false;
            var hasRows = false;
            do {
                if (!hasRows && reader.HasRows) { hasRows = true; }
                var valueContainer = new object[reader.FieldCount];
                var columnIndices = GetReaderColumnsAndIndices(reader);
                var typeIndex = GetTypeIndex(columnIndices);
                var mappings = GetMappings(container, typeIndex, columnIndices);
                if (mappings.IsEmpty()) {
                    continue;
                }

                foundAnyMappings = true;
                var type = container.TypeData.TryGetValueWithDefault(typeIndex);
                var objConverter = GetProjector(linkedEntityDelegateCreators, type);

                var dataEntry = container.Data[typeIndex];
                var getter = container.ForeignKeyGetters.TryGetValueWithDefault(typeIndex);

                while (reader.Read()) {
                    reader.GetValues(valueContainer);
                    var obj = objConverter(valueContainer, mappings);
                    Store(obj, dataEntry, getter);
                }
            } while (reader.NextResult());

            if (hasRows && !foundAnyMappings) {
                throw new FailedResultConversionException();
            }

            if (container.Data.IsEmpty()) {
                return pagedList;
            }

            foreach (var entity in container.RetrieveConvertedEntities(tablePart)) {
                FinishEntity(entity);
                pagedList.Items.Add(entity);
            }

            return pagedList;
        }

        private List<PropertyMapping> GetMappings(ParseContainer container, TypeIndex tempIndex, Dictionary<string, int> columnIndices) {
            var type = container.TypeData.TryGetValueWithDefault(tempIndex);
            if (type == null) {
                return new List<PropertyMapping>();
            }
            var lowerCaseTypeName = type.GetNameWithoutGenerics().ToLowerInvariant();
            var levelAlias = tempIndex.IsMain() ? BaseLevelAlias : $"ent{tempIndex.Level}_{tempIndex.Index}_{lowerCaseTypeName}";
            return EntityConverter.GetMapper(type, levelAlias, columnIndices);
        }

        private static DelegateCreator.Projector GetProjector(Dictionary<Type, DelegateCreator.Projector> linkedEntityDelegateCreators, Type type) {
            if (!linkedEntityDelegateCreators.TryGetValue(type, out var objConverter)) {
                objConverter = DelegateCreator.CreateConvertEntityDelegate(type);
                linkedEntityDelegateCreators.Add(type, objConverter);
            }

            return objConverter;
        }

        private void FillParseContainer(ParseContainer container) {
            foreach (var subNode in container.FlattenedTree) {
                var typeIndex = new TypeIndex(subNode.Level, subNode.Value.Index);
                var type = subNode.Value.EntityTypeInfo.EntityType;
                container.TypeData.Add(typeIndex, type);
                container.Data.Add(typeIndex, new Dictionary<int, List<object>>());
                if (typeIndex.IsMain()) { continue; }
                var property = GetParentForeignKeyPropertyInfo(subNode, type);
                if (property != null) {
                    container.ForeignKeyGetters.Add(typeIndex, property.Getter);
                }
            }
        }

        private EntityPropertyInfo GetParentForeignKeyPropertyInfo(ITreeNode<EntityTreeNode> subNode, Type type) {
            var parentEntityInfo = subNode.Parent.Value.EntityTypeInfo;
            var parentLinkedProperty = GetLinkedEntitiesProperties(parentEntityInfo).FirstOrDefault(e => e.BaseEntityType == type);
            if (parentLinkedProperty == null) {
                parentLinkedProperty = GetLinkedEntityProperties(parentEntityInfo).FirstOrDefault(e => e.BaseEntityType == type);
            }
            var parentForeignKeyName = parentLinkedProperty?.ForeignKeyColumn;
            if (parentForeignKeyName != null) {
                return subNode.Value.EntityTypeInfo.GetPropertyInfo(parentForeignKeyName, null);
            }
            var linkedPropertyType = GetInverseLinkedEntityProperties(parentEntityInfo).FirstOrDefault(e => e.BaseEntityType == type)?.PropertyType;
            if (linkedPropertyType != null) {
                var entityTypeInfo = EntityConverter.GetEntityTypeInfo(linkedPropertyType);
                return entityTypeInfo.PrimaryKeyInfo ?? entityTypeInfo.TablePrimaryKeyInfo;
            }
            var linkedBaseChildPropertyType = GetInverseLinkedBaseChildProperties(parentEntityInfo).FirstOrDefault(e => e.BaseEntityType == type)?.PropertyType;
            if (linkedBaseChildPropertyType != null) {
                var entityTypeInfo = EntityConverter.GetEntityTypeInfo(linkedBaseChildPropertyType);
                return entityTypeInfo.PrimaryKeyInfo ?? entityTypeInfo.TablePrimaryKeyInfo;
            }
            return null;
        }

        private static bool ReadPageInfo<TModel>(IDataReader reader, PagedList<TModel> pagedList) {
            if (reader.FieldCount != 3) { return false; }
            var columnIndices = GetReaderColumnsAndIndices(reader);
            if (!columnIndices.ContainsKey("pagertotalitemcount")) { return false; }
            var valueContainer = new object[reader.FieldCount];
            reader.Read();
            reader.GetValues(valueContainer);
            pagedList.SetPageInfo(valueContainer[columnIndices["pagercurrentskip"]].To<int>(),
                                  valueContainer[columnIndices["pagercurrenttake"]].To<int>(),
                                  valueContainer[columnIndices["pagertotalitemcount"]].To<int>());
            return true;
        }

        private static Dictionary<string, int> GetReaderColumnsAndIndices(IDataReader reader) {
            var table = reader.GetSchemaTable() ?? new DataTable();
            return table.AsEnumerable()
                        .GroupBy(row => row["ColumnName"].ToString().ToLowerInvariant())
                        .ToDictionary(r => r.Key, row => row.First()["ColumnOrdinal"].To<int>());
        }

        private static TypeIndex GetTypeIndex(Dictionary<string, int> columnIndices) {
            foreach (var columnIndex in columnIndices) {
                var column = columnIndex.Key.Split('_');
                if (column.Length <= 1) { continue; }
                var level = column[0].Substring(3).To<int>();
                var index = level == 0 ? 0 : column[1].To<int>();
                return new TypeIndex(level, index);
            }

            return new TypeIndex(0, 0);
        }

        private static void Store(object obj, Dictionary<int, List<object>> entityDataEntry, Func<object, object> getter) {
            var parentID = getter == null ? 0 : getter(obj).To<int>();
            if (!entityDataEntry.TryGetValue(parentID, out var dataEntry)) {
                dataEntry = new List<object>();
                entityDataEntry.Add(parentID, dataEntry);
            }
            dataEntry.Add(obj);
        }

        [DebuggerDisplay("{Level} - {Index}")]
        private readonly struct TypeIndex : IEquatable<TypeIndex> {
            internal byte Level { get; }
            internal byte Index { get; }

            public TypeIndex(int level, int index)
                : this(level.To<byte>(), index.To<byte>()) {
            }

            public TypeIndex(byte level, byte index) {
                Level = level;
                Index = index;
            }

            public bool IsMain() => Level == 0 && Index == 0;

            public override int GetHashCode() {
                unchecked { return (Level * 397) ^ Index; }
            }

            public bool Equals(TypeIndex other) {
                return Level == other.Level && Index == other.Index;
            }

            public override bool Equals(object obj) {
                if (ReferenceEquals(null, obj)) { return false; }
                return obj is TypeIndex index && Equals(index);
            }

            public static bool operator ==(TypeIndex left, TypeIndex right) {
                return left.Equals(right);
            }

            public static bool operator !=(TypeIndex left, TypeIndex right) {
                return !(left == right);
            }
        }

        private class ParseContainer {
            private readonly IOfflineTableExistsRepository _tableExistsRepository;
            public Dictionary<TypeIndex, Type> TypeData { get; }
            public Dictionary<TypeIndex, Dictionary<int, List<object>>> Data { get; }
            public Dictionary<TypeIndex, Func<object, object>> ForeignKeyGetters { get; }
            public List<TreeNode<EntityTreeNode>> FlattenedTree { get; }

            internal ParseContainer(ITreeNode<EntityTreeNode> tree, IOfflineTableExistsRepository tableExistsRepository) {
                _tableExistsRepository = tableExistsRepository;
                TypeData = new Dictionary<TypeIndex, Type>();
                Data = new Dictionary<TypeIndex, Dictionary<int, List<object>>>();
                ForeignKeyGetters = new Dictionary<TypeIndex, Func<object, object>>();
                FlattenedTree = tree.Flatten().ToList();
            }

            public List<T> RetrieveConvertedEntities(string tablePart) {
                Couple(tablePart);
                var mainEntitiesContainer = Data.TryGetValueWithDefault(new TypeIndex(0, 0));
                var mainEntities = mainEntitiesContainer.TryGetValueWithDefault(0);
                return mainEntities?.Cast<T>().ToList() ?? new List<T>(0);
            }

            private void Couple(string tablePart) {
                foreach (var nodesByParentTypeIndex in FlattenedTree.Where(e => e.Parent != null).GroupBy(e => new TypeIndex(e.Parent.Level, e.Parent.Value.Index))) {
                    var parentData = Data.TryGetValueWithDefault(nodesByParentTypeIndex.Key);
                    if (parentData.IsEmpty()) {
                        continue;
                    }
                    var parentEntities = parentData.Values.SelectMany(v => v).ToList();
                    foreach (var node in nodesByParentTypeIndex) {
                        Couple(parentEntities, node, node.Level, tablePart);
                    }
                }
            }

            private void Couple(List<object> entities, TreeNode<EntityTreeNode> node, int level, string tablePart) {
                var treeNodeValue = node.Value;
                var propertyInfo = treeNodeValue.EntityPropertyInfo;
                EntityPropertyInfo keyProperty;
                EntityPropertyInfo baseChildProperty = null;
                if (propertyInfo.IsInverseLinkedEntity) {
                    keyProperty = propertyInfo.Parent.GetPropertyInfo(propertyInfo.ForeignKeyColumn, null);
                    if (keyProperty.Parent.EntityType.IsSubclassOf(typeof(BaseChild))) {
                        var parentTypeInfo = node.Parent.Value.EntityTypeInfo;
                        baseChildProperty = parentTypeInfo.Properties.FirstOrDefault(p => p.PropertyType == keyProperty.Parent.EntityType);
                    }
                } else {
                    keyProperty = GetPrimaryKeyProperty(propertyInfo);
                }

                if (keyProperty == null) { return; }

                var typeIndex = new TypeIndex(level, treeNodeValue.Index);
                var type = TypeData.TryGetValueWithDefault(typeIndex);
                if (!_tableExistsRepository.TableExists(type, tablePart)) {
                    foreach (var entity in entities) {
                        propertyInfo.Setter(entity, null);
                    }
                    return;
                }
                var values = Data.TryGetValueWithDefault(typeIndex);
                if (values.IsEmpty()) { return; }

                foreach (var entity in entities) {
                    var setterSubject = entity;
                    if (baseChildProperty != null) {
                        setterSubject = baseChildProperty.Getter(entity);
                    }
                    var key = keyProperty.Getter(setterSubject).To<int>();
                    var linkedEntities = values.TryGetValueWithDefault(key);

                    if (linkedEntities == null) { continue; }
                    if (propertyInfo.AreLinkedBaseResults || propertyInfo.AreLinkedEntities) {
                        var observerList = linkedEntities.ToObserverList();
                        propertyInfo.Setter(setterSubject, observerList);
                    } else {
                        var linkedEntity = linkedEntities.FirstOrDefault();
                        if (linkedEntity == null) { continue; }
                        propertyInfo.Setter(setterSubject, linkedEntity);
                    }
                }
            }

            private EntityPropertyInfo GetPrimaryKeyProperty(EntityPropertyInfo propertyInfo) {
                if (propertyInfo.Parent?.PrimaryKeyInfo != null) {
                    return propertyInfo.Parent.PrimaryKeyInfo;
                }

                if (propertyInfo.Parent?.TablePrimaryKeyInfo != null) {
                    return propertyInfo.Parent.TablePrimaryKeyInfo;
                }

                return propertyInfo.Parent?.GetPropertyInfo(propertyInfo.PrimaryKeyColumn, null);
            }
        }
    }
}