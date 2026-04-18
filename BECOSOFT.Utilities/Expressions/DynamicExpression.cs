using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Utilities.Expressions {
    /// <summary>
    /// Source: https://github.com/kahanu/System.Linq.Dynamic
    /// </summary>
    public static class DynamicExpression {
        public static Expression Parse(Type resultType, string expression, params object[] values) {
            var parser = new ExpressionParser(expression, null, values);
            return parser.Parse(resultType);
        }

        public static Expression Parse<TResult>(string expression, params object[] values) {
            var parser = new ExpressionParser(expression, null, values);
            return parser.Parse(typeof(TResult));
        }

        public static Func<T, TResult> ParseFunc<T, TResult>(string expression, params object[] values) {
            var lambda = ParseLambda<T, TResult>(expression, values);
            return lambda.Compile();
        }

        public static Expression<Func<T, TResult>> ParseLambda<T, TResult>(string expression, params object[] values) {
            return (Expression<Func<T, TResult>>) ParseLambda(typeof(T), typeof(TResult), expression, values);
        }

        public static LambdaExpression ParseLambda(Type itType, Type resultType, string expression, params object[] values) {
            return ParseLambda(new[] { Expression.Parameter(itType, "") }, resultType, expression, values);
        }

        public static LambdaExpression ParseLambda(ParameterExpression[] parameters, Type resultType, string expression, params object[] values) {
            var parser = new ExpressionParser(expression, parameters, values);
            return Expression.Lambda(parser.Parse(resultType), parameters);
        }

        public static Type CreateClass(params DynamicProperty[] properties) {
            return ExpressionParser.ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static Type CreateClass(IEnumerable<DynamicProperty> properties) {
            return ExpressionParser.ClassFactory.Instance.GetDynamicClass(properties);
        }

        public static bool TryParse(Type resultType, string expression, out Expression result, params object[] values) {
            var parser = new ExpressionParser(expression, null, values, noExceptions: true);
            result = parser.Parse(resultType);
            return result != null;
        }

        public static bool TryParse<TResult>(string expression, out Expression result, params object[] values) {
            var parser = new ExpressionParser(expression, null, values);
            result = parser.Parse(typeof(TResult));
            return result != null;
        }

        public static bool TryParseFunc<T, TResult>(string expression, out Func<T, TResult> result, params object[] values) {
            var parseResult = TryParseLambda(expression, out Expression<Func<T, TResult>> lambda, values);
            result = lambda?.Compile();
            return parseResult;
        }

        public static bool CreateTryParseFunc(Type itType, Type resultType, string expression, out Delegate result, params object[] values) {
            var dynamicExpressionType = typeof(DynamicExpression);
            var genericTryParseFunc = dynamicExpressionType.GetMethod("TryParseFunc", BindingFlags.Public | BindingFlags.Static);
            var genericHelper = genericTryParseFunc.MakeGenericMethod(itType, resultType);
            //result = null;
            var parameters = new object[] { expression, null, values };
            var parseResult = (bool) genericHelper.Invoke(null, parameters);
            result = (Delegate) parameters[1];
            return parseResult;
        }

        public static bool TryParseLambda<T, TResult>(string expression, out Expression<Func<T, TResult>> result, params object[] values) {
            var parseResult = TryParseLambda(typeof(T), typeof(TResult), expression, out var temp, values);
            result = (Expression<Func<T, TResult>>) temp;
            return parseResult;
        }

        public static bool TryParseLambda(Type itType, Type resultType, string expression, out LambdaExpression result, params object[] values) {
            return TryParseLambda(new[] { Expression.Parameter(itType, "") }, resultType, expression, out result, values);
        }

        public static bool TryParseLambda(ParameterExpression[] parameters, Type resultType, string expression, out LambdaExpression result, params object[] values) {
            var parser = new ExpressionParser(expression, parameters, values, noExceptions: true);
            var parseResult = parser.Parse(resultType);
            if (parseResult != null) {
                result = Expression.Lambda(parseResult, parameters);
                return true;
            }
            result = null;
            return false;
        }
    }
}