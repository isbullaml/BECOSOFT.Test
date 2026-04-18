using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using System;

namespace BECOSOFT.Data.Context {
    /// <summary>
    /// Context for the database
    /// </summary>
    public interface IBaseResultDbContext : IDbContext {

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> and transforms the result to a <see cref="IPagedList{T}"/>.
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseResult"/></typeparam>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <returns></returns>
        IPagedList<T> Query<T>(DatabaseCommand command) where T : BaseResult;

        /// <summary>
        /// Executes a query using the provided <see cref="command"/> and transforms the result to a <see cref="IPagedList{T}"/>.
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="IConvertible"/></typeparam>
        /// <param name="command">The <see cref="DatabaseCommand"/> to execute.</param>
        /// <returns></returns>
        IPagedList<T> QueryConvertible<T>(DatabaseCommand command) where T : IConvertible;
        
        /// <summary>
        /// Executes a query using the provided <see cref="command"/> and transforms the result to a <see cref="IPagedList{TResult}"/>. 
        /// </summary>
        /// <typeparam name="T">A type that inherits <see cref="BaseResult"/></typeparam>
        /// <typeparam name="TResult">The result type</typeparam>
        /// <param name="command"></param>
        /// <returns></returns>
        IPagedList<TResult> GetProperties<T, TResult>(DatabaseCommand command) where T : BaseResult where TResult : class;
    }
}