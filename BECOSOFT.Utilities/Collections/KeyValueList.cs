using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BECOSOFT.Utilities.Collections {
    /// <inheritdoc cref="IKeyValueList{TKey,TValue}" />
    public class KeyValueList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>, IKeyValueList<TKey, TValue> {
        public KeyValueList() {
        }

        public KeyValueList(int capacity) : base(capacity) {
        }

        public KeyValueList(IEnumerable<KeyValuePair<TKey, TValue>> items) {
            AddRange(items);
        }

        /// <inheritdoc />
        public void Add(TKey key, TValue value) {
            Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <inheritdoc />
        public List<TKey> Keys {
            get {
                var result = new List<TKey>(Count);
                for (var i = 0; i < Count; i++) {
                    result.Add(this[i].Key);
                }
                return result;
            }
        }

        /// <inheritdoc />
        public List<TValue> Values {
            get {
                var result = new List<TValue>(Count);
                for (var i = 0; i < Count; i++) {
                    result.Add(this[i].Value);
                }
                return result;
            }
        }

        public int IndexOfKey(TKey key) {
            for (var i = 0; i < Count; i++) {
                if (ReferenceEquals(this[i].Key, key)) {
                    return i;
                }
            }

            return -1;
        }

        public KeyValuePair<TKey, TValue> GetAndRemove(int index) {
            var returnValue = this[index];
            RemoveAt(index);
            return returnValue;
        }

        public Dictionary<TKey, TValue> ToDictionary() => this.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }
}