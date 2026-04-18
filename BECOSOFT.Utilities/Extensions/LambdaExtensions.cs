using System;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Extensions {

    /// <summary>
    /// Extensions for <see cref="Expression{TDelegate}"/>
    /// Source: https://stackoverflow.com/a/457328/4182837
    /// </summary>
    public static class LambdaExtensions {
        /// <summary>
        /// Performs an AND on two expressions
        /// Example:
        /// 1: a => a.Id == 1
        /// 2: a => a.Property &gt; 7
        /// Combined: a => a.Id == 1 &amp;&amp; a.Property &gt; 7
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right) {
            return Merge(left, right, Expression.AndAlso);
        }

        /// <summary>
        /// Performs an OR on two expressions
        /// Example:
        /// 1: a => a.Id == 1
        /// 2: a => a.Property &gt; 7
        /// Combined: a => a.Id == 1 || a.Property &gt; 7
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> left, Expression<Func<T, bool>> right) {
            return Merge(left, right, Expression.OrElse);
        }

        // https://stackoverflow.com/a/457328
        private static Expression<Func<T, bool>> Merge<T>(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right, 
                                                          Func<Expression, Expression, BinaryExpression> mergeFunc) {
            if (left == null) { return right; }
            if (right == null) { return left; }

            var parameter = Expression.Parameter(typeof(T));
            var leftVisitor = new ReplaceExpressionVisitor(left.Parameters[0], parameter);
            var leftVisited = leftVisitor.Visit(left.Body);

            var rightVisitor = new ReplaceExpressionVisitor(right.Parameters[0], parameter);
            var rightVisited = rightVisitor.Visit(right.Body);

            var bodyExpression = mergeFunc(leftVisited, rightVisited);
            return Expression.Lambda<Func<T, bool>>(bodyExpression, parameter);
        }

        // https://stackoverflow.com/a/457328
        private class ReplaceExpressionVisitor : ExpressionVisitor {

            private readonly Expression _oldValue;
            private readonly Expression _newValue;

            public ReplaceExpressionVisitor(Expression oldValue, Expression newValue) {
                _oldValue = oldValue;
                _newValue = newValue;
            }

            public override Expression Visit(Expression node) {
                return node == _oldValue ? _newValue : base.Visit(node);
            }
        }
    }
}