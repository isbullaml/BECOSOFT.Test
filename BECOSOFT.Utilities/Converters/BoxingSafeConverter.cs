using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Models;
using System;
using System.Diagnostics;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Converters {
    public static class BoxingSafeConverter<TIn, TOut> {
        [DebuggerHidden]
        public static Func<TIn, TOut> Instance { get; } = Create();

        [DebuggerHidden]
        private static Func<TIn, TOut> Create() {
            return CreateLambda()?.Compile();
        }

        [DebuggerHidden]
        private static Expression<Func<TIn, TOut>> CreateLambda() {
            var inType = typeof(TIn);
            var paramExpression = Expression.Parameter(inType);
            var outType = typeof(TOut);
            var outTypeInformation = outType.GetTypeInformation();
            Expression expression = paramExpression;
            if (!BoxingSafeConverter.CanConvert(inType, inType.GetTypeInformation(), outType, outTypeInformation)) {
                return null;
            }
            try {
                if (outTypeInformation.IsEnum || inType != outType) {
                    expression = Expression.Convert(expression, outType);
                }
            } catch (InvalidOperationException) {
                return null;
            }
            return Expression.Lambda<Func<TIn, TOut>>(expression, paramExpression);
        }
    }

    public static class BoxingSafeConverter {
        [DebuggerHidden]
        public static bool CanConvert(Type inType, Type outType) {
            var inTypeInformation = inType.GetTypeInformation();
            var outTypeInformation = outType.GetTypeInformation();
            return CanConvert(inType, inTypeInformation, outType, outTypeInformation);
        }
        
        [DebuggerHidden]
        public static bool CanConvert(Type inType, TypeInformation inTypeInformation,
                                      Type outType, TypeInformation outTypeInformation) {
            var inBaseType = inTypeInformation.BaseType;
            var outBaseType = outTypeInformation.BaseType;
            if (inType == outType || inBaseType == outBaseType) {
                return true;
            }

            if (inType == typeof(DBNull) || inType == typeof(Guid) || inType == typeof(Guid?) || outType == typeof(DBNull) || outType == typeof(Guid) || outType == typeof(Guid?)) {
                return false;
            }

            if ((inType == typeof(bool) || inBaseType == typeof(bool)) && (outType.IsNumeric() || outType == typeof(char) || outBaseType.IsNumeric() || outBaseType == typeof(char))) {
                return true;
            }
            if ((outType == typeof(bool) || outBaseType == typeof(bool)) && (inType.IsNumeric() || inType == typeof(char) || inBaseType.IsNumeric() || inBaseType == typeof(char))) {
                return false;
            }
            var inTypeIsString = inType == typeof(string);
            if (inTypeIsString && (outType.IsPrimitive || outTypeInformation.IsNumeric || outType == typeof(DateTime))) {
                return false;
            }

            if (inType == typeof(DateTime) && (outType.IsPrimitive || outTypeInformation.IsNumeric)) {
                return false;
            }
            if (outType == typeof(DateTime) && (inType.IsPrimitive || inTypeInformation.IsNumeric)) {
                return false;
            }
            return true;
        }
    }
}