using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;
using System.Data.Common;
using System.Data;

namespace BECOSOFT.Data.Context {
    public interface IDbContext : IDisposable {

        int ExecuteNonQuery(DatabaseCommand databaseCommand);
        DbDataReader ExecuteReader(DatabaseCommand databaseCommand);
        DbDataReader ExecuteReader(DatabaseCommand databaseCommand, CommandBehavior behavior);
        object ExecuteScalar(DatabaseCommand databaseCommand);
    }
    /// <summary>
    /// Context for the database
    /// </summary>
    public interface IBaseEntityDbContext : IDbContext {
        /// <summary>
        /// Executes a query using the provided <see cref="command"/> and transforms the result to a <see cref="IPagedList{T}"/>.
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseEntity"/></typeparam>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <returns></returns>
        IPagedList<T> Query<T>(DatabaseCommand command) where T : BaseEntity;

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> and transforms the result to a <see cref="IPagedList{T}"/>.
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="IConvertible"/></typeparam>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <returns></returns>
        IPagedList<T> QueryConvertible<T>(DatabaseCommand command) where T : IConvertible;

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> and returns a single <see cref="T"/> (<see cref="BaseEntity"/>).
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseEntity"/></typeparam>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <returns></returns>
        T Get<T>(DatabaseCommand command) where T : BaseEntity;

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> and transforms the result to a <see cref="IPagedList{TResult}"/>. 
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseResult"/></typeparam>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        IPagedList<TResult> GetProperties<T, TResult>(DatabaseCommand command) where T : BaseEntity where TResult : class;

        /// <summary>
        /// Executes a query to retrieve properties (from the <see cref="resultType"/>) from the <see cref="T"/> <see cref="BaseEntity"/>-type.
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseEntity"/></typeparam>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <param name="resultType">The result type</param>
        /// <returns></returns>
        IPagedList<object> GetPropertiesNonGeneric<T>(DatabaseCommand command, Type resultType) where T : BaseEntity;
        
        /// <summary>
        /// Executes queries to insert a list of entities using the provided <see cref="commandBuilder"/>.
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseEntity"/></typeparam>
        /// <param name="commandBuilder">The command builder</param>
        void Insert<T>(DatabaseCommandBuilder<T> commandBuilder) where T : BaseEntity;

        /// <summary>
        /// Executes queries to updates a list of entities using the provided <see cref="commandBuilder"/>.
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseEntity"/></typeparam>
        /// <param name="commandBuilder">The command builder</param>
        void Update<T>(DatabaseCommandBuilder<T> commandBuilder) where T : BaseEntity;

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> to update a property on a <see cref="BaseEntity"/>.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        void UpdateProperty(DatabaseCommand command);

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> to delete a <see cref="BaseEntity"/>.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <returns></returns>
        bool Delete(DatabaseCommand command);

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> to check whether an <see cref="BaseEntity"/> exists.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <returns></returns>
        bool Exists(DatabaseCommand command);

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> that generates no result.
        /// </summary>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute</param>
        void Execute(DatabaseCommand command);
    }
}