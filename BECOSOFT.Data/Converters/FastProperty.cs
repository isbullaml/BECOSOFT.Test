using BECOSOFT.Data.Collections;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Data.Converters {
    /// <summary>
    /// Class for all info about a fast property
    /// </summary>
    internal class FastProperty : IFastDelegate {
        /// <summary>
        /// The info about the property
        /// </summary>
        public PropertyInfo Property { get; set; }

        /// <summary>
        /// Delegate to get
        /// </summary>
        public Func<object, object> GetDelegate { get; private set; }

        /// <summary>
        /// Delegate to set
        /// </summary>
        public Action<object, object> SetDelegate { get; private set; }

        public FastProperty(PropertyInfo property, bool includeGet = true, bool includeSet = true) {
            Property = property;
            if (includeGet) {
                InitializeGet();
            }

            if (includeSet) {
                InitializeSet();
            }
        }

        private void InitializeSet() {
            var instance = Expression.Parameter(typeof(object), "instance");
            var value = Expression.Parameter(typeof(object), "value");

            // value as T is slightly faster than (T)value, so if it's not a value type, use that
            var instanceCast = !Property.DeclaringType.IsValueType ? Expression.TypeAs(instance, Property.DeclaringType) : Expression.Convert(instance, Property.DeclaringType);

            Expression valueCast;
            if (Property.PropertyType.IsGenericList()) {
                var listType = Property.PropertyType.GetGenericArguments().First();
                MethodInfo listMethod = null;
                if (Property.PropertyType.IsSubclassOfRawGeneric(typeof(ObserverList<>))) {
                    listMethod = typeof(Extensions.EnumerableExtensions).GetMethod("ToObserverList");
                } else if (Property.PropertyType.IsSubclassOfRawGeneric(typeof(List<>))) {
                    listMethod = typeof(Utilities.Extensions.Collections.EnumerableExtensions).GetMethod("ToSafeList");
                }
                if (listMethod == null) {
                    throw new ArgumentException($"'{Property.PropertyType.GetNameWithoutGenerics()}' is not supported as settable type for entity properties ");
                }
                var genericList = listMethod.MakeGenericMethod(listType);
                var castMethod = typeof(Enumerable).GetMethod("Cast");
                if (castMethod == null) {
                    throw new ArgumentException("'Cast' method not found on Enumerable.");
                }
                var genericCast = castMethod.MakeGenericMethod(listType);
                var castArg = Expression.TypeAs(value, typeof(IEnumerable));
                var toObserverListArg = Expression.Call(null, genericCast, castArg);
                var fullExpression = Expression.Call(null, genericList, toObserverListArg);
                var nullExpression = Expression.Constant(null);
                valueCast = Expression.Condition(Expression.Equal(value, nullExpression), Expression.TypeAs(nullExpression, Property.PropertyType), fullExpression);
            } else if (Property.PropertyType.IsSubclassOf(typeof(BaseEntity))) {
                valueCast = Expression.TypeAs(value, Property.PropertyType);
            } else {
                var converter = typeof(Converter);
                var converterMethod = converter.GetMethod("GetValue", new[] { typeof(object) });
                var genericMethod = converterMethod.MakeGenericMethod(Property.PropertyType);
                if (converterMethod == null) {
                    throw new ArgumentException("'GetValue' method not found on Converter.");
                }
                valueCast = Expression.Call(null, genericMethod, value);
            }

            //valueCast = (!Property.PropertyType.IsValueType)
            //    ? Expression.TypeAs(valueCast, Property.PropertyType)
            //    : Expression.Convert(valueCast, Property.PropertyType);
            var callExpression = Expression.Call(instanceCast, Property.GetSetMethod(!Property.SetMethod.IsPublic), valueCast);
            SetDelegate = Expression.Lambda<Action<object, object>>(callExpression, instance, value).Compile();
        }

        private void InitializeGet() {
            var instance = Expression.Parameter(typeof(object), "instance");
            var instanceCast = !Property.DeclaringType.IsValueType ? Expression.TypeAs(instance, Property.DeclaringType) : Expression.Convert(instance, Property.DeclaringType);
            var callExpression = Expression.Call(instanceCast, Property.GetGetMethod(!Property.GetMethod.IsPublic));
            GetDelegate = Expression.Lambda<Func<object, object>>(Expression.TypeAs(callExpression, typeof(object)), instance).Compile();
        }
    }
}