using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using NLog;

namespace BECOSOFT.Utilities.Cache {
    /// <summary>
    /// Least-Recently Used cache
    /// </summary>
    /// <typeparam name="TKey">Type of the key.</typeparam>
    /// <typeparam name="TValue">Type of the value.</typeparam>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class LRUCache<TKey, TValue> : IDisposable {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        public delegate void ExpiredCacheEntryHandler(TKey key, TValue value);
        public event ExpiredCacheEntryHandler OnExpiredCacheEntry;
        private readonly TimeSpan _timeToLive;
        private readonly Timer _timer;
        private readonly Dictionary<TKey, CacheNode> _entries;
        private CacheNode _head;
        private CacheNode _tail;
        private int _count;

        /// <summary>
        /// Construct a new <see cref="LRUCache{TKey,TValue}"/> object with a fixed <see cref="capacity"/> and optionally a <see cref="timeToLive"/>
        /// </summary>
        /// <param name="capacity">Capacity of the cache.</param>
        /// <param name="timeToLive">
        /// Maximum time that an entry will live in the cache.
        /// If <see cref="timeToLive"/> and <see cref="interval"/> are greater than <see cref="TimeSpan.Zero"/>, a <see cref="Timer"/> will automatically purge the expired cache items.
        /// </param>
        /// <param name="interval">
        /// Interval between cache refreshes. 
        /// If <see cref="timeToLive"/> and <see cref="interval"/> are greater than <see cref="TimeSpan.Zero"/>, a <see cref="Timer"/> will automatically purge the expired cache items.
        /// </param>
        public LRUCache(int capacity,
                        TimeSpan timeToLive = default,
                        TimeSpan interval = default) {
            Capacity = capacity;
            _timeToLive = timeToLive;
            _entries = new Dictionary<TKey, CacheNode>(Capacity);
            if (timeToLive > TimeSpan.Zero && interval > TimeSpan.Zero) {
                _timer = new Timer(Purge, null,
                                   dueTime: timeToLive,
                                   period: interval);
                Logger.Debug("Created '{0}' with capacity {1}, TTL {2} and Refresh interval {3}", nameof(LRUCache<TKey, TValue>), capacity, timeToLive, interval);
            }else{
                Logger.Debug("Created '{0}' with capacity {1}, TTL {2}", nameof(LRUCache<TKey, TValue>), capacity, timeToLive);
            }
        }

        /// <summary>
        /// Gets the urrent number of entries.
        /// </summary>
        public int Count => _entries.Count;
        /// <summary>
        /// Gets the maximum number of entries in the cache.
        /// </summary>
        public int Capacity { get; }
        /// <summary>
        /// Gets whether the cache is full.
        /// </summary>
        public bool IsFull => Count == Capacity;

        /// <summary>
        /// Gets whether the cache is empty.
        /// </summary>
        public bool IsEmpty => Count == 0;

        /// <summary>
        /// Gets the value associated with the specified <see cref="key"/>.
        /// If the entry is expired, the default value for the type of the <paramref name="value"/> parameter is returned and the entry is removed.
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found and the entry is not expired; otherwise, the default value for the type of the <paramref name="value" /> parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true" /> if the <see cref="LRUCache{TKey,TValue}" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        public bool TryGetValue(TKey key, out TValue value) {
            value = default;
            if (!_entries.TryGetValue(key, out var entry)) {
                return false;
            }
            lock (this) {
                if (entry.IsExpired(_timeToLive)) {
                    Remove(entry);
                    HandleExpiredCacheEntry(entry);
                    return false;
                }
            }
            MoveToHead(entry);
            lock (entry) {
                value = entry.Value;
                entry.LastAccessed = DateTime.UtcNow;
            }
            return true;
        }

        /// <summary>
        /// Adds the specified key and value to the dictionary.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be <see langword="null" /> for reference types.</param>
        public void Add(TKey key, TValue value) {
            TryAdd(key, value);
        }

        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="LRUCache{TKey,TValue}" />.
        /// </summary>
        /// <param name="key">The key of the element to add.</param>
        /// <param name="value">The value of the element to add. The value can be <see langword="null" /> for reference types.</param>
        /// <returns><see langword="true" /> if the <see cref="LRUCache{TKey,TValue}" /> contains an element with the specified key; otherwise, <see langword="false" />.</returns>
        public bool TryAdd(TKey key, TValue value) {
            if (!_entries.TryGetValue(key, out var entry)) {
                lock (this) {
                    if (!_entries.TryGetValue(key, out entry)) {
                        if (IsFull) {
                            entry = _tail;
                            _entries.Remove(_tail.Key);
                            HandleExpiredCacheEntry(entry);
                            entry.Key = key;
                            entry.Value = value;
                            entry.Creation = DateTime.UtcNow;
                            entry.LastAccessed = DateTime.UtcNow;
                        } else {
                            _count++;
                            entry = new CacheNode {
                                Key = key,
                                Value = value,
                                Creation = DateTime.UtcNow,
                                LastAccessed = DateTime.UtcNow,
                            };
                        }
                        _entries.Add(key, entry);
                    }
                }
            } else {
                lock (entry) {
                    entry.Value = value;
                }
            }
            MoveToHead(entry);
            if (_tail == null) {
                _tail = _head;
            }
            return true;
        }

        /// <summary>Removes the value with the specified key from the <see cref="LRUCache{TKey,TValue}" />.</summary>
        /// <param name="key">The key of the element to remove.</param>
        /// <returns>
        /// <see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.  This method returns <see langword="false" /> if <paramref name="key" /> is not found in the <see cref="LRUCache{TKey,TValue}" />.
        /// </returns>
        public bool Remove(TKey key) {
            return TryRemove(key, out _);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="LRUCache{TKey,TValue}" />.
        /// </summary>
        /// <param name="key">The key of the element to remove and return.</param>
        /// <param name="value">When this method returns, contains the object removed from the <see cref="LRUCache{TKey,TValue}" />, or the default value of  the <see langword="TValue" /> type if <paramref name="key" /> does not exist.</param>
        /// <returns>
        /// <see langword="true" /> if the object was removed successfully; otherwise, <see langword="false" />.
        /// </returns>
        public bool TryRemove(TKey key, out TValue value) {
            value = default;
            if (!_entries.TryGetValue(key, out var entry)) {
                return false;
            }
            lock (entry) {
                value = entry.Value;
            }
            Remove(entry);
            HandleExpiredCacheEntry(entry);
            return true;
        }

        /// <summary>
        /// Removes all keys and values from the <see cref="LRUCache{TKey,TValue}" />.
        /// </summary>
        public void Clear() {
            lock (this) {
                _entries.Clear();
                _head = null;
                _tail = null;
            }
        }

        /// <summary>
        /// Moves the <see cref="entry"/> to the head of the cache.
        /// </summary>
        /// <param name="entry">Entry to move to head.</param>
        private void MoveToHead(CacheNode entry) {
            if (entry == _head) { return; }
            lock (this) {
                RemoveFromLinkedList(entry);
                AddToHead(entry);
            }
        }

        /// <summary>
        /// Purges all expired entries from the cache.
        /// </summary>
        public void PurgeExpired() {
            if (_timeToLive <= TimeSpan.Zero || _count == 0) { return; }
            lock (this) {
                Logger.Debug("Purging expired, original count: {0}", _count);
                var current = _tail;
                var now = DateTime.UtcNow;

                while (current != null && current.IsExpired(_timeToLive, now)) {
                    Remove(current);
                    HandleExpiredCacheEntry(current);
                    current = current.Previous;
                }
                Logger.Debug("Purged expired, current count: {0}", _count);
            }
        }

        /// <summary>
        /// Purges all expired cached items
        /// </summary>
        /// <param name="state"></param>
        private void Purge(object state) {
            PurgeExpired();
        }

        /// <summary>
        /// Makes <see cref="entry"/> the head of the cache.
        /// </summary>
        /// <param name="entry"></param>
        private void AddToHead(CacheNode entry) {
            entry.Previous = null;
            entry.Next = _head;
            if (_head != null) {
                _head.Previous = entry;
            }
            _head = entry;
        }

        /// <summary>
        /// Relinks the <see cref="CacheNode.Next"/> and <see cref="CacheNode.Previous"/> properties on the <see cref="entry"/>.
        /// </summary>
        /// <param name="entry"></param>
        private void RemoveFromLinkedList(CacheNode entry) {
            var next = entry.Next;
            var prev = entry.Previous;

            if (next != null) {
                next.Previous = entry.Previous;
            }
            if (prev != null) {
                prev.Next = entry.Next;
            }
            if (entry == _head) {
                _head = next;
            }
            if (entry == _tail) {
                _tail = prev;
            }
        }

        /// <summary>
        /// Removes the <see cref="entry"/> from the cache and relinks the <see cref="CacheNode.Next"/> and <see cref="CacheNode.Previous"/> properties on the <see cref="entry"/>.
        /// </summary>
        /// <param name="entry"></param>
        private void Remove(CacheNode entry) {
            RemoveFromLinkedList(entry);
            _entries.Remove(entry.Key);
            _count--;
        }

        /// <summary>
        /// Retrieve all keys in the cache.
        /// </summary>
        /// <returns></returns>
        public List<TKey> CachedKeys() {
            lock (this) {
                return _entries.Keys.ToList();
            }
        }

        private void HandleExpiredCacheEntry(CacheNode entry) {
            Logger.Debug("Cache entry with key '{0:yyyy/MM/dd HH:mm:ss.ffffff}' expired on {1}. Was created on '{2:yyyy/MM/dd HH:mm:ss.ffffff}' and was accessed {3} times.", entry.Key.ToString(), entry.GetExpirationTime(_timeToLive), entry.Creation, entry.AccessCount);
            OnExpiredCacheEntry?.Invoke(entry.Key, entry.Value);
        }

        public void Dispose() {
            _timer?.Dispose();
        }
        

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        [DebuggerHidden]
        private string DebuggerDisplay => $"Capacity: {Capacity}, Count: {Count}, Auto clean? {_timeToLive != TimeSpan.Zero}";

        [DebuggerDisplay("{DebuggerDisplay,nq}")]
        private class CacheNode {
            private DateTime _creation;
            private DateTime _lastAccessed;
            public CacheNode Next { get; set; }
            public CacheNode Previous { get; set; }
            public TKey Key { get; set; }
            public TValue Value { get; set; }
            public int AccessCount { get; private set; }

            public DateTime LastAccessed {
                get => _lastAccessed;
                set {
                    _lastAccessed = value;
                    AccessCount += 1;
                }
            }

            public DateTime Creation {
                get => _creation;
                set {
                    _creation = value;
                    AccessCount = 0;
                }
            }

            public bool IsExpired(TimeSpan timeToLive, DateTime? utcNow = null) => timeToLive > TimeSpan.Zero && (utcNow ?? DateTime.UtcNow) - LastAccessed > timeToLive;
            public DateTime GetExpirationTime(TimeSpan timeToLive) => Creation.Add(timeToLive);
            

            [DebuggerBrowsable(DebuggerBrowsableState.Never)]
            [DebuggerHidden]
            private string DebuggerDisplay => $"K: {Key}, V: {Value}, Accessed: {AccessCount}x, Last accessed: {LastAccessed:yyyy/MM/dd HH:mm:ss.ffffff}, Created: {Creation:yyyy/MM/dd HH:mm:ss.ffffff}";
        }
    }

    // Inspiration / Code: https://github.com/tejacques/LRUCache
}