using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Helpers {
    public class FlagEnumHelper : AbstractFlagEnumHelper<Enum> {

    }

    public abstract class AbstractFlagEnumHelper<T> where T : class {
        /// <summary>
        /// Converts a collection of values to a single enum value.
        /// </summary>
        /// <typeparam name="TEnum">Type of the <see cref="Enum"/> with the <see cref="FlagsAttribute"/>.</typeparam>
        /// <param name="values">Values to convert</param>
        /// <param name="defaultValue">Default value when <see cref="values"/> is empty.</param>
        /// <exception cref="InvalidEnumArgumentException"><see cref="TEnum"/> is missing the <see cref="FlagsAttribute"/></exception>
        public static TEnum ConvertToFlags<TEnum>(IEnumerable<TEnum> values, TEnum defaultValue = default(TEnum)) where TEnum : struct, IConvertible, T {
            var type = typeof(TEnum);
            var typeInfo = type.GetTypeInformation();
            if (!typeInfo.IsEnumWithFlag) {
                throw new InvalidEnumArgumentException();
            }
            var list = values.ToSafeList();
            if (list.IsEmpty()) { return defaultValue; }
            var value = list.Aggregate(FlagEnumHelper<TEnum>.OrFunction);
            return value;
        }

        /// <summary>
        /// Converts a collection of values to a single enum value.
        /// </summary>
        /// <typeparam name="TEnum">Type of the <see cref="Enum"/> with the <see cref="FlagsAttribute"/>.</typeparam>
        /// <param name="values">Values to convert</param>
        /// <param name="defaultValue">Default value when <see cref="values"/> is empty.</param>
        /// <exception cref="InvalidEnumArgumentException"><see cref="TEnum"/> is missing the <see cref="FlagsAttribute"/></exception>
        /// <returns></returns>
        public static TEnum ConvertToFlags<TEnum>(IEnumerable<int> values, TEnum defaultValue = default(TEnum)) where TEnum : struct, IConvertible, T {
            var type = typeof(TEnum);
            var typeInfo = type.GetTypeInformation();
            if (!typeInfo.IsEnumWithFlag) {
                throw new InvalidEnumArgumentException();
            }
            var list = values.ToSafeList();
            if (list.IsEmpty()) { return defaultValue; }
            var value = list.Select(v => v.To<TEnum>()).Aggregate(FlagEnumHelper<TEnum>.OrFunction);
            return value;
        }

        private class FlagEnumHelper<TEnum> where TEnum : struct, IConvertible, T {
            public static readonly Func<TEnum, TEnum, TEnum> OrFunction = CreateOrFunction();

            private static Func<TEnum, TEnum, TEnum> CreateOrFunction() {
                var type = typeof(TEnum);
                var typeInfo = type.GetTypeInformation();
                if (!typeInfo.IsEnumWithFlag) {
                    throw new InvalidEnumArgumentException();
                }
                var underlyingType = typeInfo.UnderlyingType;
                var parameters = new[] { Expression.Parameter(type), Expression.Parameter(type) };
                var func = Expression.Convert(
                    Expression.Or(
                        Expression.Convert(parameters[0], underlyingType),
                        Expression.Convert(parameters[1], underlyingType)
                    )
                    , type);
                return Expression.Lambda<Func<TEnum, TEnum, TEnum>>(func, parameters).Compile();
            }
        }
    }
}