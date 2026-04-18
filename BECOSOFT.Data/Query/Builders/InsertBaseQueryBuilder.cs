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
    internal class InsertBaseQueryBuilder : BaseQueryBuilder {
        private const int ParameterLimit = 2000;
        private List<InsertMapping> _insertProperties;

        public override QueryType Type => QueryType.Insert;

        public InsertBaseQueryBuilder(IOfflineTableExistsRepository tableExistsRepository)
            : base(tableExistsRepository) {
        }

        protected override void InitializeInternal(QueryInfo info) {
            var properties = new List<InsertMapping>();
            var availableProperties = Info.TypeInfo.InsertableIncludingBaseChildProperties;
            object EntityGetter(object ent) => ent;

            foreach (var property in availableProperties) {
                var valueGetter = property.Value?.Getter ?? EntityGetter;
                properties.Add(new InsertMapping(valueGetter, property.Key));
            }

            _insertProperties = properties;
            SetAmountPerBatch(_insertProperties.Count + 1);
        }

        /// <inheritdoc />
        protected override StringBuilder GenerateQuery() {
            var queryBuilder = new StringBuilder();
            if (!_insertProperties.HasAny()) {
                throw new ArgumentException();
            }

            var columns = _insertProperties.Select(ip => ip.ColumnName).ToList();
            queryBuilder.AppendLine("DECLARE @IDs TABLE ([EntityIndex] INT, [ID] INT)");
            queryBuilder.AppendLine("MERGE INTO {0}", GetTableName(Info.TypeInfo.TableDefinition.FullTableName));
            queryBuilder.AppendLine("USING (");
            // Replace '{1}' with '{0}' so we can use string.Format again when entering the value list in this command
            queryBuilder.AppendLine("VALUES {0}", "{0}");
            queryBuilder.AppendLine(") AS entities ([EntityIndex], {0})", string.Join(",", columns));
            queryBuilder.AppendLine("ON 1 = 0");
            queryBuilder.AppendLine("WHEN NOT MATCHED BY TARGET THEN INSERT ({0})", string.Join(",", columns));
            queryBuilder.AppendLine("VALUES ({0})", string.Join(",", columns));
            queryBuilder.AppendLine("OUTPUT entities.[EntityIndex], inserted.{0} AS [ID]", GetPrimaryKey(Info.TypeInfo.PrimaryKeyInfo.ColumnName));
            queryBuilder.AppendLine("INTO @IDs([EntityIndex], [ID]);");
            queryBuilder.AppendLine("SELECT * FROM @IDs");

            return queryBuilder;
        }

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
            var values = new List<string>();

            var currentParameters = new string[_insertProperties.Count + 1];
            var i = 0;
            foreach (var currentEntity in entities) {
                //Add EntityIndex parameter
                var entityIndexParameterName = "@EntityIndex_" + i.ToString();
                var parameter = new SqlParameter(entityIndexParameterName, i);
                currentParameters[0] = entityIndexParameterName;
                parameterDictionary.Add(entityIndexParameterName, parameter);

                //Add column parameters
                for (var p = 0; p < _insertProperties.Count; p++) {
                    var insertMapping = _insertProperties[p];
                    var tempValuesPerType = valueToParameterMapping.TryGetValueWithDefault(insertMapping.Key);
                    var propertyEntity = insertMapping.EntityGetter(currentEntity);
                    var value = GetParameterValue(insertMapping.PropertyInfo, propertyEntity);
                    var paramName = tempValuesPerType.TryGetValueWithDefault(value);
                    currentParameters[p + 1] = paramName;
                }

                values.Add($"({string.Join(",", currentParameters)})");
                i++;
            }
            command.AddParameters(parameterDictionary, true);
            command.CommandText = command.CommandText.FormatWith(string.Join(",", values));
        }

        public override Tuple<int, Dictionary<Tuple<FieldType, Type>, HashSet<object>>> CalculateAmountPerBatch<TEntity>(IEnumerable<TEntity> entities) {
            var valuesPerType = new Dictionary<Tuple<FieldType, Type>, HashSet<object>>();
            var numberOfEntitiesProcessed = 0;
            var maxEntities = 125;
            foreach (var currentEntity in entities) {
                if (numberOfEntitiesProcessed == maxEntities || (valuesPerType.Sum(v => v.Value.Count) + numberOfEntitiesProcessed + valuesPerType.Count) > ParameterLimit) {
                    return Tuple.Create(numberOfEntitiesProcessed, valuesPerType);
                }
                for (var i = 0; i < _insertProperties.Count; i++) {
                    var insertMapping = _insertProperties[i];
                    HashSet<object> values;
                    var key = insertMapping.Key;
                    if (!valuesPerType.TryGetValue(key, out values)) {
                        values = new HashSet<object>();
                        valuesPerType[key] = values;
                    }
                    var propertyEntity = insertMapping.EntityGetter(currentEntity);
                    var parameterValue = GetParameterValue(insertMapping.PropertyInfo, propertyEntity);
                    values.Add(parameterValue);
                }
                numberOfEntitiesProcessed += 1;
            }
            return Tuple.Create(numberOfEntitiesProcessed, valuesPerType);
        }

        private void SetAmountPerBatch(int columnCount) {
            AmountPerBatch = ParameterLimit / columnCount;
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
    }

    internal class InsertMapping {
        internal Func<object, object> EntityGetter { get; }
        internal EntityPropertyInfo PropertyInfo { get; }
        internal string PartialParameterName { get; }
        internal string ColumnName { get; }
        internal Tuple<FieldType, Type> Key { get; }

        internal InsertMapping(Func<object, object> entityGetter, EntityPropertyInfo propertyInfo) {
            EntityGetter = entityGetter;
            PropertyInfo = propertyInfo;
            PartialParameterName = "@" + PropertyInfo.EscapedColumnName + "_";
            ColumnName = "[" + PropertyInfo.ColumnName + "]";
            Key = Tuple.Create(PropertyInfo.DatabaseType, PropertyInfo.PropertyType);
        }
    }
}