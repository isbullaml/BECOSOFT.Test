using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services {
    public class BaseResultService<T> : BaseService<T>, IBaseResultService<T> where T : BaseResult {
        private readonly IBaseResultRepository<T> _repository;

        public BaseResultService(IBaseResultRepository<T> repository) {
            _repository = repository;
        }

        /// <inheritdoc />
        public virtual IPagedList<T> GetAll(PagerData<T> pagerData = null) {
            Check.IsNotTableConsuming<T>();
            var queryObject = new QueryExpressionQueryObject(GetQueryExpression(pagerData: pagerData));
            return _repository.Query(queryObject);
        }

        /// <inheritdoc />
        public IPagedList<T> GetByProperty(Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null) {
            Check.IsNotTableConsuming<T>();
            return _repository.GetByProperty(whereExpression, pagerData);
        }

        /// <inheritdoc />
        public IPagedList<TResult> GetProperties<TResult>(TResult item, Expression<Func<T, bool>> whereExpression = null, PagerData<T> pagerData = null) where TResult : class {
            Check.IsNotTableConsuming<T>();
            var queryExpression = new QueryExpression<T>(whereExpression);
            queryExpression.SetFromPagerData(pagerData);
            return _repository.GetProperties(item, queryExpression);
        }
    }
}
