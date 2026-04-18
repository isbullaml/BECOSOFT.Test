using BECOSOFT.Data.Models.Base;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Data.Collections {
    /// <summary>
    /// A list that checks for dirty entities
    /// </summary>
    /// <typeparam name="T">The type of entity</typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class ObserverList<T> : IList<T>, IDirtyList {
        /// <summary>
        /// The type of the observerlist
        /// </summary>
        public Type Type => typeof(T);

        /// <summary>
        /// Value indicating whether the list is dirty itself
        /// </summary>
        public bool IsDirty { get; private set; }

        /// <summary>
        /// Value indicating whether the list contains dirty entities
        /// </summary>
        public bool HasDirty {
            get {
                if (!typeof(T).IsSubclassOf(typeof(BaseEntity))) {
                    return false;
                }
                var hasDirty = false;
                foreach (var item in _items) {
                    var entity = item as BaseEntity;
                    if (entity == null || !entity.IsDirty) { continue; }
                    hasDirty = true;
                    break;
                }
                return hasDirty;
            }
        }

        private readonly List<T> _items;

        public ObserverList() : this(0) {
        }

        public ObserverList(int capacity) {
            _items = new List<T>(capacity);
        }

        public ObserverList(IEnumerable<T> items) {
            _items = new List<T>(items);
        }


        /// <inheritdoc />
        public IEnumerator<T> GetEnumerator() {
            return _items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(T item) {
            _items.Add(item);
            IsDirty = true;
        }

        /// <summary>
        /// Adds a range of items
        /// </summary>
        /// <param name="items">The range to add</param>
        public void AddRange(IEnumerable<T> items) {
            _items.AddRange(items);
            IsDirty = true;
        }

        /// <inheritdoc />
        public void Clear() {
            _items.Clear();
            IsDirty = true;
        }

        /// <inheritdoc />
        public bool Contains(T item) {
            return _items.Contains(item);
        }

        /// <inheritdoc />
        public void CopyTo(T[] array, int arrayIndex) {
            _items.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(T item) {
            var result = _items.Remove(item);
            if (result) { IsDirty = true; }
            return result;
        }


        /// <summary>
        /// Removes all items matching the predicate (IsDirty = true)
        /// </summary>
        /// <param name="predicate">The predicate the items need to match</param>
        /// <returns>The number of removed elements</returns>
        public int RemoveAll(Predicate<T> predicate = null) {
            if (predicate == null) {
                predicate = entity => true;
            }
            var result = _items.RemoveAll(predicate);
            if (result != 0) {
                IsDirty = true;
            }
            return result;
        }

        /// <inheritdoc />
        public int Count => _items.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

        /// <inheritdoc />
        public int IndexOf(T item) {
            return _items.IndexOf(item);
        }

        /// <inheritdoc />
        public void Insert(int index, T item) {
            _items.Insert(index, item);
            IsDirty = true;
        }

        /// <inheritdoc />
        public void RemoveAt(int index) {
            _items.RemoveAt(index);
            IsDirty = true;
        }

        /// <inheritdoc />
        public T this[int index] {
            get { return _items[index]; }
            set {
                _items[index] = value;
                IsDirty = true;
            }
        }

        /// <summary>
        /// Sets the ObserverList to the clean state (IsDirty = false)
        /// </summary>
        public void CleanDirty() {
            IsDirty = false;
        }

        private string DebuggerDisplay => $"{_items.Count} items of type {Type.Name}";
    }

    public interface IDirtyList {
        /// <summary>
        /// Lets the caller know if the list is dirty of clean
        /// </summary>
        bool IsDirty { get; }
    }
}