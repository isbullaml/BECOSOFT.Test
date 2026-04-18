using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Query;
using BECOSOFT.Utilities.Cache;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface IBaseResultRepository<T> where T : BaseResult {
        IPagedList<T> Query(int? timeout = null);
        IPagedList<T> Query(QueryExpressionQueryObject queryObject);
        IPagedList<T> Query(ParametrizedQuery query);
        IPagedList<TProperty> Query<TProperty>(ParametrizedQuery query) where TProperty : IConvertible;

        /// <summary>
        /// Returns a <see cref="IPagedList{T}"/> containing entities that matched the given <see cref="whereExpression"/>. Optionally specify an <see cref="PagerData{T}"/>.<see cref="PagerData{T}.OrderBy"/>.
        /// <see cref="tablePart"/> is mandatory for <see cref="TableConsumingEntity{T}"/>-types.
        /// </summary>
        /// <param name="whereExpression">Where filter expression</param>
        /// <param name="pagerData"></param>
        /// <param name="tablePart"></param>
        /// <returns></returns>
        IPagedList<T> GetByProperty(Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null, string tablePart = null);

        /// <summary>
        /// Returns a <see cref="PagedList{T}"/> containing objects defined by the custom, anonymous class. This anonymous class must contain columns (by their property or column name) of the <see cref="T"/>-class of the repository.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="item">Instance of the custom, anonymous class</param>
        /// <param name="queryObject"><see cref="QueryExpression{T}"/> containing the optional filter and other query parameters</param>
        /// <param name="tablePart">Optional table part</param>
        /// <returns></returns>
        IPagedList<TResult> GetProperties<TResult>(TResult item, QueryExpression<T> queryObject = null, string tablePart = null) where TResult : class;

        /// <summary>
        /// Returns a <see cref="PagedList{T}"/> containing objects defined by the custom, anonymous class. This anonymous class must contain columns (by their property or column name) of the <see cref="T"/>-class of the repository.
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="item">Instance of the custom, anonymous class</param>
        /// <param name="whereExpression">Filter to execute on the <see cref="T"/>-class</param>
        /// <param name="tablePart">Optional table part</param>
        /// <returns></returns>
        IPagedList<TResult> GetProperties<TResult>(TResult item, Expression<Func<T, bool>> whereExpression, string tablePart = null) where TResult : class;

        /// <summary>
        /// Retrieve a dbcontext from the factory
        /// </summary>
        /// <returns>The context</returns>
        IBaseResultDbContext GetContext();

        void RefreshCache();
        IMemoryCacheWrapper GetCache();
    }
}
