using BECOSOFT.Data.Converters;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Numeric;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Text;

namespace BECOSOFT.Data.Extensions {
    /// <summary>
    /// Extensions for the <see cref="SqlParameter"/>-class
    /// </summary>
    public static class SqlParameterExtensions {
        /// <summary>
        /// Gets the value of the SQL-parameter as string
        /// </summary>
        /// <param name="sp">The SQL-parameter</param>
        /// <returns>The value of the parameter as string</returns>
        public static string GetParameterValueForSql(this SqlParameter sp) {
            if (sp.Value == null) {
                return "NULL";
            }
            var dbNull = sp.Value as DBNull;
            if (dbNull != null) {
                return "NULL";
            }

            return GetReturnValue(sp.Value, sp.SqlDbType);
        }


        /// <summary>
        /// Gets the SQL database type declaration
        /// </summary>
        /// <param name="sp">The SQL-parameter</param>
        /// <returns>The SQL database type declaration</returns>
        public static string GetDbTypeDeclaration(this SqlParameter sp) {
            if (sp.DbType == DbType.Decimal && sp.Scale == 0 && sp.Precision == 0) {
                if (sp.Value != null) {
                    var dec = sp.Value.To<decimal>();
                    sp.Scale = dec.GetDecimalPlaces(false).To<byte>();
                    sp.Precision = 38;
                }
            }
            return DbTypeConverter.GetStringifiedSqlType(sp.SqlDbType, sp.Size, sp.Precision, sp.Scale);
        }

        private static string GetReturnValue(object value, SqlDbType dbType) {
            string returnValue;
            switch (dbType) {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.Time:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                case SqlDbType.DateTimeOffset:
                    returnValue = "'" + value.ToString().Replace("'", "''") + "'";
                    break;
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                    returnValue = "'" + ((DateTime) value).ToString("yyyy-MM-dd HH:mm:ss.fff").Replace("'", "''") + "'";
                    break;

                case SqlDbType.Bit:
                    returnValue = (bool) value ? "1" : "0";
                    break;
                case SqlDbType.VarBinary:
                    var data = (byte[]) value;
                    if (data.Length == 0) {
                        returnValue = "NULL";
                    } else {
                        var dataBuilder = new StringBuilder("0x");
                        foreach (var b in data) {
                            dataBuilder.Append(b.ToString("x2"));
                        }
                        returnValue = dataBuilder.ToString();
                    }
                    break;
                default:
                    var type = value.GetType();
                    var typeInfo = type.GetTypeInformation();
                    if (typeInfo.IsEnum) {
                        var converter = Converter.GetDelegate(typeInfo.UnderlyingType);
                        returnValue = converter(value).ToString().Replace("'", "''");
                    } else if (typeInfo.IsNullableOf) {
                        var converter = Converter.GetDelegate(typeInfo.UnderlyingType);
                        returnValue = GetReturnValue(converter(value), DbTypeConverter.GetSqlTypeFromType(typeInfo.UnderlyingType));
                    } else {
                        var formattable = value as IFormattable;
                        if (formattable == null) {
                            returnValue = value.ToString().Replace("'", "''");
                        } else {
                            returnValue = string.Format(CultureInfo.InvariantCulture, "{0}", formattable).Replace("'", "''");
                        }
                    }
                    break;
            }
            return returnValue;
        }

        public static bool IsDbTypeString(this SqlParameter parameter) {
            return IsDbType<string>(parameter) || IsDbType<char>(parameter);
        }

        public static bool IsDbTypeInteger(this SqlParameter parameter) {
            return IsDbType<byte>(parameter)
                   || IsDbType<short>(parameter)
                   || IsDbType<int>(parameter)
                   || IsDbType<long>(parameter);
        }

        private static bool IsDbType<T>(SqlParameter parameter) {
            var type = typeof(T);
            Type compareType;
            switch (parameter.SqlDbType) {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Binary:
                    compareType = typeof(char);
                    break;
                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.NText:
                    compareType = typeof(string);
                    break;
                case SqlDbType.Image:
                case SqlDbType.Timestamp:
                case SqlDbType.VarBinary:
                    compareType = typeof(byte[]);
                    break;
                case SqlDbType.Bit:
                    compareType = typeof(bool);
                    break;
                case SqlDbType.TinyInt:
                    compareType = typeof(byte);
                    break;
                case SqlDbType.SmallInt:
                    compareType = typeof(short);
                    break;
                case SqlDbType.Int:
                    compareType = typeof(int);
                    break;
                case SqlDbType.BigInt:
                    compareType = typeof(long);
                    break;
                case SqlDbType.Decimal:
                case SqlDbType.SmallMoney:
                case SqlDbType.Money:
                    compareType = typeof(decimal);
                    break;
                case SqlDbType.Real:
                    compareType = typeof(float);
                    break;
                case SqlDbType.Float:
                    compareType = typeof(double);
                    break;
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                    compareType = typeof(DateTime);
                    break;
                case SqlDbType.DateTimeOffset:
                    compareType = typeof(DateTimeOffset);
                    break;
                case SqlDbType.UniqueIdentifier:
                    compareType = typeof(Guid);
                    break;
                // Unknown
                default:
                    compareType = typeof(object);
                    break;
            }
            return compareType == type;
        }

        /// <summary>
        /// Uses the <see cref="ICloneable"/> interface on <see cref="SqlParameter"/> to create a clone of the parameter.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static SqlParameter GetClone(this SqlParameter parameter) {
            return (SqlParameter)((ICloneable)parameter).Clone();
        }
    }
}
