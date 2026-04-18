using BECOSOFT.Utilities.Extensions;
using BECOSOFT.Utilities.Helpers;
using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;

namespace BECOSOFT.Utilities.Converters {
    /// <summary>
    /// Class for creating types
    /// </summary>
    /// <typeparam name="T">The type to create</typeparam>
    public static class TypeActivator<T> {
        /// <summary>
        /// Creates an instance of type <see cref="T"/>.
        /// <para>!! If the provided type does not have a default constructor, the object is initialised with all fields/properties set to their <see langword="default"/> value. !!</para>
        /// </summary>
        public static readonly Func<T> Instance = Creator();

        /// <summary>
        /// Create the creator
        /// </summary>
        /// <returns>The creator</returns>
        public static Func<T> Creator() {
            var t = typeof(T);
            if (t == typeof(string)) {
                var emptyField = typeof(string).GetField("Empty", BindingFlags.Static | BindingFlags.Public);
                return Expression.Lambda<Func<T>>(Expression.Field(null, emptyField)).Compile();
            }

            if (t == typeof(DBNull)) {
                return Expression.Lambda<Func<T>>(Expression.Constant(DBNull.Value)).Compile();
            }

            if (t == typeof(Guid)) {
                return Expression.Lambda<Func<T>>(Expression.Constant(Guid.Empty)).Compile();
            }

            if (t == typeof(DateTime)) {
                return Expression.Lambda<Func<T>>(Expression.Constant(DateTimeHelpers.BaseDate)).Compile();
            }

            if (t == typeof(byte[])) {
                return Expression.Lambda<Func<T>>(Expression.Constant(new byte[0])).Compile();
            }

            if (t.HasDefaultConstructor()) {
                return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();
            }

            return () => (T) FormatterServices.GetUninitializedObject(t);
        }
    }

    /// <summary>
    /// Class for statically creating types 
    /// </summary>
    public static class TypeActivator {
        /// <summary>
        /// Creates an instance of a type.
        /// <para>!! If the provided type does not have a default constructor, the object is initialised with all fields/properties set to their <see langword="default"/> value. !!</para>
        /// </summary>
        /// <param name="t">The type</param>
        /// <returns>The instance</returns>
        public static object CreateInstance(Type t) {
            if (t == typeof(string)) {
                return string.Empty;
            }
            if (t == typeof(DBNull)) {
                return DBNull.Value;
            }

            if (t == typeof(Guid)) {
                return Guid.Empty;
            }

            if (t == typeof(DateTime)) {
                return DateTimeHelpers.BaseDate;
            }

            if (t == typeof(byte[])) {
                return new byte[0];
            }

            if (t.HasDefaultConstructor()) {
                return Activator.CreateInstance(t);
            }

            return FormatterServices.GetSafeUninitializedObject(t);
        }
    }
}