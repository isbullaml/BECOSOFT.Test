using BECOSOFT.Data.Models;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Extensions {
    public static class QueryableExtensions {
        public static IOrderedQueryable<TSource> OrderBy<TSource, TKey>(this IQueryable<TSource> values, Expression<Func<TSource, TKey>> keySelector, bool descending)
            => descending ? values.OrderByDescending(keySelector) : values.OrderBy(keySelector);

        public static IOrderedQueryable<TSource> ThenBy<TSource, TKey>(this IOrderedQueryable<TSource> values, Expression<Func<TSource, TKey>> keySelector, bool descending)
            => descending ? values.ThenByDescending(keySelector) : values.ThenBy(keySelector);

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> values, OrderDefinition<TSource> orderDefinition)
            => values.OrderBy(orderDefinition.OrderByExpression, !orderDefinition.IsAsc);

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> values, OrderDefinition<TSource> orderDefinition)
            => values.ThenBy(orderDefinition.OrderByExpression, !orderDefinition.IsAsc);
    }
}
