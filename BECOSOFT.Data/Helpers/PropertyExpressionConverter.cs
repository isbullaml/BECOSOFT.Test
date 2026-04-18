using BECOSOFT.Data.Context;
using System;
using System.Linq.Expressions;

namespace BECOSOFT.Data.Helpers {
    internal static class PropertyExpressionConverter {
        /// <summary>
        /// Converts an Expression{Func{T, TProp}} to an Expression{Func{T, object}}. Which is necessary to use <see cref="Entity.GetColumn()"/>
        /// </summary>
        /// <typeparam name="TInput"></typeparam>
        /// <typeparam name="TOutput"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        internal static Expression<Func<TInput, object>> ConvertToObjectPropertySelector<TInput, TOutput>(Expression<Func<TInput, TOutput>> expression) {
            Expression converted = Expression.Convert(expression.Body, typeof(object));
            return Expression.Lambda<Func<TInput, object>>(converted, expression.Parameters);
        }
    }
}