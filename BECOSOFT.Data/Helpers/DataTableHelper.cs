using BECOSOFT.Data.Converters;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace BECOSOFT.Data.Helpers {
    public static class DataTableHelper {
        public static List<T> ConvertToList<T>(DataTable dataTable) {
            var result = new List<T>();

            var properties = GetProperties<T>();
            var columnNames = dataTable.Columns.Cast<DataColumn>().Select(c => c.ColumnName.ToLowerInvariant()).ToSafeHashSet();
            var dataRows = dataTable.AsEnumerable().ToList();

            foreach (var dataRow in dataRows) {
                var entity = TypeActivator<T>.Instance();

                foreach (var property in properties) {
                    var propertyInfo = property.Key;
                    var columnName = propertyInfo.Name.ToLowerInvariant();
                    if (!columnNames.Contains(columnName)) {
                        continue;
                    }

                    var propertyValue = dataRow[columnName];
                    var setter = property.Value;
                    setter(entity, propertyValue);
                };

                result.Add(entity);
            }

            return result;
        }

        private static Dictionary<PropertyInfo, Action<object, object>> GetProperties<T>() {
            var result = new Dictionary<PropertyInfo, Action<object, object>>();

            var properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var property in properties) {
                var fastProperty = new FastProperty(property);
                result.Add(property, fastProperty.SetDelegate);
            }

            return result;
        }
    }
}