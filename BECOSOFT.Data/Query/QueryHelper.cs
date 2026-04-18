using BECOSOFT.Data.Context;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Query {
    /// <summary>
    /// Helper class for creating custom queries
    /// </summary>
    internal static class QueryHelper {
        /// <summary>
        /// Create a REPLACE LIKE-statement:
        /// REPLACE(<see cref="tableAlias"/>.<see cref="columnName"/>, ' ', '') LIKE <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The name of the column</param>
        /// <param name="paramName">The parameter name</param>
        /// <returns>The statement</returns>
        internal static string CreateReplaceLike(string tableAlias, string columnName, string paramName) =>
            $"REPLACE({tableAlias}.{columnName}, ' ', '') LIKE {paramName}";

        /// <summary>
        /// Create a REPLACE-statement:
        /// <see cref="tableAlias"/>.<see cref="columnName"/> LIKE <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The name of the column</param>
        /// <param name="paramName">The parameter name</param>
        /// <returns>The statement</returns>
        internal static string CreateLike(string tableAlias, string columnName, string paramName) =>
            $"{tableAlias}.{columnName} LIKE {paramName}";

        /// <summary>
        /// Create a LEFT JOIN-statement:
        /// LEFT JOIN <see cref="table"/> <see cref="alias"/> ON <see cref="firstKey"/> = <see cref="secondKey"/>
        /// </summary>
        /// <param name="table">The tablename</param>
        /// <param name="alias">The alias of the table</param>
        /// <param name="firstKey">The first key</param>
        /// <param name="secondKey">The second key</param>
        /// <returns>The statement</returns>
        internal static string CreateLeftJoin(string table, string alias, string firstKey, string secondKey) =>
            $"LEFT JOIN {table} {alias} ON {firstKey} = {secondKey}";

        /// <summary>
        /// Create a LEFT JOIN-statement:
        /// LEFT JOIN <see cref="table"/> <see cref="alias"/> ON {<see cref="joiningFields"/>.Item1 = <see cref="joiningFields"/>.Item2}
        /// </summary>
        /// <param name="table">The tablename</param>
        /// <param name="alias">The alias of the table</param>
        /// <param name="joiningFields">Array of joining fields</param>
        /// <returns>The statement</returns>
        internal static string CreateLeftJoin(string table, string alias, params Tuple<string, string>[] joiningFields) =>
            $"LEFT JOIN {table} {alias} ON {string.Join(" AND ", joiningFields.Select(jf => $"{jf.Item1} = {jf.Item2}"))}";

        /// <summary>
        /// Create a INNER JOIN-statement:
        /// INNER JOIN <see cref="table"/> <see cref="alias"/> ON <see cref="firstKey"/> = <see cref="secondKey"/>
        /// </summary>
        /// <param name="table">The tablename</param>
        /// <param name="alias">The alias of the table</param>
        /// <param name="firstKey">The first key</param>
        /// <param name="secondKey">The second key</param>
        /// <returns>The statement</returns>
        internal static string CreateInnerJoin(string table, string alias, string firstKey, string secondKey) =>
            $"INNER JOIN {table} {alias} ON {firstKey} = {secondKey}";

        /// <summary>
        /// Create a INNER JOIN-statement:
        /// INNER JOIN <see cref="table"/> <see cref="alias"/> ON {<see cref="joiningFields"/>.Item1 = <see cref="joiningFields"/>.Item2}
        /// </summary>
        /// <param name="table">The tablename</param>
        /// <param name="alias">The alias of the table</param>
        /// <param name="joiningFields">Array of joining fields</param>
        /// <returns>The statement</returns>
        internal static string CreateInnerJoin(string table, string alias, params Tuple<string, string>[] joiningFields) =>
            $"INNER JOIN {table} {alias} ON {string.Join(" AND ", joiningFields.Select(jf => $"{jf.Item1} = {jf.Item2}"))}";

        /// <summary>
        /// Create a SELECT-statement:
        /// SELECT <see cref="tableAlias"/>.<see cref="columnName"/> AS <see cref="columnAlias"/>
        /// or if columnAlias is empty:
        /// SELECT <see cref="tableAlias"/>.<see cref="columnName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The column to select</param>
        /// <param name="columnAlias">The alias for the column</param>
        /// <returns>The statement</returns>
        internal static string CreateSelect(string tableAlias, string columnName, string columnAlias = null) {
            if (tableAlias.IsNullOrWhiteSpace()) {
                return columnAlias.IsNullOrWhiteSpace() ? $"{columnName}" : $"{columnName} AS {columnAlias}";
            }

            return columnAlias.IsNullOrWhiteSpace()
                ? $"{tableAlias}.{columnName}"
                : $"{tableAlias}.{columnName} AS {columnAlias}";
        }

        /// <summary>
        /// Create a SELECT-statement based on an expression function
        /// </summary>
        /// <typeparam name="T">The entity</typeparam>
        /// <typeparam name="TProp">The type of the property</typeparam>
        /// <param name="prop">The expression to get the property</param>
        /// <param name="columnAlias">The alias for the column</param>
        /// <returns>The statement</returns>
        internal static string CreateSelect<T>(Expression<Func<T, object>> prop, string columnAlias = null) {
            var columnName = Entity.GetColumn(prop);
            var parameter = prop.Parameters.First();
            var tableAlias = parameter.Name;
            return CreateSelect(tableAlias, columnName, columnAlias);
        }

        /// <summary>
        /// Create a WHERE-statement:
        /// <see cref="tableAlias"/>.<see cref="columnName"/> = <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The column to do a WHERE on</param>
        /// <param name="paramName">The parameter</param>
        /// <returns>The statement</returns>
        internal static string CreateWhere(string tableAlias, string columnName, string paramName) => $"{tableAlias}.{columnName} = {paramName}";

        /// <summary>
        /// Create a WHERE LIKE-statement:
        /// <see cref="tableAlias"/>.<see cref="columnName"/> LIKE <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The column to do a WHERE on</param>
        /// <param name="paramName">The parameter</param>
        /// <returns>The statement</returns>
        public static string CreateWhereLike(string tableAlias, string columnName, string paramName) => $"{tableAlias}.{columnName} LIKE {paramName}";

        /// <summary>
        /// Create a WHERE ISNULL-statement:
        /// ISNULL(<see cref="tableAlias"/>.<see cref="columnName"/>, <see cref="defaultValue"/>) = <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The column to do a WHERE on</param>
        /// <param name="paramName">The parameter</param>
        /// <param name="defaultValue">The default value for the ISNULL-check</param>
        /// <returns>The statement</returns>
        internal static string CreateWhereIsNull(string tableAlias, string columnName, string paramName, string defaultValue) => $"ISNULL({tableAlias}.{columnName}, {defaultValue}) = {paramName} ";

        /// <summary>
        /// Create a WHERE NOT-statement:
        /// <see cref="tableAlias"/>.<see cref="columnName"/> = <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The column to do a WHERE on</param>
        /// <param name="paramName">The parameter</param>
        /// <returns>The statement</returns>
        internal static string CreateWhereNot(string tableAlias, string columnName, string paramName) => $"{tableAlias}.{columnName} <> {paramName}";

        /// <summary>
        /// Create a WHERE NOT LIKE-statement:
        /// <see cref="tableAlias"/>.<see cref="columnName"/> LIKE <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The column to do a WHERE on</param>
        /// <param name="paramName">The parameter</param>
        /// <returns>The statement</returns>
        public static string CreateWhereNotLike(string tableAlias, string columnName, string paramName) => $"{tableAlias}.{columnName} NOT LIKE {paramName}";

        /// <summary>
        /// Create a WHERE NOT ISNULL-statement:
        /// ISNULL(<see cref="tableAlias"/>.<see cref="columnName"/>, <see cref="defaultValue"/>) = <see cref="paramName"/>
        /// </summary>
        /// <param name="tableAlias">The alias of the table</param>
        /// <param name="columnName">The column to do a WHERE on</param>
        /// <param name="paramName">The parameter</param>
        /// <param name="defaultValue">The default value for the ISNULL-check</param>
        /// <returns>The statement</returns>
        internal static string CreateWhereNotIsNull(string tableAlias, string columnName, string paramName, string defaultValue) => $"ISNULL({tableAlias}.{columnName}, {defaultValue}) <> {paramName} ";

        /// <summary>
        /// Gets the sort order string based on a <see cref="SortOrder"/>
        /// </summary>
        /// <param name="sortOrder">The sort order</param>
        /// <returns>The sort order string</returns>
        internal static string GetSortOrderString(SortOrder sortOrder) {
            switch (sortOrder) {
                case SortOrder.Ascending:
                    return "ASC";
                case SortOrder.Descending:
                    return "DESC";
                case SortOrder.Unspecified:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

    }
}