using BECOSOFT.Data.Models;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Data;

namespace BECOSOFT.Data.Converters {
    /// <summary>
    /// Class for converting from C#-types to SQL-types
    /// </summary>
    internal static class DbTypeConverter {
        /// <summary>
        /// Gets the according SQL-type based on the C#-type
        /// </summary>
        /// <param name="fieldType">The field-type</param>
        /// <param name="propType">The property-type</param>
        /// <returns>The SQL-type</returns>
        internal static SqlDbType GetSqlTypeFromFieldType(FieldType fieldType, Type propType) {
            switch (fieldType) {
                case FieldType.Bit:
                    return SqlDbType.Bit;
                case FieldType.Byte:
                    return SqlDbType.VarBinary;
                case FieldType.Numeric:
                    var propTypeToUse = propType.GetNonNullableType();
                    if (propTypeToUse == typeof(int)) {
                        return SqlDbType.Int;
                    }
                    if (propTypeToUse == typeof(short)) {
                        return SqlDbType.SmallInt;
                    }
                    if (propTypeToUse == typeof(long)) {
                        return SqlDbType.BigInt;
                    }
                    if (propTypeToUse == typeof(decimal) || propTypeToUse == typeof(double) || propTypeToUse == typeof(float)) {
                        return SqlDbType.Decimal;
                    }
                    if (propTypeToUse == typeof(Guid)) {
                        return SqlDbType.UniqueIdentifier;
                    }
                    return SqlDbType.Int;
                case FieldType.Char:
                    return SqlDbType.NVarChar;
                case FieldType.Date:
                    return SqlDbType.DateTime;
                case FieldType.Time:
                    return SqlDbType.Time;
                case FieldType.Free:
                case FieldType.Null:
                    return 0;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Gets the according SQL-type to a C# property-type
        /// </summary>
        /// <param name="propType">The property-type</param>
        /// <returns>The SQL-type</returns>
        internal static SqlDbType GetSqlTypeFromType(Type propType) {
            var propTypeToUse = propType.GetNonNullableType();
            if (propTypeToUse == typeof(bool)) {
                return SqlDbType.Bit;
            }
            if (propTypeToUse == typeof(byte)) {
                return SqlDbType.TinyInt;
            }
            if (propTypeToUse == typeof(byte[])) {
                return SqlDbType.VarBinary;
            }
            if (propTypeToUse == typeof(int)) {
                return SqlDbType.Int;
            }
            if (propTypeToUse == typeof(short)) {
                return SqlDbType.SmallInt;
            }
            if (propTypeToUse == typeof(long)) {
                return SqlDbType.BigInt;
            }
            if (propTypeToUse == typeof(decimal) || propTypeToUse == typeof(double) || propTypeToUse == typeof(float)) {
                return SqlDbType.Decimal;
            }
            if (propTypeToUse == typeof(string)) {
                return SqlDbType.NVarChar;
            }
            if (propTypeToUse == typeof(char)) {
                return SqlDbType.NChar;
            }
            if (propTypeToUse == typeof(DateTime)) {
                return SqlDbType.DateTime;
            }
            if (propTypeToUse == typeof(TimeSpan)) {
                return SqlDbType.Time;
            }
            return 0;
        }

        /// <summary>
        /// Gets the stringified SQL Database type for the provided Type, optional size and optional precision.
        /// </summary>
        /// <param name="type">Parameter Type</param>
        /// <param name="size">Optional size definition (for example: NVARCHAR(4000)). Size -1 defines MAX</param>
        /// <param name="precision">Optional size definition (for example: the 18 in NUMERIC(18,2))</param>
        /// <param name="scale">Optional scale definition (for example: the 2 in NUMERIC(18,2))</param>
        /// <returns>Stringified SQL Database type</returns>
        internal static string GetStringifiedSqlType(Type type, int size = 0, int precision = 0, int scale = 0) {
            var dbType = GetSqlTypeFromType(type);
            return GetStringifiedSqlType(dbType, size, precision, scale);
        }

        /// <summary>
        /// Gets the stringified SQL Database type for the provided SqlDbType, optional size and optional precision.
        /// </summary>
        /// <param name="sqlDbType">Parameter Type</param>
        /// <param name="size">Optional size definition (for example: NVARCHAR(4000)). Size -1 defines MAX</param>
        /// <param name="precision">Optional size definition (for example: the 18 in NUMERIC(18,2))</param>
        /// <param name="scale">Optional scale definition (for example: the 2 in NUMERIC(18,2))</param>
        /// <returns>Stringified SQL Database type</returns>
        internal static string GetStringifiedSqlType(SqlDbType sqlDbType, int size = 0, int precision = 0, int scale = 0) {
            var sqlDataType = sqlDbType.ToString().ToUpper();
            switch (sqlDbType) {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Binary:
                    return $"{sqlDataType}({size})";
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                    if (size == 0) { size = 1; }
                    return $"{sqlDataType}({(size == -1 ? "MAX" : size.ToString())})";
                // fixed length
                case SqlDbType.Text:
                case SqlDbType.NText:
                case SqlDbType.Bit:
                case SqlDbType.TinyInt:
                case SqlDbType.SmallInt:
                case SqlDbType.Int:
                case SqlDbType.BigInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Money:
                case SqlDbType.Real:
                case SqlDbType.Float:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.Image:
                    return sqlDataType;
                case SqlDbType.Decimal:
                    if (precision == 0) { precision = 1; }
                    return $"{sqlDataType}({precision},{scale})";
                // Unknown
                case SqlDbType.Timestamp:
                default:
                    return $"/* UNKNOWN DATATYPE: {sqlDataType} */ {sqlDataType}";
            }
        }

        internal static string GetCollation(SqlDbType sqlDbType, string collation = "DATABASE_DEFAULT") {
            switch (sqlDbType) {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.NText:
                    return $" COLLATE {collation}";
                default:
                    return "";
            }
        }
    }
}