using BECOSOFT.Utilities.Annotations;
using BECOSOFT.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Extensions.Collections {
    /// <summary>
    /// Extensions for the <see cref="IEnumerable{T}"/>-class
    /// </summary>
    public static class EnumerableExtensions {
        /// <summary>
        /// Converts a sequence of <see cref="KeyValuePair{TKey,TValue}"/> to a <see cref="KeyValueList{TKey,TValue}"/>
        /// </summary>
        /// <typeparam name="TKey">Type of the key of the <see cref="KeyValuePair{TKey,TValue}"/></typeparam>
        /// <typeparam name="TValue">Type of the value of the <see cref="KeyValuePair{TKey,TValue}"/></typeparam>
        /// <param name="values">Source sequence</param>
        /// <returns></returns>
        public static KeyValueList<TKey, TValue> ToList<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> values) {
            return new KeyValueList<TKey, TValue>(values);
        }

        /// <summary>
        /// Converts a sequence to a hashset
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence</param>
        /// <param name="comparer">Optional <see cref="IEqualityComparer{T}"/></param>
        /// <returns></returns>
        public static HashSet<T> ToSafeHashSet<T>(this IEnumerable<T> values, IEqualityComparer<T> comparer = null) {
            if (values == null) {
                return new HashSet<T>(comparer);
            }
            return values.ToHashSet(comparer);
        }

        /// <summary>
        /// Determine whether a sequence has elements
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence</param>
        /// <returns></returns>
        [ContractAnnotation("values:null => false")]
        public static bool HasAny<T>(this IEnumerable<T> values) {
            if (values == null) {
                return false;
            }
            if (values is ICollection<T> iCollection) {
                return iCollection.Count > 0;
            }
            if (values is IReadOnlyCollection<T> iReadOnlyCollection) {
                return iReadOnlyCollection.Count > 0;
            }
            return values.Any();
        }

        /// <summary>
        /// Determine whether a sequence is empty
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence</param>
        /// <returns></returns>
        [ContractAnnotation("values:null => true")]
        public static bool IsEmpty<T>(this IEnumerable<T> values) {
            return !values.HasAny();
        }

        /// <summary>
        /// Adds a <see cref="IEnumerable{T}"/> to a <see cref="ISet{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the key</typeparam>
        /// <param name="source">The source set, to which the new data is added</param>
        /// <param name="collection">The collection that will be added</param>
        public static void AddRange<T>(this ISet<T> source, IEnumerable<T> collection) {
            if (collection == null) {
                return;
            }
            foreach (var item in collection) {
                source.Add(item);
            }
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence</param>
        /// <returns>Returns a list based on the source sequence.</returns>
        public static List<T> ToSafeList<T>(this IEnumerable<T> values) {
            return values as List<T> ?? (values?.ToList() ?? new List<T>(0));
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a distinct <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence</param>
        /// <returns>Returns a distinct list based on the source sequence.</returns>
        public static List<T> ToDistinctList<T>(this IEnumerable<T> values) {
            return ToDistinctList(values, null);
        }

        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a distinct <see cref="List{T}"/>
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="values">Source sequence</param>
        /// <param name="comparer">Comparer to use for the distinct operation.</param>
        /// <returns>Returns a distinct list based on the source sequence.</returns>
        public static List<T> ToDistinctList<T>(this IEnumerable<T> values, IEqualityComparer<T> comparer) {
            if (values is List<T> ts) { return ts.Distinct(comparer).ToList(); }
            if (values == null) { return new List<T>();}
            return values.Distinct(comparer).ToList();
        }

        /// <summary>
        /// Reverses the provided <see cref="values"/> collection and returns it as <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="T">Type of the collection</typeparam>
        /// <param name="values">Value collection to reverse</param>
        /// <returns>Returns a <see cref="List{T}"/> containing the reversed <see cref="values"/> collection</returns>
        public static List<T> ToReversedList<T>(this IEnumerable<T> values) {
            var valueList = values.ToSafeList();
            valueList.Reverse();
            return valueList;
        }

        /// <summary>
        /// Divide the <see cref="source"/> collection in to partitions by size. 
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="source">The sequence to partition</param>
        /// <param name="size">the size of the partition</param>
        /// <returns>Returns a sequence of partitions of the defined size."/></returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> source, int size) {
            if (source == null) {
                yield break;
            }
            List<T> list = null;
            var count = 0;
            foreach (var item in source) {
                if (list == null) {
                    list = new List<T>(size);
                }
                list.Add(item);
                count++;
                if (count != size) { continue; }
                yield return new ReadOnlyCollection<T>(list);
                list = null;
                count = 0;
            }
            if (list != null) {
                yield return new ReadOnlyCollection<T>(list);
            }
        }

        /// <summary>
        /// Converts a <see cref="source"/> collection to a <see cref="DataTable"/> with an optional <see cref="tableName"/>.
        /// The <see cref="DataTable.Columns"/> are filled with all public properties of the <see cref="T:T"/>-type.
        /// The <see cref="DataTable.Rows"/> are filled with all the objects from the <see cref="source"/>.
        /// This removes the "IsDirty" and "IsTrackingChanges" properties.
        /// </summary>
        /// <typeparam name="T">Type of the sequence</typeparam>
        /// <param name="source">Source sequence</param>
        /// <param name="tableName">Name of the table</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> source, string tableName = null) {
            var propertiesToIgnore = new List<string> { "IsDirty", "IsTrackingChanges" };
            var result = new DataTable { TableName = tableName };
            var type = typeof(T);
            var typeProperties = type.GetProperties().Where(p => !propertiesToIgnore.Contains(p.Name)).ToList();

            //Create the columns in the DataTable
            foreach (var pi in typeProperties) {
                result.Columns.Add(pi.Name, pi.PropertyType);
            }

            //Populate the table
            foreach (var item in source) {
                var dr = result.NewRow();
                dr.BeginEdit();
                for (var index = 0; index < typeProperties.Count; index++) {
                    var pi = typeProperties[index];
                    dr[index] = pi.GetValue(item, null);
                }
                dr.EndEdit();
                result.Rows.Add(dr);
            }

            return result;
        }

        /// <summary>
        /// Checks if a list contains duplicates
        /// </summary>
        /// <typeparam name="T">The type of list</typeparam>
        /// <param name="values">The list</param>
        /// <param name="comparer">Optional <see cref="IEqualityComparer{T}"/></param>
        /// <returns>Value indicating whether the list contains duplicates</returns>
        public static bool ContainsDuplicates<T>(this IEnumerable<T> values, IEqualityComparer<T> comparer = null) {
            var list = values.ToSafeList();
            if (list.Count <= 1) {
                return false;
            }

            var hashSet = new HashSet<T>(comparer);
            return !list.All(hashSet.Add);
        }

        /// <summary>
        /// Calculates the weighted average of a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the list-items</typeparam>
        /// <param name="values">The list</param>
        /// <param name="valueSelector">The function that selects the value from which the average should be calculated</param>
        /// <param name="weightSelector">The function that selects the weight to use for each value</param>
        /// <returns>The weighted average if the total of al the weights is not 0</returns>
        public static decimal WeightedAverage<T>(this IEnumerable<T> values, Func<T, decimal> valueSelector, Func<T, decimal> weightSelector) {
            var list = values.ToSafeList();
            var weightedValueSum = list.Sum(x => valueSelector(x) * weightSelector(x));
            var weightSum = list.Sum(weightSelector);

            if (Math.Abs(weightSum) > 0.0001m) {
                return weightedValueSum / weightSum;
            }

            return 0;
        }

        /// <summary>
        /// Returns the maximum value in a sequence.
        /// If the sequence is empty, the optional <see cref="defaultValue"/> will be used.
        /// </summary>
        /// <typeparam name="T">The type of the list-items</typeparam>
        /// <typeparam name="TOut">The return type</typeparam>
        /// <param name="values">The list</param>
        /// <param name="selector">The function that selects the value</param>
        /// <param name="defaultValue">Optional default value, uses default(<see cref="TOut"/>) if not provided</param>
        /// <returns>The maximum value (or default value if the list is empty)</returns>
        public static TOut MaxOrDefault<T, TOut>(this IEnumerable<T> values, Func<T, TOut> selector, TOut defaultValue = default(TOut)) {
            var list = values.ToSafeList();
            if (list.IsEmpty()) {
                return defaultValue;
            }

            return list.Select(selector).Max();
        }

        /// <summary>
        /// Returns the maximum value in a sequence.
        /// If the sequence is empty, the optional <see cref="defaultValue"/> will be used.
        /// </summary>
        /// <typeparam name="T">The type of the list-items</typeparam>
        /// <param name="values">The list</param>
        /// <param name="defaultValue">Optional default value, uses default(<see cref="T"/>) if not provided</param>
        /// <returns>The maximum value (or default value if the list is empty)</returns>
        public static T MaxOrDefault<T>(this IEnumerable<T> values, T defaultValue = default(T)) {
            var list = values.ToSafeList();
            if (list.IsEmpty()) {
                return defaultValue;
            }

            return list.Max();
        }

        /// <summary>
        /// Returns the minimum value in a sequence.
        /// If the sequence is empty, the optional <see cref="defaultValue"/> will be used.
        /// </summary>
        /// <typeparam name="T">The type of the list-items</typeparam>
        /// <typeparam name="TOut">The return type</typeparam>
        /// <param name="values">The list</param>
        /// <param name="selector">The function that selects the value</param>
        /// <param name="defaultValue">Optional default value, uses default(<see cref="TOut"/>) if not provided</param>
        /// <returns>The minimum value (or default value if the list is empty)</returns>
        public static TOut MinOrDefault<T, TOut>(this IEnumerable<T> values, Func<T, TOut> selector, TOut defaultValue = default(TOut)) {
            var list = values.ToSafeList();
            if (list.IsEmpty()) {
                return defaultValue;
            }

            return list.Select(selector).Min();
        }

        /// <summary>
        /// Returns the minimum value in a sequence.
        /// If the sequence is empty, the optional <see cref="defaultValue"/> will be used.
        /// </summary>
        /// <typeparam name="T">The type of the list-items</typeparam>
        /// <param name="values">The list</param>
        /// <param name="defaultValue">Optional default value, uses default(<see cref="T"/>) if not provided</param>
        /// <returns>The minimum value (or default value if the list is empty)</returns>
        public static T MinOrDefault<T>(this IEnumerable<T> values, T defaultValue = default(T)) {
            var list = values.ToSafeList();
            if (list.IsEmpty()) {
                return defaultValue;
            }

            return list.Min();
        }

        /// <summary>
        /// Executes an ORDER BY based on the property-name instead of a function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderBy<T>(this IEnumerable<T> values, string propertyName) {
            return OrderByInternal(values, propertyName, "OrderBy");
        }

        /// <summary>
        /// Executes an ORDER BY DESC based on the property-name instead of a function.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static IEnumerable<T> OrderByDescending<T>(this IEnumerable<T> values, string propertyName) {
            return OrderByInternal(values, propertyName, "OrderByDescending");
        }

        private static IEnumerable<T> OrderByInternal<T>(IEnumerable<T> values, string propertyName, string methodName) {
            var objType = typeof(T);
            var properties = objType.GetProperties();
            var matchedProperty = properties.FirstOrDefault(p => p.Name.EqualsIgnoreCase(propertyName));
            if (matchedProperty == null) {
                throw new ArgumentException(nameof(propertyName));
            }

            var paramExpr = Expression.Parameter(objType);
            var propAccess = Expression.PropertyOrField(paramExpr, matchedProperty.Name);
            var expr = Expression.Lambda(propAccess, paramExpr);

            var method = typeof(Enumerable).GetMethods().FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == 2);
            if (method == null) {
                throw new ArgumentException(nameof(method));
            }
            var genericMethod = method.MakeGenericMethod(typeof(T), matchedProperty.PropertyType);
            return (IEnumerable<T>) genericMethod.Invoke(null, new object[] { values, expr.Compile() });
        }

        public static int IndexOf<T>(this IReadOnlyList<T> values, T item) {
            if (values is IList<T> list) {
                return list.IndexOf(item);
            }
            var i = 0;
            foreach (var element in values) {
                if (Equals(element, item)) {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}