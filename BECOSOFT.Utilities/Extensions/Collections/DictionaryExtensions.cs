using BECOSOFT.Utilities.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Extensions.Collections {
    /// <summary>
    /// Extensions for the <see cref="IDictionary{TKey,TValue}"/>-class
    /// </summary>
    public static class DictionaryExtensions {
        /// <summary>
        /// Retrieve a value from the provided <see cref="IDictionary{TKey,TValue}"/> by the provided key, with a fallback <see cref="defaultValue"/> if the key doesn't exist in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="values">Source dictionary</param>
        /// <param name="key">The key to find the value for</param>
        /// <param name="defaultValue">Default value, used if the key is not present in the <see cref="values"/> sequence.</param>
        /// <returns></returns>
        public static TValue TryGetValueWithDefault<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> values, TKey key, TValue defaultValue = default) {
            TValue returnValue;
            if (values is IDictionary<TKey, TValue> dict) {
                return dict.TryGetValue(key, out returnValue) ? returnValue : defaultValue;
            }
            if (values is IReadOnlyDictionary<TKey, TValue> readonlyDict) {
                return readonlyDict.TryGetValue(key, out returnValue) ? returnValue : defaultValue;
            }
            foreach (var pair in values) {
                if (key.Equals(pair.Key)) {
                    return pair.Value;
                }
            }
            return defaultValue;
        }
        /// <summary>
        /// Retrieve a value from the provided <see cref="IDictionary{TKey,TValue}"/> by the provided key, with a fallback <see cref="defaultValue"/> if the key doesn't exist in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="values">Source dictionary</param>
        /// <param name="key">The key to find the value for</param>
        /// <param name="defaultValue">Default value, used if the key is not present in the <see cref="values"/> sequence.</param>
        /// <returns></returns>
        public static TValue TryGetValueWithDefault<TKey, TValue>(this IDictionary<TKey, TValue> values, TKey key, TValue defaultValue = default) {
            return values.TryGetValue(key, out var returnValue) ? returnValue : defaultValue;
        }
        /// <summary>
        /// Retrieve a value from the provided <see cref="IDictionary{TKey,TValue}"/> by the provided key, with a fallback <see cref="defaultValueFunc"/> if the key doesn't exist in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="values">Source dictionary</param>
        /// <param name="key">The key to find the value for</param>
        /// <param name="defaultValueFunc">Default value function, called if the key is not present in the <see cref="values"/> sequence.</param>
        /// <returns></returns>
        public static TValue TryGetValueWithDefaultFunc<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> values, TKey key, Func<TValue> defaultValueFunc) {
            if (values is IDictionary<TKey, TValue> dict && dict.TryGetValue(key, out var returnValue)) {
                return returnValue;
            }
            if (values is IReadOnlyDictionary<TKey, TValue> readonlyDict && readonlyDict.TryGetValue(key, out returnValue)) {
                return returnValue;
            }
            foreach (var pair in values) {
                if (key.Equals(pair.Key)) {
                    return pair.Value;
                }
            }
            return defaultValueFunc();
        }
        /// <summary>
        /// Retrieve a value from the provided <see cref="IDictionary{TKey,TValue}"/> by the provided key, with a fallback <see cref="defaultValueFunc"/> if the key doesn't exist in the dictionary.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="values">Source dictionary</param>
        /// <param name="key">The key to find the value for</param>
        /// <param name="defaultValueFunc">Default value function, called if the key is not present in the <see cref="values"/> sequence.</param>
        /// <returns></returns>
        public static TValue TryGetValueWithDefaultFunc<TKey, TValue>(this IDictionary<TKey, TValue> values, TKey key, Func<TValue> defaultValueFunc) {
            if (values.TryGetValue(key, out var returnValue)) {
                return returnValue;
            }
            return defaultValueFunc();
        }

        /// <summary>
        /// Adds a <see cref="IDictionary{TKey,TValue}"/> to a <see cref="IDictionary{TKey,TValue}"/>. If the key already exists and <paramref name="overwrite"/> = <see langword="true"/>, the existing value is overwritten.
        /// </summary>
        /// <typeparam name="T">The type of the key</typeparam>
        /// <typeparam name="TS">The type of the value</typeparam>
        /// <param name="source">The source dictionary, to which the new data is added</param>
        /// <param name="collection">The dictionary that will be added</param>
        /// <param name="overwrite"></param>
        public static void AddRange<T, TS>(this IDictionary<T, TS> source, IDictionary<T, TS> collection, bool overwrite = true) {
            if (collection == null) {
                return;
            }

            foreach (var item in collection) {
                if (!overwrite && source.ContainsKey(item.Key)) { continue; }
                source[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Adds a <see cref="IEnumerable{T}"/> to a <see cref="IDictionary{TKey,TValue}"/>. If the key already exists and <paramref name="overwrite"/> = <see langword="true"/>, the existing value is overwritten.
        /// </summary>
        /// <typeparam name="T">The type of the key</typeparam>
        /// <typeparam name="TS">The type of the value</typeparam>
        /// <param name="source">The source dictionary, to which the new data is added</param>
        /// <param name="collection">The collection of <see cref="KeyValuePair{TKey,TValue}"/> to add to <param name="source"></param></param>
        /// <param name="overwrite">Indicates whether to overwrite the value when a <see cref="KeyValuePair{TKey,TValue}"/> already exists.</param>
        public static void AddRange<T, TS>(this IDictionary<T, TS> source, IEnumerable<KeyValuePair<T, TS>> collection, bool overwrite = true) {
            if (collection == null) {
                return;
            }

            foreach (var item in collection) {
                if (!overwrite && source.ContainsKey(item.Key)) { continue; }
                source[item.Key] = item.Value;
            }
        }

        /// <summary>
        /// Adds a <see cref="TKey"/> with a <see cref="TValue"/> to a <see cref="IDictionary{TKey,TValue}"/>. If the key already exists and <paramref name="overwrite"/> = <see langword="true"/>, the existing value is overwritten.
        /// </summary>
        /// <typeparam name="T">The type of the key</typeparam>
        /// <typeparam name="TS">The type of the value</typeparam>
        /// <param name="source">The source dictionary, to which the new data is added</param>
        /// <param name="key">The key that will be added</param>
        /// <param name="value">The value that will be added</param>
        /// <param name="overwrite"></param>
        public static void AddOrUpdate<T, TS>(this IDictionary<T, TS> source, T key, TS value, bool overwrite = true) {
            if (!overwrite && source.ContainsKey(key)) { return; }
            source[key] = value;
        }

        /// <summary>
        /// Returns the <see cref="Dictionary{TKey,TValue}.Values"/> of a <see cref="IDictionary{TKey,TValue}"/> as a <see cref="List{T}"/>.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TValue">Dictionary value type</typeparam>
        /// <param name="dictionary"></param>
        /// <returns>Returns the <see cref="Dictionary{TKey,TValue}.Values"/> of a <see cref="IDictionary{TKey,TValue}"/> as a <see cref="List{T}"/>.</returns>
        public static List<TValue> GetValueList<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) {
            return dictionary.Values.ToList();
        }


        /// <summary>
        /// Returns the <see cref="Dictionary{TKey,TValue}.Values"/> of a <see cref="IDictionary{TKey,TValue}"/> as an <see cref="T:T[]"/>.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type</typeparam>
        /// <typeparam name="TValue">Dictionary value type</typeparam>
        /// <param name="dictionary"></param>
        /// <returns>Returns the <see cref="Dictionary{TKey,TValue}.Values"/> of a <see cref="IDictionary{TKey,TValue}"/> as an <see cref="T:T[]"/>.</returns>
        public static TValue[] GetValueArray<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) {
            return dictionary.Values.ToArray();
        }

        /// <summary>
        /// Creates a copy of the provided <see cref="IDictionary{TKey,TValue}"/>. Note: This action does not copy the keys and the values.
        /// </summary>
        /// <typeparam name="TKey">Type of the key</typeparam>
        /// <typeparam name="TValue">Type of the value</typeparam>
        /// <param name="dictionary">Dictionary to copy</param>
        /// <returns>A copy of the provided <see cref="Dictionary{TKey,TValue}"/></returns>
        public static Dictionary<TKey, TValue> Copy<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) {
            if (dictionary.IsEmpty()) {
                return new Dictionary<TKey, TValue>();
            }
            var result = new Dictionary<TKey, TValue>(dictionary);
            return result;
        }

        /// <summary>
        /// Converts a <see cref="IDictionary{TKey,TValue}"/> to a <see cref="KeyValueList{TKey,TValue}"/>
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dictionary"></param>
        /// <returns></returns>
        public static KeyValueList<TKey, TValue> ToKeyValueList<TKey, TValue>(this IDictionary<TKey, TValue> dictionary) {
            return new KeyValueList<TKey, TValue>(dictionary);
        }
    }
}