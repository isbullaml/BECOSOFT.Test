using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Utilities.Collections;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BECOSOFT.Data.Models {
    /// <summary>
    /// Class representing a temporary table in the database
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TempTable<T> {
        /// <summary>
        /// The name of the table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The rows in the table
        /// </summary>
        public List<T> Values { get; set; } = new List<T>();
        /// <summary>
        /// The type of the rows in the table
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Indicates that the <see cref="Type"/> is implementing <see cref="IBulkCopyable"/>.
        /// </summary>
        public bool IsBulkCopyable { get; }

        private readonly List<PropertyInfo> _bulkCopyableProperties;

        public TempTable(Type type) {
            Type = type;
            IsBulkCopyable = typeof(IBulkCopyable).IsAssignableFrom(type);
            if (!IsBulkCopyable) { return; }
            var properties = Type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var indexedProperties = new KeyValueList<int, PropertyInfo>();
            foreach (var propertyInfo in properties) {
                var bulkCopyableAttribute = propertyInfo.GetCustomAttribute<BulkCopyableColumnAttribute>();
                if (bulkCopyableAttribute == null) { continue; }
                indexedProperties.Add(bulkCopyableAttribute.Index, propertyInfo);
            }
            if (indexedProperties.GroupBy(i => i.Key).Any(g => g.Count() > 1)) {
                throw new BulkCopyableException($"Duplicate {nameof(BulkCopyableColumnAttribute)} indices present in {type.Name}");
            }
            indexedProperties = indexedProperties.OrderBy(ip => ip.Key).ToList();
            _bulkCopyableProperties = new List<PropertyInfo>();
            foreach (var indexedProperty in indexedProperties) {
                _bulkCopyableProperties.Add(indexedProperty.Value);
            }
        }

        /// <summary>
        /// Returns the string required to create the temporary table in SQL.
        /// </summary>
        /// <returns></returns>
        public string GetCreationScript() {
            if (IsBulkCopyable) {
                if (_bulkCopyableProperties.IsEmpty()) {
                    throw new BulkCopyableException();
                }
                var query = new StringBuilder();
                query.AppendLine(" CREATE TABLE {0} ( ", TableName);
                for (var index = 0; index < _bulkCopyableProperties.Count; index++) {
                    var property = _bulkCopyableProperties[index];
                    var (sqlType, collation) = GetSqlTypeString(property.PropertyType);
                    query.AppendLine("{0} {1}{2}", property.Name, sqlType, collation);
                    if (index < _bulkCopyableProperties.Count - 1) {
                        query.Append(", ");
                    }
                }
                query.AppendLine(" ) ");
                return query.ToString();
            } else {
                var (sqlType, collation) = GetSqlTypeString(Type);
                return $"CREATE TABLE {TableName} (tempValue {sqlType}{collation}); ";
            }
        }

        /// <summary>
        /// Returns the string required to fill the temporary table in SQL.
        /// </summary>
        /// <returns></returns>
        public StringBuilder GetFillScript() {
            var sb = new StringBuilder();
            if (IsBulkCopyable) {
                var indexedProperties = new List<(PropertyInfo Property, bool IsBool, bool IsString)>();
                foreach (var prop in _bulkCopyableProperties) {
                    indexedProperties.Add((prop, prop.PropertyType == typeof(bool), prop.PropertyType == typeof(string)));
                }
                var propertyString = string.Join(", ", indexedProperties.Select(prop => $"[{prop.Property.Name}]"));
                foreach (var values in Values.Partition(1000)) {
                    sb.AppendLine(" INSERT INTO {0}({1}) ", TableName, propertyString);
                    sb.AppendLine(" VALUES ");
                    var valueList = values.ToSafeList();
                    for (var i = 0; i < valueList.Count; i++) {
                        var value = valueList[i];
                        sb.Append(" ( ");
                        for (var j = 0; j < indexedProperties.Count; j++) {
                            var (prop, isBool, isString) = indexedProperties[j];
                            sb.AppendLine(ToInsertString(prop.GetValue(value), isBool, isString, false, true));
                            if (j < indexedProperties.Count - 1) {
                                sb.Append(" , ");
                            }
                        }
                        sb.AppendLine(" ) ");
                        if (i < valueList.Count - 1) {
                            sb.Append(" , ");
                        }
                    }
                    sb.AppendLine(";");
                }
            } else {
                var isBool = Type == typeof(bool);
                var isString = Type == typeof(string);
                foreach (var values in Values.Partition(1000)) {
                    sb.AppendLine(" INSERT INTO {0}(tempValue) VALUES ", TableName);
                    sb.AppendLine(string.Join(",", values.Select(sv => ToInsertString(sv, isBool, isString, true, false))));
                    sb.AppendLine(";");
                }
            }
            return sb;
        }

        private static string ToInsertString(object value, bool isBool, bool isString, bool enclose, bool canBeNull) {
            string partial;
            if (canBeNull && value == null) {
                partial = "NULL";
            }else if (isBool) {
                partial = Convert.ToBoolean(value) ? "1" : "0";
            } else if (isString) {
                partial = "'" + value.ToString().Replace("'", "''") + "'";
            } else {
                partial = ((IConvertible) value).ToString(CultureInfo.InvariantCulture);
            }
            if (!enclose) {
                return partial;
            }
            return "(" + partial + ")";
        }

        private (string SqlType, string Collation) GetSqlTypeString(Type type) {
            var size = 0;
            var precision = 0;
            var scale = 0;
            if (type == typeof(string)) {
                size = -1;
            } else if (type == typeof(byte[])) {
                size = 4000;
            }
            if (type.IsDecimal()) {
                precision = 18;
                scale = 4;
            }
            var dbType = DbTypeConverter.GetSqlTypeFromType(type);
            var sqlType = DbTypeConverter.GetStringifiedSqlType(dbType, size, precision, scale);
            var collation = DbTypeConverter.GetCollation(dbType);
            return (sqlType, collation);
        }

        internal IReadOnlyList<PropertyInfo> GetIndexedBulkCopyableProperties() => _bulkCopyableProperties;
    }

    public static class TempTableHelper {
        /// <summary>
        /// Generates a random temp table name using #TempTable, <paramref name="tempTablesGenerated"/> and a random next 1000 value using a <see cref="Guid"/> seeded <see cref="Random"/>.
        /// </summary>
        /// <param name="tempTablesGenerated">ref <see cref="int"/> indicating the number of generated temp tables. Is auto-incremented.</param>
        /// <returns></returns>
        public static string GetTempTableName(ref int tempTablesGenerated) {
            var random = new Random(BitConverter.ToInt32(Guid.NewGuid().ToByteArray(), 0)); 
            tempTablesGenerated++;
            var tableName = $"#TempTable{tempTablesGenerated}{DateTime.UtcNow:yyyyMMddHHmmssffffff}{random.Next(1000)}";
            return tableName;
        }
    }
}
