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
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Services {
    public abstract class ReadonlyTableService<T, TDefining> : IReadonlyTableService<T, TDefining>
        where T : TableConsumingEntity<TDefining>
        where TDefining : TableDefiningEntity {
        private readonly IReadonlyRepository<T> _repository;
        private readonly IReadonlyRepository<TDefining> _definingRepository;

        protected ReadonlyTableService(IReadonlyRepository<T> repository, IReadonlyRepository<TDefining> definingRepository) {
            _repository = repository;
            _definingRepository = definingRepository;
        }

        public virtual T Get(TDefining definition, int id) {
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            return id <= 0 ? null : _repository.GetById(id, tableName);
        }

        public virtual IPagedList<T> Get(TDefining definition, IEnumerable<int> ids) {
            var idList = ids.ToSafeList();
            if (idList.IsEmpty()) {
                return new PagedList<T>();
            }
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            var queryObject = QueryExpressionQueryObject.FromWhere<T>(e => idList.Contains(e.Id), tableName);
            return _repository.Query(queryObject);
        }

        public virtual IPagedList<T> GetAll(TDefining definition, PagerData<T> pagerData = null) {
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            var queryObject = new QueryExpressionQueryObject(GetQueryExpression(pagerData), tableName);
            return _repository.Query(queryObject);
        }

        public virtual IPagedList<TResult> GetProperties<TResult>(TDefining definition, TResult item, Expression<Func<T, bool>> whereExpression = null, PagerData<T> pagerData = null) where TResult : class {
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            var queryExpression = new QueryExpression<T>(whereExpression);
            queryExpression.SetFromPagerData(pagerData);
            return _repository.GetProperties(item, queryExpression, tableName);
        }

        /// <inheritdoc />
        public virtual IPagedList<T> GetByProperty(TDefining definition, Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null) {
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            return _repository.GetByProperty(whereExpression, pagerData, tableName);
        }

        /// <inheritdoc />
        public TProp GetProperty<TProp>(TDefining definition, int id, Expression<Func<T, TProp>> property) where TProp : IConvertible {
            PreValidateDefinition(definition);
            var tablePart = GetTableName(definition);
            var parameterQuery = new ParametrizedQuery();
            parameterQuery.TablePart = tablePart;
            parameterQuery.AddParameter("@id", id);
            var objectExpression = PropertyExpressionConverter.ConvertToObjectPropertySelector(property);
            var query = $"SELECT {Entity.GetColumn(objectExpression)} FROM {Entity.GetFullTable<T>(tablePart)} WHERE {Entity.GetColumn((T t) => t.Id)} = @id";
            parameterQuery.SetQuery(query);
            return id <= 0 ? default(TProp) : _repository.Query<TProp>(parameterQuery).FirstOrDefault();
        }

        public virtual bool Exists(TDefining definition, T entity) {
            PreValidateDefinition(definition);
            var expressionBuilder = new ExistsExpressionBuilder<T>(entity.Id);
            var tableName = GetTableName(definition);
            return _repository.Exists(expressionBuilder.ToExpression(), tableName);
        }

        public virtual bool Exists(TDefining definition, int id) {
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            return _repository.Exists(id, tableName);
        }

        public bool Exists(TDefining definition, Expression<Func<T, bool>> whereExpression) {
            PreValidateDefinition(definition);
            var tableName = GetTableName(definition);
            return _repository.Exists(whereExpression, tableName);
        }

        protected static QueryExpression<T> GetQueryExpression(PagerData<T> pagerData = null) {
            var queryExpression = new QueryExpression<T>();
            queryExpression.SetFromPagerData(pagerData);
            return queryExpression;
        }

        protected void PreValidateDefinition(TDefining definition) {
            var tableName = GetTableName(definition);
            if (tableName.IsNullOrWhiteSpace()) {
                throw new UndefinedTableDefinitionException(Resources.Error_MissingTablePart);
            }
            if (!_definingRepository.Exists(definition.Id)) {
                throw new UndefinedTableDefinitionException(Resources.Error_DefiningType_NonExistent);
            }
        }

        protected virtual string GetTableName(TDefining definition) {
            return definition?.TableName;
        }
    }
}