using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Exceptions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Models.QueryObjects;
using BECOSOFT.Data.Query;
using BECOSOFT.Data.Repositories.Interfaces;
using BECOSOFT.Data.Services.Interfaces;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Repositories {
    public abstract class BaseResultRepository<T> : QueryRepository<T>, IBaseResultRepository<T> where T : BaseResult {
        protected readonly IDatabaseCommandFactory DatabaseCommandFactory;

        protected BaseResultRepository(IDbContextFactory dbContextFactory, IDatabaseCommandFactory databaseCommandFactory) 
            : base(dbContextFactory) {
            DatabaseCommandFactory = databaseCommandFactory;
        }

        /// <inheritdoc />
        public IPagedList<T> Query(int? timeout = null) {
            return Query(new QueryExpressionQueryObject {
                Timeout = timeout,
            });
        }

        public virtual IPagedList<T> Query(QueryExpressionQueryObject queryObject) {
            Check.IsValidTableConsuming<T>(queryObject.TablePart);
            var command = DatabaseCommandFactory.Select<T>(queryObject);

            return Execute(Func, () => command.ToHashString());

            IPagedList<T> Func() {
                using (var context = GetContext()) {
                    return context.Query<T>(command);
                }
            }
        }

        public IPagedList<T> Query(ParametrizedQuery query) {
            Check.IsValidTableConsuming<T>(query.TablePart);
            var command = DatabaseCommandFactory.Custom<T>(query);

            return Execute(Func, () => command.ToHashString());

            IPagedList<T> Func() {
                using (var context = GetContext()) {
                    return context.Query<T>(command);
                }
            }
        }

        public IPagedList<TProperty> Query<TProperty>(ParametrizedQuery query) where TProperty : IConvertible {
            Check.IsValidTableConsuming<T>(query.TablePart);
            var command = DatabaseCommandFactory.Custom(query);

            return Execute(Func, () => command.ToHashString());

            IPagedList<TProperty> Func() {
                using (var context = GetContext()) {
                    return context.QueryConvertible<TProperty>(command);
                }
            }
        }

        /// <inheritdoc/>
        public IPagedList<T> GetByProperty(Expression<Func<T, bool>> whereExpression, PagerData<T> pagerData = null, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var queryExpression = new QueryExpression<T>(whereExpression);
            queryExpression.SetFromPagerData(pagerData);
            var queryObject = new QueryExpressionQueryObject(queryExpression, tablePart);
            return Query(queryObject);
        }

        /// <inheritdoc/>
        public IPagedList<TResult> GetProperties<TResult>(TResult item, QueryExpression<T> queryObject = null, string tablePart = null) where TResult : class {
            Check.IsValidTableConsuming<T>(tablePart);
            var command = DatabaseCommandFactory.Select(EntityConverter.GetEntityTypeInfo(typeof(T)), item.GetType(), queryObject ?? new QueryExpression<T>(), tablePart);
            using (var context = GetContext()) {
                return context.GetProperties<T, TResult>(command);
            }
        }

        /// <inheritdoc/>
        public IPagedList<TResult> GetProperties<TResult>(TResult item, Expression<Func<T, bool>> whereExpression, string tablePart = null) where TResult : class {
            Check.IsValidTableConsuming<T>(tablePart);
            var queryExpression = new QueryExpression<T>(whereExpression);
            var command = DatabaseCommandFactory.Select(EntityConverter.GetEntityTypeInfo(typeof(T)), item.GetType(), queryExpression, tablePart);
            using (var context = GetContext()) {
                return context.GetProperties<T, TResult>(command);
            }
        }

        public IBaseResultDbContext GetContext() {
            return DbContextFactory.CreateBaseResultContext();
        }

        protected string GetPropertyAlias(Expression<Func<T, object>> selector, int level, Type highestParentType = null, string tablePart = null) => 
            BaseQueryBuilder.GetPropertyAlias(selector, level, highestParentType ?? RepositoryType, tablePart);
    }
}