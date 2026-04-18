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
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace BECOSOFT.Data.Repositories {
    public abstract class ReadonlyRepository<T> : QueryRepository<T>, IReadonlyRepository<T> where T : BaseEntity {
        protected readonly IDatabaseCommandFactory DatabaseCommandFactory;

        protected ReadonlyRepository(IDbContextFactory dbContextFactory, 
                                     IDatabaseCommandFactory databaseCommandFactory)
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
            var command = DatabaseCommandFactory.Custom<TProperty>(query);

            return Execute(Func, () => command.ToHashString());

            IPagedList<TProperty> Func() {
                using (var context = GetContext()) {
                    return context.QueryConvertible<TProperty>(command);
                }
            }
        }

        /// <summary>
        /// Executes a query without returning a result.
        /// </summary>
        /// <param name="query">The query to execute</param>
        public void Execute(ParametrizedQuery query) {
            var command = DatabaseCommandFactory.Custom(query);
            using (var context = GetContext()) {
                context.Execute(command);
            }
        }

        /// <inheritdoc/>
        public virtual T GetById(int id, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var command = DatabaseCommandFactory.Select<T>(id, tablePart);

            return Execute(Func, () => command.ToHashString());

            T Func() {
                using (var context = GetContext()) {
                    return context.Get<T>(command);
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
            var type = typeof(T);
            Check.IsValidTableConsuming<T>(tablePart);
            var command = DatabaseCommandFactory.Select(EntityConverter.GetEntityTypeInfo(type), item.GetType(), queryObject ?? new QueryExpression<T>(), tablePart);
            using (var context = GetContext()) {
                return context.GetProperties<T, TResult>(command);
            }
        }

        /// <inheritdoc/>
        public IPagedList<TResult> GetProperties<TResult>(TResult item, Expression<Func<T, bool>> whereExpression, string tablePart = null) where TResult : class {
            var type = typeof(T);
            Check.IsValidTableConsuming<T>(tablePart);
            var queryExpression = new QueryExpression<T>(whereExpression);
            var command = DatabaseCommandFactory.Select(EntityConverter.GetEntityTypeInfo(type), item.GetType(), queryExpression, tablePart);
            using (var context = GetContext()) {
                return context.GetProperties<T, TResult>(command);
            }
        }

        /// <inheritdoc/>
        public HashSet<int> GetIDs(string tablePart = null) {
            var type = typeof(T);
            Check.IsValidTableConsuming<T>(tablePart);
            var entityInfo = EntityConverter.GetEntityTypeInfo(type);
            var fullTableName = entityInfo.TableDefinition.GetFullTable(tablePart);
            var query = $"SELECT {entityInfo.PrimaryKeyInfo.ColumnName} FROM {fullTableName}";
            var parameterQuery = new ParametrizedQuery(query, tablePart, false);
            var command = DatabaseCommandFactory.Custom<int>(parameterQuery);

            return Execute(Func, () => command.ToHashString());

            HashSet<int> Func() {
                using (var context = GetContext()) {
                    return context.QueryConvertible<int>(command).ToSafeHashSet();
                }
            }
        }

        /// <inheritdoc/>
        public HashSet<int> GetIDs(IEnumerable<int> ids, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            var idList = ids.ToDistinctList();
            if (idList.IsEmpty()) {
                return new HashSet<int>();
            }
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var parametrizedQuery = new ParametrizedQuery();
            var query = new StringBuilder();
            var primaryKeyColumnName = entityInfo.PrimaryKeyInfo.ColumnName;
            query.AppendLine(" SELECT e.{0}", primaryKeyColumnName);
            query.AppendLine(" FROM {0} e", entityInfo.TableDefinition.GetFullTable(tablePart));
            if (idList.Count <= 10) {
                query.AppendLine(" WHERE e.{0} IN (", primaryKeyColumnName);
                for (var i = 0; i < idList.Count; i++) {
                    var param = parametrizedQuery.AddParameter("@Id" + i.ToString(), idList[i]);
                    query.Append(param);
                    if (i != idList.Count - 1) {
                        query.Append(",");
                    }
                }
                query.Append(")");
            } else {
                var tempTableName = "#IdTempTable";
                parametrizedQuery.AddBulkCopyTempTable(idList, tempTableName);
                query.AppendLine(" INNER JOIN {0} t ON t.tempValue = e.{1}", tempTableName, primaryKeyColumnName);
            }
            parametrizedQuery.SetQuery(query);
            var command = DatabaseCommandFactory.Custom(parametrizedQuery);
            using (var context = GetContext()) {
                var result = context.QueryConvertible<int>(command);
                return result.ToSafeHashSet();
            }
        }

        /// <inheritdoc/>
        public virtual bool Exists(int id, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            if (id < 0) { return false; }
            var command = DatabaseCommandFactory.Exists<T>(id, tablePart);

            return Execute(Func, () => command.ToHashString());

            bool Func() {
                using (var context = GetContext()) { return context.Exists(command); }
            }
        }

        /// <inheritdoc/>
        public virtual bool Exists(Expression<Func<T, bool>> where, string tablePart = null) {
            Check.IsValidTableConsuming<T>(tablePart);
            if (where == null) {
                return false;
            }
            var command = DatabaseCommandFactory.Exists(where, tablePart);
            using (var context = GetContext()) {
                return context.Exists(command);
            }
        }

        public IBaseEntityDbContext GetContext() {
            return DbContextFactory.CreateBaseEntityContext();
        }
    }
}