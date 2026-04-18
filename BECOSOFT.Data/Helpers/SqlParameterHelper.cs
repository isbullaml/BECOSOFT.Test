using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BECOSOFT.Data.Helpers {
    /// <summary>
    /// Helper class for the <see cref="SqlParameter"/>-class
    /// </summary>
    internal static class SqlParameterHelper {
        /// <summary>
        /// Adds a parameter to the collection
        /// </summary>
        /// <param name="parameters">The <see cref="Dictionary{TKey,TValue}"/> containing parameters</param>
        /// <param name="property">The property to add</param>
        /// <param name="entity">The entity containing the value</param>
        /// <param name="tablePart"></param>
        internal static void AddParameter(Dictionary<string, SqlParameter> parameters, EntityPropertyInfo property, object entity, string tablePart) {
            var parameterName = "@" + (property.HasFormatSpecifier ? property.ColumnName.FormatWith(tablePart) : property.ColumnName);

            SqlParameter parameter;
            if (!parameters.TryGetValue(parameterName, out parameter)) {
                var sqlDbType = DbTypeConverter.GetSqlTypeFromFieldType(property.DatabaseType, property.PropertyType);
                parameter = new SqlParameter(parameterName, sqlDbType);
                parameters.Add(parameterName, parameter);
            }

            SetParameterValue(property, entity, parameter);
        }

        internal static void AddParameter(Dictionary<string, SqlParameter> parameters, string parameterName, SqlDbType dbType, object value) {
            SqlParameter parameter;
            if (!parameters.TryGetValue(parameterName, out parameter)) {
                parameter = new SqlParameter(parameterName, dbType);
                parameters.Add(parameterName, parameter);
            }
            parameter.SqlDbType = dbType;
            parameter.Value = value ?? DBNull.Value;
        }

        internal static void AddParameter(Dictionary<string, SqlParameter> parameters, SqlParameter parameter) {
            var parameterName = parameter.ParameterName;
            parameters[parameterName] = parameter;
        }

        private static void SetParameterValue(EntityPropertyInfo property, object entity, IDataParameter parameter) {
            var t = entity.GetType();
            if (t.IsSubclassOf(typeof(BaseEntity)) || t.IsSubclassOf(typeof(BaseChild))) {
                parameter.Value = property.Getter(entity);
            } else {
                parameter.Value = entity;
            }
            if (parameter.Value == null) {
                parameter.Value = DBNull.Value;
            }
        }
    }
}