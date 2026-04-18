using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BECOSOFT.Utilities.Collections {
    /// <summary>
    /// List of Key-Value pairs
    /// </summary>
    /// <typeparam name="TKey">The type of key</typeparam>
    /// <typeparam name="TValue">The type of value</typeparam>
    public interface IKeyValueList<TKey, TValue> : IList<KeyValuePair<TKey, TValue>> {
        /// <summary>
        /// Adds a key-value pair
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">The value</param>
        void Add(TKey key, TValue value);
        /// <summary>
        /// Adds a range of key-value pairs
        /// </summary>
        /// <param name="items">The range of key-value pairs</param>
        void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items);

        /// <summary>
        /// Defines the list as read-only
        /// </summary>
        /// <returns>The immutable list</returns>
        ReadOnlyCollection<KeyValuePair<TKey, TValue>> AsReadOnly();

        /// <summary>
        /// A list containing the keys in the <see cref="IKeyValueList{TKey,TValue}"/>
        /// </summary>
        List<TKey> Keys { get; }

        /// <summary>
        /// A list containing the values in the <see cref="IKeyValueList{TKey,TValue}"/>
        /// </summary>
        List<TValue> Values { get; }

        /// <summary>
        /// Retrieves the index based on the <see cref="key"/>.
        /// </summary>
        /// <param name="key"></param>
        int IndexOfKey(TKey key);

        /// <summary>
        /// Retrieves a <see cref="KeyValuePair{TKey,TValue}"/> base on an index and removes it afterwards.
        /// </summary>
        /// <param name="index"></param>
        KeyValuePair<TKey, TValue> GetAndRemove(int index);
    }
}