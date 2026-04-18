using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Context {
    /// <summary>
    /// Class for retrieving info about an entity it's table or column
    /// </summary>
    public static class Entity {
        /// <summary>
        /// Returns the schema of the provided <see cref="T"/>-parameter. The schema is escaped for Enter-characters and Sql.
        /// </summary>
        /// <returns></returns>
        public static Schema GetSchema<T>() where T : IEntity {
            return GetTableDefinition<T>().Schema;
        }

        /// <summary>
        /// Returns the table name of the provided <see cref="T"/>-parameter. The table name is escaped for Enter-characters and Sql.
        /// </summary>
        /// <returns></returns>
        public static string GetTable<T>() where T : IEntity {
            return GetTableDefinition<T>().TableName;
        }

        /// <summary>
        /// Returns the column name of the provided column <see cref="prop"/> of the <see cref="T"/>-parameter.
        /// </summary>
        /// <typeparam name="T">The entity</typeparam>
        /// <param name="prop">The expression to get the property</param>
        /// <returns>The column name in the database</returns>
        public static string GetColumn<T>(Expression<Func<T, object>> prop) => prop.GetProperty().ColumnName;

        /// <summary>
        /// Returns the primary key column name of the provided <see cref="T"/>-parameter.
        /// </summary>
        /// <typeparam name="T">The entity</typeparam>
        /// <returns>The column name in the database</returns>
        public static string GetPrimaryKeyColumn<T>() where T : BaseEntity => GetColumn<T>(t => t.Id);

        /// <summary>
        /// Returns the table name of the provided <see cref="T"/>-parameter. The table name is escaped for Enter-characters and Sql.
        /// </summary>
        /// <returns></returns>
        public static string GetFullTable<T>() where T : IEntity {
            return GetTableDefinition<T>().FullTableName;
        }

        /// <summary>
        /// Returns the table name of the provided <see cref="T"/>-parameter. The table name is escaped for Enter-characters and Sql.
        /// </summary>
        /// <param name="tablePart">If defined, the <see cref="tablePart"/> is inserted into placeholder in the table name</param>
        /// <returns></returns>
        public static string GetFullTable<T>(string tablePart) where T : IEntity {
            var tableDefinition = GetTableDefinition<T>();
            return tableDefinition.GetFullTable(tablePart);
        }

        public static TableDefinition GetTableDefinition<T>() where T : IEntity {
            return EntityConverter.GetEntityTypeInfo(typeof(T)).TableDefinition;
        }
    }
}