using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace BECOSOFT.Data.Query.Builders {
    /// <summary>
    /// Querybuilder for an UPDATE-statement
    /// </summary>
    internal class UpdateBaseQueryBuilder : BaseQueryBuilder {
        private const int ParameterLimit = 2000;
        private List<UpdateMapping> _defaultUpdateProperties;

        public override QueryType Type => QueryType.Update;

        public UpdateBaseQueryBuilder(IOfflineTableExistsRepository tableExistsRepository)
            : base(tableExistsRepository) {
        }

        protected override void InitializeInternal(QueryInfo info) {
            var properties = new List<UpdateMapping>();
            var availableProperties = Info.TypeInfo.UpdateableIncludingBaseChildProperties;
            object EntityGetter(object ent) => ent;
            var hasSelectedProperties = info.SelectedProperties.HasAny();
            foreach (var property in availableProperties) {
                var valueGetter = property.Value?.Getter ?? EntityGetter;
                if (hasSelectedProperties && !info.SelectedProperties.Contains(property.Key)) { continue; }
                properties.Add(new UpdateMapping(valueGetter, property.Key));
            }

            _defaultUpdateProperties = properties;
        }

        /// <inheritdoc />
        protected override StringBuilder GenerateQuery() {
            return new StringBuilder();
        }

        /// <inheritdoc />
        public override void SetParameters(DatabaseCommand command, object entity) {
            var tup = entity as Tuple<Dictionary<Tuple<FieldType, Type>, HashSet<object>>, IEnumerable>;
            if (tup == null) { return; }
            var entities = tup.Item2;
            if (entities == null) { return; }
            var valuesPerType = tup.Item1;
            var valueToParameterMapping = new Dictionary<Tuple<FieldType, Type>, Dictionary<object, string>>(valuesPerType.Count);
            var parameterDictionary = new Dictionary<string, SqlParameter>(valuesPerType.Sum(v => v.Value.Count) + 1);
            var paramIndexCounter = 0;
            foreach (var kvp in valuesPerType) {
                var valueParamMapping = new Dictionary<object, string>(kvp.Value.Count);
                valueToParameterMapping.Add(kvp.Key, valueParamMapping);
                foreach (var value in kvp.Value) {
                    var paramName = "@Param" + paramIndexCounter.ToString();
                    valueParamMapping.Add(value, paramName);
                    var sqlDbType = DbTypeConverter.GetSqlTypeFromFieldType(kvp.Key.Item1, kvp.Key.Item2);
                    var parameter = new SqlParameter(paramName, sqlDbType) {
                        Value = value,
                    };
                    parameterDictionary.Add(paramName, parameter);
                    paramIndexCounter += 1;
                }
            }
            var batchQuery = new StringBuilder();
            var i = 0;
            var updateQuery = new StringBuilder();
            var fullEntityTableName = GetTableName(Info.TypeInfo.TableDefinition.FullTableName);
            var primaryKeyInfo = Info.TypeInfo.PrimaryKeyInfo;
            var primaryKeyColumnName = GetPrimaryKey(primaryKeyInfo.ColumnName);
            var updateParts = new List<string>();
            foreach (var obj in entities) {
                var primaryKeyParameterName = "@EntityPrimaryKey_" + i.ToString();
                var parameter = new SqlParameter(primaryKeyParameterName, primaryKeyInfo.Getter(obj));
                parameterDictionary.Add(primaryKeyParameterName, parameter);
                List<UpdateMapping> propertiesToUpdate;
                var dirty = obj as IDirty;
                if (dirty == null || !dirty.IsTrackingChanges) {
                    propertiesToUpdate = _defaultUpdateProperties;
                } else {
                    propertiesToUpdate = GetTrackedPropertiesToUpdate(dirty);
                }
                if (propertiesToUpdate.IsEmpty()) {
                    i++;
                    continue;
                }
                updateParts.Clear();
                updateQuery.Clear();
                updateQuery.AppendFormat("UPDATE {0} SET ", fullEntityTableName);
                foreach (var updateMapping in propertiesToUpdate) {
                    var tempValuesPerType = valueToParameterMapping.TryGetValueWithDefault(updateMapping.Key);
                    var propertyEntity = updateMapping.EntityGetter(dirty);
                    var property = updateMapping.PropertyInfo;
                    var value = GetParameterValue(property, propertyEntity);
                    var paramName = tempValuesPerType.TryGetValueWithDefault(value);
                    updateParts.Add("[{0}] = {1}".FormatWith(property.ColumnName, paramName));
                }
                updateQuery.AppendFormat("{0}", string.Join(", ", updateParts));
                updateQuery.AppendFormat(" WHERE [{0}] = {1} ", primaryKeyColumnName, primaryKeyParameterName);
                batchQuery.Append(";").Append(updateQuery).AppendLine();
                i++;
            }

            command.AddParameters(parameterDictionary, true);
            command.CommandText = batchQuery.ToString();
        }

        public override Tuple<int, Dictionary<Tuple<FieldType, Type>, HashSet<object>>> CalculateAmountPerBatch<TEntity>(IEnumerable<TEntity> entities) {
            var valuesPerType = new Dictionary<Tuple<FieldType, Type>, HashSet<object>>();
            var numberOfEntitiesProcessed = 0;
            var maxEntities = 125;
            foreach (var currentEntity in entities) {
                if (numberOfEntitiesProcessed == maxEntities || (valuesPerType.Sum(v => v.Value.Count) + numberOfEntitiesProcessed + valuesPerType.Count) > ParameterLimit) {
                    return Tuple.Create(numberOfEntitiesProcessed, valuesPerType);
                }
                List<UpdateMapping> propertiesToUpdate;
                var dirty = currentEntity as IDirty;
                if (dirty == null || !dirty.IsTrackingChanges) {
                    propertiesToUpdate = _defaultUpdateProperties;
                } else {
                    propertiesToUpdate = GetTrackedPropertiesToUpdate(dirty);
                }
                for (var i = 0; i < propertiesToUpdate.Count; i++) {
                    var updateMapping = propertiesToUpdate[i];
                    HashSet<object> values;
                    var key = updateMapping.Key;
                    if (!valuesPerType.TryGetValue(key, out values)) {
                        values = new HashSet<object>();
                        valuesPerType[key] = values;
                    }
                    var propertyEntity = updateMapping.EntityGetter(currentEntity);
                    var parameterValue = GetParameterValue(updateMapping.PropertyInfo, propertyEntity);
                    values.Add(parameterValue);
                }
                numberOfEntitiesProcessed += 1;
            }

            return Tuple.Create(numberOfEntitiesProcessed, valuesPerType);
        }

        private List<UpdateMapping> GetTrackedPropertiesToUpdate(IDirty dirty) {
            var selectedProperties = new List<UpdateMapping>();
            var availableProperties = Info.TypeInfo.UpdateableIncludingBaseChildProperties;
            var propertiesGroupedPerParent = availableProperties.ToLookup(p => p.Value);
            foreach (var lookupGrouping in propertiesGroupedPerParent) {
                var dirtyObj = lookupGrouping.Key == null ? dirty : lookupGrouping.Key.Getter(dirty) as IDirty;
                if (dirtyObj == null) { continue; }
                var dirtyProperties = dirtyObj.GetDirtyPropertyNames();
                foreach (var property in lookupGrouping) {
                    if (!dirtyProperties.Contains(property.Key.PropertyName)) { continue; }
                    var valueGetter = property.Value?.Getter ?? EntityGetter;
                    selectedProperties.Add(new UpdateMapping(valueGetter, property.Key));
                }
            }
            return selectedProperties;
            object EntityGetter(object ent) => ent;
        }

        private static object GetParameterValue(EntityPropertyInfo property, object entity) {
            var t = entity.GetType();
            object value;
            if (t.IsSubclassOf(typeof(BaseEntity)) || t.IsSubclassOf(typeof(BaseChild))) {
                value = property.Getter(entity);
            } else {
                value = entity;
            }
            if (value == null) {
                value = DBNull.Value;
            }
            return value;
        }

        internal class UpdateMapping {
            internal Func<object, object> EntityGetter { get; }
            internal EntityPropertyInfo PropertyInfo { get; }
            internal string PartialParameterName { get; }
            internal string ColumnName { get; }
            internal Tuple<FieldType, Type> Key { get; }

            internal UpdateMapping(Func<object, object> entityGetter, EntityPropertyInfo propertyInfo) {
                EntityGetter = entityGetter;
                PropertyInfo = propertyInfo;
                PartialParameterName = "@" + PropertyInfo.EscapedColumnName + "_";
                ColumnName = "[" + PropertyInfo.ColumnName + "]";
                Key = Tuple.Create(PropertyInfo.DatabaseType, PropertyInfo.PropertyType);
            }
        }
    }
}