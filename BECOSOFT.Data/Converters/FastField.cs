using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Converters;
using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Extensions.Collections;
using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BECOSOFT.Data.Converters {
    /// <summary>
    /// Class for all info about a fast field
    /// </summary>
    internal class FastField : IFastDelegate {
        /// <summary>
        /// The info about the field
        /// </summary>
        public FieldInfo Field { get; set; }

        /// <summary>
        /// Delegate to get
        /// </summary>
        public Func<object, object> GetDelegate { get; private set; }

        /// <summary>
        /// Delegate to set
        /// </summary>
        public Action<object, object> SetDelegate { get; private set; }

        public FastField(FieldInfo field, bool includeGet = true, bool includeSet = true) {
            Field = field;
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
            var instanceCast = !Field.DeclaringType.IsValueType ? Expression.TypeAs(instance, Field.DeclaringType) : Expression.Convert(instance, Field.DeclaringType);

            Expression valueCast;
            if (Field.FieldType.IsGenericList()) {
                var listType = Field.FieldType.GetGenericArguments().First();
                var listMethod = typeof(Extensions.EnumerableExtensions).GetMethod("ToObserverList");
                var genericList = listMethod.MakeGenericMethod(listType);
                var castMethod = typeof(Enumerable).GetMethod("Cast");
                var genericCast = castMethod.MakeGenericMethod(listType);
                valueCast = Expression.Call(null, genericList, Expression.Call(null, genericCast, Expression.TypeAs(value, typeof(IEnumerable))));
            } else if (Field.FieldType.IsSubclassOf(typeof(BaseEntity))) {
                valueCast = Expression.TypeAs(value, Field.FieldType);
            } else {
                var converter = typeof(Converter);
                var converterMethod = converter.GetMethod("GetValue", new[] { typeof(object) });
                var genericMethod = converterMethod.MakeGenericMethod(Field.FieldType);
                valueCast = Expression.Call(null, genericMethod, value);
            }

            //valueCast = (!Property.PropertyType.IsValueType)
            //    ? Expression.TypeAs(valueCast, Property.PropertyType)
            //    : Expression.Convert(valueCast, Property.PropertyType);

            // ((int)obj).Field = x
            var fieldExpression = Expression.Field(instanceCast, Field);
            Expression assignExpression;
            if (Field.IsInitOnly) {
                valueCast = Expression.TypeAs(valueCast, typeof(object));
                var methodInfo = Field.GetType().GetMethods().First(m => m.Name.Equals("SetValue") && m.GetParameters().Length == 2);
                assignExpression = Expression.Call(Expression.Constant(Field), methodInfo, instance, valueCast);
            } else {
                assignExpression = Expression.Assign(fieldExpression, valueCast);
            }

            SetDelegate = Expression.Lambda<Action<object, object>>(assignExpression, instance, value).Compile();
        }

        private void InitializeGet() {
            var instance = Expression.Parameter(typeof(object), "instance");
            var instanceCast = !Field.DeclaringType.IsValueType ? Expression.TypeAs(instance, Field.DeclaringType) : Expression.Convert(instance, Field.DeclaringType);
            GetDelegate = Expression.Lambda<Func<object, object>>(Expression.TypeAs(Expression.Field(instanceCast, Field), typeof(object)), instance).Compile();
        }
    }
}