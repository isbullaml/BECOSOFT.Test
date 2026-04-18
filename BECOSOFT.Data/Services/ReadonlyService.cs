using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Helpers;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services {
    public abstract class ReadonlyService<T> : BaseService<T>, IReadonlyService<T> where T : BaseEntity {
        private readonly IReadonlyRepository<T> _repository;

        protected ReadonlyService(IReadonlyRepository<T> repository) {
            _repository = repository;
        }

        /// <inheritdoc />
        public virtual T Get(int id) {
            Check.IsNotTableConsuming<T>();
            return id <= 0 ? null : _repository.GetById(id);
        }

        /// <inheritdoc />
        public virtual IPagedList<T> Get(IEnumerable<int> ids) {
            Check.IsNotTableConsuming<T>();
            var idList = ids.ToSafeList();
            if (idList.IsEmpty()) {
                return new PagedList<T>();
            }
            var queryObject = QueryExpressionQueryObject.FromWhere<T>(e => idList.Contains(e.Id));
            return _repository.Query(queryObject);
        }

        /// <inheritdoc />
        public virtual IPagedList<T> GetAll(PagerData<T> pagerData = null) {
            Check.IsNotTableConsuming<T>();
            var queryObject = new QueryExpressionQueryObject(GetQueryExpression(pagerData: pagerData));
            return _repository.Query(queryObject);
        }

        /// <inheritdoc />
        public virtual IPagedList<T> GetByProperty(Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null) {
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

        /// <inheritdoc />
        public TProp GetProperty<TProp>(int id, Expression<Func<T, TProp>> property) where TProp : IConvertible {
            Check.IsNotTableConsuming<T>();
            if (id <= 0) {
                return default(TProp);
            }
            var parameterQuery = new ParametrizedQuery();
            parameterQuery.AddParameter("@id", id);
            var objectExpression = PropertyExpressionConverter.ConvertToObjectPropertySelector(property);
            var query = $"SELECT {Entity.GetColumn(objectExpression)} FROM {Entity.GetFullTable<T>()} WHERE {Entity.GetColumn((T t) => t.Id)} = @id";
            parameterQuery.SetQuery(query);
            return _repository.Query<TProp>(parameterQuery).FirstOrDefault();
        }

        public virtual bool Exists(T entity) {
            Check.IsNotTableConsuming<T>();
            var expressionBuilder = new ExistsExpressionBuilder<T>(entity.Id);
            return _repository.Exists(expressionBuilder.ToExpression());
        }

        public bool Exists(int id) {
            Check.IsNotTableConsuming<T>();
            return _repository.Exists(id);
        }

        public bool Exists(Expression<Func<T, bool>> whereExpression) {
            Check.IsNotTableConsuming<T>();
            return _repository.Exists(whereExpression);
        }
    }
}