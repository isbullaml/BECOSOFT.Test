using System;
using System.Collections.Generic;

namespace BECOSOFT.Utilities.Extensions.Collections {
    public static class CollectionExtensions {

        /// <summary>
        /// Find all indices that conform to the given predicate in the provided collection.
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <param name="values">Collection to match the predicate against</param>
        /// <param name="match">Predicate to match the items with</param>
        /// <returns>All indices that conform to the given predicate in the provided collection</returns>
        public static List<int> FindIndices<T>(this IList<T> values, Predicate<T> match) {
            var result = new List<int>();
            for (var i = 0; i < values.Count; i++) {
                if (match(values[i])) {
                    result.Add(i);
                }
            }
            return result;
        }

        /// <summary>
        /// Determines all indices of the provided items in the given collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">Collection to locate the items in</param>
        /// <param name="items">The items to locate in the collection</param>
        /// <returns></returns>
        public static Dictionary<T, int> IndicesOf<T>(this IList<T> values, IEnumerable<T> items) {
            var result = new Dictionary<T, int>();
            var itemList = items.ToSafeList();
            foreach (var item in itemList) {
                result.Add(item, values.IndexOf(item));
            }
            return result;
        }

        /// <summary>
        /// Determines all indices of the provided items in the given collection.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">Collection to locate the items in</param>
        /// <param name="items">The items to locate in the collection</param>
        /// <returns></returns>
        public static Dictionary<T, int> IndicesOf<T>(this T[] values, IEnumerable<T> items) {
            var result = new Dictionary<T, int>();
            var itemList = items.ToSafeList();
            foreach (var item in itemList) {
                result.Add(item, Array.IndexOf(values, item));
            }
            return result;
        }
    }
}
