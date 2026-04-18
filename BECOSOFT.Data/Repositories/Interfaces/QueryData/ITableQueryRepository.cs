using BECOSOFT.Data.Attributes;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryData;
using BECOSOFT.Utilities.Cache;
using BECOSOFT.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Repositories.Interfaces.QueryData {
    internal interface ITableQueryRepository : IBaseRepository {
        /// <summary>
        /// Check if the specified table (defined by the <see cref="TableAttribute"/> on <see cref="T"/>) exists.
        /// Optionally a <see cref="database"/> can be specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablePart"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        bool TableExists<T>(string tablePart = null, string database = null) where T : IEntity;

        /// <summary>
        /// Check if the specified table (defined by the <see cref="TableAttribute"/> on <see cref="type"/> exists.
        /// Optionally a <see cref="database"/> can be specified.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tablePart"></param>
        /// <param name="database"></param>
        bool TableExists(Type type, string tablePart = null, string database = null);


        /// <summary>
        /// Check if the specified table exists.
        /// Optionally a <see cref="database"/> can be specified.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tablePart"></param>
        /// <param name="database"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        bool TableExists(Schema schema, string tableName, string tablePart = null, string database = null);

        /// <summary>
        /// Check if the specified view (defined by the <see cref="TableAttribute"/> on <see cref="T"/>) exists.
        /// Optionally a <see cref="database"/> can be specified.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tablePart"></param>
        /// <param name="database"></param>
        /// <returns></returns>
        bool ViewExists<T>(string tablePart = null, string database = null) where T : IEntity;

        /// <summary>
        /// Check if the specified view (defined by the <see cref="TableAttribute"/> on <see cref="type"/> exists.
        /// Optionally a <see cref="database"/> can be specified.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tablePart"></param>
        /// <param name="database"></param>
        bool ViewExists(Type type, string tablePart = null, string database = null);


        /// <summary>
        /// Check if the specified view exists.
        /// Optionally a <see cref="database"/> can be specified.
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="tablePart"></param>
        /// <param name="database"></param>
        /// <param name="schema"></param>
        /// <returns></returns>
        bool ViewExists(Schema schema, string tableName, string tablePart = null, string database = null);

        void RegisterTableExists(ReplicatedTableEntry tableEntry);

        bool ColumnExists<T, TProp>(Expression<Func<T, TProp>> columnSelector, string tablePart = null, string database = null) where T : IEntity;
        bool ColumnExists<T>(string column, string tablePart = null, string database = null) where T : IEntity;
        bool ColumnExists(Schema schema, string tableName, string column, string tablePart = null, string database = null);
        bool HasRows<T>(string tablePart = null, string database = null) where T : IEntity;
        bool HasRows(Schema schema, string tableName, string tablePart = null, string database = null);
        Dictionary<TypeTablePartDefinition, bool> HasRows(KeyValueList<Type, string> typeTablePartValues, string database = null);
        bool HasIdentity<T>(string tablePart = null, string database = null) where T : IEntity;
        bool HasIdentity(Schema schema, string tableName, string tablePart = null, string database = null);
        IMemoryCacheWrapper GetCache();
    }
}