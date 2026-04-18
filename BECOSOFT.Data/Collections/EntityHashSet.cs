using BECOSOFT.Data.Validation.Attributes;
using System.Collections;
using System.Collections.Generic;

namespace BECOSOFT.Data.Collections {
    public class EntityHashSet<T> : IEntityHashSet<T> {
        private readonly HashSet<T> _internalHashSet;

        /// <inheritdoc />
        public int Count => _internalHashSet.Count;
        
        public EntityHashSet() {
            _internalHashSet = new HashSet<T>();
        }
        
        public EntityHashSet(IEqualityComparer<T> comparer) {
            _internalHashSet = new HashSet<T>(comparer);
        }
        
        public EntityHashSet([NotNull] IEnumerable<T> collection) {
            _internalHashSet = new HashSet<T>(collection);
        }
        
        public EntityHashSet([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer) {
            _internalHashSet = new HashSet<T>(collection, comparer);
        }
        
        public EntityHashSet(HashSet<T> internalHashSet) {
            _internalHashSet = internalHashSet ?? new HashSet<T>();
        }
        
        public IEnumerator<T> GetEnumerator() {
            return _internalHashSet.GetEnumerator();
        }
        
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        public bool Contains(T item) {
            return _internalHashSet.Contains(item);
        }

        public void UnionWith(IEnumerable<T> other) {
            _internalHashSet.UnionWith(other);
        }

        public void IntersectWith(IEnumerable<T> other) {
            _internalHashSet.IntersectWith(other);
        }

        public void ExceptWith(IEnumerable<T> other) {
            _internalHashSet.ExceptWith(other);
        }

        public void SymmetricExceptWith(IEnumerable<T> other) {
            _internalHashSet.SymmetricExceptWith(other);
        }

        public bool IsSubsetOf(IEnumerable<T> other) {
            return _internalHashSet.IsSubsetOf(other);
        }

        public bool IsProperSubsetOf(IEnumerable<T> other) {
            return _internalHashSet.IsProperSubsetOf(other);
        }

        public bool IsSupersetOf(IEnumerable<T> other) {
            return _internalHashSet.IsSupersetOf(other);
        }

        public bool IsProperSupersetOf(IEnumerable<T> other) {
            return _internalHashSet.IsProperSupersetOf(other);
        }

        public bool Overlaps(IEnumerable<T> other) {
            return _internalHashSet.Overlaps(other);
        }

        public bool SetEquals(IEnumerable<T> other) {
            return _internalHashSet.SetEquals(other);
        }
    }
    public interface IEntityHashSet<out T> : IReadOnlyCollection<T> {

    }
}
