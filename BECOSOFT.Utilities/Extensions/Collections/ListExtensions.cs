using BECOSOFT.Utilities.Converters;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Extensions.Collections {
    /// <summary>
    /// Extensions for the <see cref="IList{T}"/>-class
    /// </summary>
    public static class ListExtensions {
        /// <summary>
        /// Retrieve a value from the provided <see cref="IList{T}"/> by the provided index and casts it to <see cref="TOut"/>.
        /// If the index doesn't exist in the list, the fallback <see cref="defaultValue"/> is used.
        /// </summary>
        /// <typeparam name="TIn">Type of the list values</typeparam>
        /// <typeparam name="TOut">Type to cast the value to</typeparam>
        /// <param name="values">Source list</param>
        /// <param name="index">The index to find the value for</param>
        /// <param name="defaultValue">
        ///     (Optional) Default value, used if the index is out of bounds for the <see cref="values"/> sequence.
        ///     The default value of <see cref="TOut"/> is used when this is not specified.
        /// </param>
        /// <returns>The value from the list, casted to <see cref="TOut"/></returns>
        public static TOut GetValue<TIn, TOut>(this IList<TIn> values, int index, TOut defaultValue = default(TOut)) {
            if (index < 0 || values.Count <= index) {
                return defaultValue;
            }

            var value = values[index];
            var parsedValue = value.To<TOut>();
            return parsedValue;
        }

        /// <summary>
        /// Creates a copy of the provided <see cref="IList{T}"/>. Note: This action does not copy the values.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IList{T}"/></typeparam>
        /// <param name="list">List to copy</param>
        /// <returns>A copy of the provided <see cref="IList{T}"/></returns>
        public static List<T> Copy<T>(this IList<T> list) {
            var tempList = list.ToSafeList();
            var result = new List<T>(tempList);
            return result;
        }

        /// <summary>
        /// Removes all the elements that match the conditions defined by the specified predicate.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="List{T}"/></typeparam>
        /// <param name="list">The list to remove from.</param>
        /// <param name="match">The <see cref="Expression{TT}" /> delegate that defines the conditions of the elements to remove.</param>
        /// <returns>The number of elements removed from the <see cref="IList{T}" /> .</returns>
        public static int RemoveAll<T>(this List<T> list, Expression<Func<T, bool>> match) {
            Predicate<T> predicate = match.Compile().Invoke;
            return list.RemoveAll(predicate);
        }
    }
}
