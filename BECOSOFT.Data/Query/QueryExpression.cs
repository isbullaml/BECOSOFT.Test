using BECOSOFT.Data.Converters;
using BECOSOFT.Data.Extensions;
using BECOSOFT.Data.Models;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Query {
    /// <inheritdoc />
    public class QueryExpression<T> : IQueryExpression {
        private readonly Expression<Func<T, bool>> _where;
        /// <summary>
        /// The amount of rows to skip
        /// </summary>
        public int? Skip { get; set; }
        /// <summary>
        /// The amount of rows to take
        /// </summary>
        public int? Take { get; set; }
        /// <summary>
        /// ORDER BY
        /// </summary>
        internal List<OrderDefinition<T>> OrderBy { get; private set; } = new List<OrderDefinition<T>>();
        /// <summary>
        /// The text to search for
        /// </summary>
        public string SearchText { get; set; }
        /// <summary>
        /// DISTINCT filter
        /// </summary>
        public bool Distinct { get; set; } = false;

        public QueryExpression(Expression<Func<T, bool>> where = null) {
            _where = QueryTranslator.BooleanComplexifier.Process(where);
        }

        public void AddOrderBy(OrderDefinition<T> orderExpression) => OrderBy.Add(orderExpression);

        public void AddOrderBy(int position, OrderDefinition<T> orderExpression) => OrderBy.Insert(position, orderExpression);

        public void RemoveOrderBy(OrderDefinition<T> orderExpression) => OrderBy.Remove(orderExpression);

        public void RemoveOrderBy(int position) => OrderBy.RemoveAt(position);

        /// <summary>
        /// Sets the <see cref="Skip"/>, <see cref="Take"/>, <see cref="Distinct"/>, <see cref="SearchText"/> and <see cref="OrderBy"/> fields from a pager-data object
        /// </summary>
        /// <param name="pagerData"></param>
        public void SetFromPagerData(PagerData<T> pagerData) {
            if (pagerData == null) {
                return;
            }
            if (pagerData.Skip.HasValue) {
                Skip = pagerData.Skip;
            }
            if (pagerData.Take.HasValue) {
                Take = pagerData.Take;
            }
            SearchText = pagerData.SearchText;
            if (pagerData.OrderBy.HasAny()) {
                OrderBy = pagerData.OrderBy;
            }
            Distinct = pagerData.Distinct;
        }

        /// <inheritdoc />
        public Expression ToExpression() {
            // ReSharper disable once CollectionNeverUpdated.Local
            var list = new List<T>(1);
            var queryable = list.AsQueryable();
            var hasSearchText = !SearchText.IsNullOrWhiteSpace() && typeof(T).IsInterfaceImplementationOf<IEntity>();
            if (_where != null && !hasSearchText) {
                queryable = queryable.Where(_where);
            } else if (hasSearchText) {
                var res = WhereFromSearchText();
                if (res != null) {
                    queryable = queryable.Where(res);
                }
            }

            for (var i = 0; i < OrderBy.Count; i++) {
                queryable = i == 0 ? queryable.OrderBy(OrderBy[i]) : ((IOrderedQueryable<T>) queryable).ThenBy(OrderBy[i]);
            }

            if (Skip.HasValue) {
                queryable = queryable.Skip(Skip.Value);
            }
            if (Take.HasValue) {
                queryable = queryable.Take(Take.Value);
            }
            if (Distinct) {
                queryable = queryable.Distinct();
            }

            return queryable.Expression;
        }

        private Expression<Func<T, bool>> WhereFromSearchText() {
            var entityInfo = EntityConverter.GetEntityTypeInfo(typeof(T));
            var argParam = Expression.Parameter(typeof(T), "e");
            var entityInfoProperties = entityInfo.SearchProperties;
            var splitSearchText = SearchText.Split('+').Select(s => s.ToLowerInvariant()).Distinct().ToList();
            Expression andExpression = null;
            foreach (var part in splitSearchText) {
                var orExpression = GetOrExpressionForPart(entityInfoProperties, argParam, part);
                andExpression = CombineToAndAlsoExpression(andExpression, orExpression);
            }
            if (andExpression == null) {
                return null;
            }
            if (_where != null) {
                var renamedExpression = new ExpressionRenamer().Rename(_where.Body, argParam);
                andExpression = CombineToAndAlsoExpression(renamedExpression, andExpression);
            }
            return Expression.Lambda<Func<T, bool>>(andExpression, argParam);
        }

        private static Expression GetOrExpressionForPart(IEnumerable<EntityPropertyInfo> entityInfoProperties,
                                                         ParameterExpression argParam, string part) {
            Expression orExpression = null;
            foreach (var propertyInfo in entityInfoProperties) {
                var propertyType = propertyInfo.PropertyType;
                Expression eqExpression;
                if (propertyInfo.AreLinkedEntities) {
                    eqExpression = GetLinkedEntitiesEqualsExpression(argParam, part, propertyInfo);
                } else if (propertyInfo.IsLinkedEntity) {
                    eqExpression = GetLinkedEntityEqualsExpression(argParam, part, propertyInfo);
                } else if (propertyInfo.IsBaseChild) {
                    eqExpression = GetLinkedBaseChildEqualsExpression(argParam, part, propertyInfo);
                } else {
                    var propertyExpression = Expression.Property(argParam, propertyInfo.PropertyName);
                    eqExpression = CreateEqualityExpression(propertyType, propertyExpression, part);
                }
                if (eqExpression == null) {
                    continue;
                }
                orExpression = CombineToOrExpression(orExpression, eqExpression);
            }
            return orExpression;
        }

        private static Expression GetLinkedBaseChildEqualsExpression(ParameterExpression argParam, string part,
                                                                     EntityPropertyInfo propertyInfo) {
            var property = Expression.Property(argParam, propertyInfo.PropertyName);
            var propertyEntityType = propertyInfo.BaseEntityType;
            if (!propertyEntityType.IsSubclassOf(typeof(BaseChild))) {
                return null;
            }
            var propertyEntityInfo = EntityConverter.GetEntityTypeInfo(propertyEntityType);
            Expression subOrExpression = null;
            foreach (var linkedProperty in propertyEntityInfo.SearchProperties) {
                if (linkedProperty.Equals(propertyEntityInfo.PrimaryKeyInfo)) {
                    continue;
                }
                var subProperty = Expression.Property(property, linkedProperty.PropertyInfo);
                var subEqExpression = CreateEqualityExpression(linkedProperty.PropertyType, subProperty, part);
                if (subEqExpression == null) {
                    continue;
                }
                subOrExpression = subOrExpression == null
                    ? subEqExpression
                    : Expression.OrElse(subOrExpression, subEqExpression);
            }
            return subOrExpression;
        }

        private static Expression GetLinkedEntityEqualsExpression(ParameterExpression argParam, string part,
                                                                  EntityPropertyInfo propertyInfo) {
            var property = Expression.Property(argParam, propertyInfo.PropertyName);
            var propertyEntityType = propertyInfo.BaseEntityType;
            if (!propertyEntityType.IsInterfaceImplementationOf<IEntity>()) {
                return null;
            }
            var propertyEntityInfo = EntityConverter.GetEntityTypeInfo(propertyEntityType);
            Expression subOrExpression = null;
            foreach (var linkedProperty in propertyEntityInfo.SearchProperties) {
                if (linkedProperty.Equals(propertyEntityInfo.PrimaryKeyInfo)) {
                    continue;
                }
                var subProperty = Expression.Property(property, linkedProperty.PropertyInfo);
                var subEqExpression = CreateEqualityExpression(linkedProperty.PropertyType, subProperty, part);
                if (subEqExpression == null) {
                    continue;
                }
                subOrExpression = subOrExpression == null
                    ? subEqExpression
                    : Expression.OrElse(subOrExpression, subEqExpression);
            }
            return subOrExpression;
        }

        private static Expression GetLinkedEntitiesEqualsExpression(ParameterExpression argParam, string part,
                                                                    EntityPropertyInfo propertyInfo) {
            var property = Expression.Property(argParam, propertyInfo.PropertyName);
            var propertyEntityType = propertyInfo.BaseEntityType;
            if (!propertyEntityType.IsInterfaceImplementationOf<IEntity>()) {
                return null;
            }
            var propertyEntityInfo = EntityConverter.GetEntityTypeInfo(propertyEntityType);
            Expression subOrExpression = null;
            var subArgParam = Expression.Parameter(propertyEntityType, "se");
            foreach (var linkedProperty in propertyEntityInfo.SearchProperties) {
                if (linkedProperty.Equals(propertyEntityInfo.PrimaryKeyInfo)) {
                    continue;
                }
                var subProperty = Expression.Property(subArgParam, linkedProperty.PropertyInfo);
                var subEqExpression = CreateEqualityExpression(linkedProperty.PropertyType, subProperty, part);
                if (subEqExpression == null) {
                    continue;
                }
                subOrExpression = subOrExpression == null
                    ? subEqExpression
                    : Expression.OrElse(subOrExpression, subEqExpression);
            }
            if (subOrExpression == null) {
                return null;
            }
            var anyMethod =
                typeof(Enumerable).GetMethods()
                    .First(m => m.Name == "Any" && m.GetParameters().Length == 2)
                    .MakeGenericMethod(propertyEntityType);
            var lambda = Expression.Lambda(subOrExpression, subArgParam);
            var anyCall = Expression.Call(null, anyMethod, property, lambda);
            return anyCall;
        }

        private static Expression CombineToAndAlsoExpression(Expression andExpression, Expression orExpression) {
            andExpression = andExpression == null ? orExpression : Expression.AndAlso(andExpression, orExpression);
            return andExpression;
        }

        private static Expression CombineToOrExpression(Expression orExpression, Expression eqExpression) {
            orExpression = orExpression == null ? eqExpression : Expression.OrElse(orExpression, eqExpression);
            return orExpression;
        }

        private static Expression CreateEqualityExpression(Type propertyType, MemberExpression property,
                                                           string searchText) {
            if (!propertyType.IsPrimitive && propertyType != typeof(string) && propertyType != typeof(DateTime)) {
                return null;
            }
            var value = CreateConstantExpression(propertyType, searchText);
            if (value == null) {
                return null;
            }
            if (propertyType != typeof(string)) {
                return Expression.Equal(property, value);
            }

            var containsMethodInfo = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            return Expression.Call(property, containsMethodInfo, value);
        }

        private static ConstantExpression CreateConstantExpression(Type propertyType, string searchText) {
            ConstantExpression value = null;
            if (propertyType == typeof(string)) {
                value = Expression.Constant(searchText);
            } else if (propertyType == typeof(DateTime)) {
                var val = Converter.GetDelegate(propertyType)(searchText);
                if (!val.Equals(Activator.CreateInstance(propertyType)) && !val.Equals(DateTimeHelpers.BaseDate)) {
                    value = Expression.Constant(val, propertyType);
                }
            } else if (propertyType.IsNumeric()) {
                decimal d;
                object val;
                if (decimal.TryParse(searchText, NumberStyles.Any, CultureInfo.InvariantCulture, out d)) {
                    val = Converter.GetDelegate(propertyType)(searchText);
                } else {
                    val = Converter.GetDelegate(propertyType)(null);
                }
                if (!val.Equals(Activator.CreateInstance(propertyType))) {
                    value = Expression.Constant(val, propertyType);
                }
            }
            return value;
        }
    }
}