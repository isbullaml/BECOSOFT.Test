using BECOSOFT.Data.Converters;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Data.Extensions {
    public static class ExpressionExtensions {

        /// <summary>
        /// Returns the <see cref="PropertyInfo"/> of the <see cref="selector"/>.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        internal static PropertyInfo GetPropertyInfo(this Expression selector) {
            var memberExpression = GetMemberExpression(selector);
            if (memberExpression == null) { return null; }

            var propertyInfo = (PropertyInfo) memberExpression.Member;
            return propertyInfo;
        }
        
        /// <summary>
        /// Returns the <see cref="EntityPropertyInfo"/> of the <see cref="selector"/>.
        /// </summary>
        /// <param name="selector"></param>
        /// <returns></returns>
        internal static EntityPropertyInfo GetProperty(this Expression selector) {
            if (selector is LambdaExpression expression) {
                var constantExpression = (expression.Body as ConstantExpression);
                var ct = constantExpression?.Value?.ToString();
                if (ct != null) {
                    var eti = EntityConverter.GetEntityTypeInfo(expression.Parameters[0].Type);
                    return eti.GetPropertyInfo(ct, null);
                }
                if (expression.Body.NodeType == ExpressionType.Convert || expression.Body.NodeType == ExpressionType.ConvertChecked) {
                    var unaryExpression = expression.Body as UnaryExpression;
                    if (unaryExpression != null) {
                        var operandExpr = unaryExpression.Operand as MemberExpression;
                        return GetPropertyInfo(operandExpr);
                    }
                }
            }
            var memberExpression = GetMemberExpression(selector);
            
            return GetPropertyInfo(memberExpression);

            EntityPropertyInfo GetPropertyInfo(MemberExpression me) {
                if (me == null) { return null; }

                var propertyInfo = (PropertyInfo)me.Member;
                var entityTypeInfo = EntityConverter.GetEntityTypeInfo(me.Expression.Type);
                return entityTypeInfo.GetPropertyInfo(propertyInfo.Name, null);
            }
        }

        internal static Tuple<EntityPropertyInfo, EntityPropertyInfo> GetPropertyWithParent(this Expression selector) {
            var memberExpression = GetMemberExpression(selector);
            var propertyInfo = (PropertyInfo) memberExpression.Member;
            var entityTypeInfo = EntityConverter.GetEntityTypeInfo(propertyInfo.DeclaringType);
            EntityPropertyInfo parentPropertyInfo = null;
            if (memberExpression.Expression.NodeType != ExpressionType.Parameter) {
                parentPropertyInfo = memberExpression.Expression.GetProperty();
            }
            return Tuple.Create(parentPropertyInfo, entityTypeInfo.GetPropertyInfo(propertyInfo.Name, null));
        }

        private static MemberExpression GetMemberExpression(Expression selector) {
            var body = selector;
            if (body is LambdaExpression expression) {
                body = expression.Body;
            }
            if (body == null) {
                return null;
            }
            MemberExpression memberExpression;
            switch (body.NodeType) {
                case ExpressionType.MemberAccess:
                    memberExpression = (MemberExpression)body;
                    break;
                case ExpressionType.Convert:
                    memberExpression = (MemberExpression)((UnaryExpression)body).Operand;
                    break;
                case ExpressionType.Call:
                    memberExpression = (MemberExpression) ((MethodCallExpression) body).Object;
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return memberExpression;
        }

        public static Expression Simplify(this Expression expression) {
            var searcher = new ParameterlessExpressionSearcher();
            searcher.Visit(expression);
            return new ParameterlessExpressionEvaluator(searcher.ParameterlessExpressions).Visit(expression);
        }

        public static Expression<T> Simplify<T>(this Expression<T> expression) {
            return (Expression<T>)Simplify((Expression)expression);
        }

        private class ParameterlessExpressionSearcher : ExpressionVisitor {
            public HashSet<Expression> ParameterlessExpressions { get; } = new HashSet<Expression>();
            private bool _containsParameter;

            public override Expression Visit(Expression node) {
                var originalContainsParameter = _containsParameter;
                _containsParameter = false;
                base.Visit(node);
                if (!_containsParameter) {
                    if (node?.NodeType == ExpressionType.Parameter) {
                        _containsParameter = true;
                    } else {
                        ParameterlessExpressions.Add(node);
                    }
                }
                _containsParameter |= originalContainsParameter;

                return node;
            }
        }
        private class ParameterlessExpressionEvaluator : ExpressionVisitor {
            private readonly HashSet<Expression> _parameterlessExpressions;
            public ParameterlessExpressionEvaluator(HashSet<Expression> parameterlessExpressions) {
                _parameterlessExpressions = parameterlessExpressions;
            }
            public override Expression Visit(Expression node) {
                if (_parameterlessExpressions.Contains(node)) {
                    return Evaluate(node);
                }
                return base.Visit(node);
            }

            private static Expression Evaluate(Expression node) {
                if (node.NodeType == ExpressionType.Constant) {
                    return node;
                }
                var value = Expression.Lambda(node).Compile().DynamicInvoke();
                return Expression.Constant(value, node.Type);
            }
        }
    }
}