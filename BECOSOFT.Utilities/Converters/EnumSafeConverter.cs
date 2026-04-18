using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Converters {
    public static class EnumSafeConverter<T> {
        public static readonly Func<string, T> Instance = Create();

        public static Func<string, T> Create() {
            var paramExpression = Expression.Parameter(typeof(string));
            var outType = typeof(T);
            var outTypeInformation = outType.GetTypeInformation();
            Expression body;
            var converterType = typeof(EnumConverterHelper);
            var tryParseMethod = converterType.GetMethod("TryParse");
            if (tryParseMethod == null) {
                throw new ArgumentException(nameof(EnumConverterHelper.TryParse));
            }
            Type tupleType;
            if (outTypeInformation.IsNullableOf && outTypeInformation.BaseType.IsEnum) {
                tupleType = typeof(Tuple<,>).MakeGenericType(typeof(bool), outTypeInformation.BaseType);
                var convertMethod = converterType.GetMethod("Convert");
                if (convertMethod == null) {
                    throw new ArgumentException(nameof(EnumConverterHelper.Convert));
                }
                var method = tryParseMethod.MakeGenericMethod(outTypeInformation.BaseType);
                var parse = Expression.Call(method, paramExpression);
                var resultExpr = Expression.Variable(tupleType);
                var itemMethod = tupleType.GetProperty("Item1");
                var item2Method = tupleType.GetProperty("Item2");
                if (itemMethod == null) {
                    throw new ArgumentException(nameof(Tuple<bool, T>.Item1));
                }
                if (item2Method == null) {
                    throw new ArgumentException(nameof(Tuple<bool, T>.Item2));
                }
                var convert = convertMethod.MakeGenericMethod(outType, outTypeInformation.BaseType);
                var returnTarget = Expression.Label(outType);
                var block = Expression.Block(new[]{resultExpr},
                    Expression.Assign(resultExpr, parse), 
                    Expression.IfThenElse(
                        Expression.Property(resultExpr, itemMethod),  
                        Expression.Return(returnTarget,
                                          Expression.Convert(
                                              Expression.Call(convert, Expression.Property(resultExpr, item2Method))
                                              , outType)
                                          ),
                        Expression.Return(returnTarget, Expression.Default(outType))),
                    Expression.Label(returnTarget, Expression.Default(outType))
                    );
                body = block;
            } else {
                tupleType = typeof(Tuple<,>).MakeGenericType(typeof(bool), outType);
                var method = tryParseMethod.MakeGenericMethod(outType);
                body = Expression.Call(method, paramExpression);
                var item2Method = tupleType.GetProperty("Item2");
                if (item2Method == null) {
                    throw new ArgumentException(nameof(Tuple<bool, T>.Item2));
                }
                body = Expression.Property(body, item2Method);
            }
            return Expression.Lambda<Func<string, T>>(body, paramExpression).Compile();
        }
    }
}