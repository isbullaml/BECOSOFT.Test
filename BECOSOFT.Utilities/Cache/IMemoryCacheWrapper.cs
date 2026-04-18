using System;
using System.Runtime.Caching;

namespace BECOSOFT.Utilities.Cache {
    /// <summary>
    /// Wrapper for a memory-cache
    /// </summary>
    public interface IMemoryCacheWrapper {
        /// <summary>
        /// The name of the cache
        /// </summary>
        string Name { get; }
        /// <summary>
        /// The amount of memory on the computer, in bytes, that can be used by the cache
        /// </summary>
        long CacheMemoryLimitInBytes { get; }
        /// <summary>
        /// The percentage of physical memory that the cache can use
        /// </summary>
        long PhysicalMemoryLimit { get; }
        /// <summary>
        /// The maximum time after which the cache updates its memory statistics
        /// </summary>
        TimeSpan PollingInterval { get; }
        /// <summary>
        /// Defines the <see cref="CacheItemPolicy"/> absolute expiration interval.
        /// </summary>
        long IntervalInMilliseconds { get; set; }
        /// <summary>
        /// The policy for the cache-items, including eviction end expiration details
        /// </summary>
        CacheItemPolicy CacheItemPolicy { get; }
        /// <summary>
        /// Adds or updates an entry in the cache
        /// </summary>
        /// <param name="key">The key in the cache</param>
        /// <param name="value">The value to insert in the cache</param>
        void AddOrUpdate(string key, object value);
        /// <summary>
        /// Retrieve an object from the cache by the specified <see cref="key"/>.
        /// If the cache does not contain an item with the specified <see cref="key"/>, the <see cref="cacheMissAction"/> is executed.
        /// The result of the <see cref="cacheMissAction"/> is then stored in the cache.
        /// </summary>
        /// <typeparam name="TReturn"></typeparam>
        /// <param name="key"></param>
        /// <param name="cacheMissAction"></param>
        /// <returns></returns>
        TReturn Retrieve<TReturn>(string key, Func<TReturn> cacheMissAction);
        /// <summary>
        /// Tries to retrieve a value from the cache as <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key in the cache</param>
        /// <param name="value">The retrieved value</param>
        /// <returns>True if successful, false if not</returns>
        bool TryGetValue<T>(string key, out T value);
        /// <summary>
        /// Tries to retrieve an entry in the cache as an object
        /// </summary>
        /// <param name="key">The key in the cache</param>
        /// <param name="value">The retrieved value</param>
        /// <returns>True if successful, false if not</returns>
        bool TryGetValue(string key, out object value);
        /// <summary>
        /// Tries to remove a value from the cache as <see cref="T"/>
        /// </summary>
        /// <typeparam name="T">The type of the value</typeparam>
        /// <param name="key">The key in the cache</param>
        /// <param name="value">The deleted value</param>
        /// <returns>True if successful, false if not</returns>
        bool TryRemove<T>(string key, out T value);
        /// <summary>
        /// Tries to remove a value from the cache as an object
        /// </summary>
        /// <param name="key">The key of the value</param>
        /// <param name="value">The deleted value</param>
        /// <returns>True if successful, false if not</returns>
        bool TryRemove(string key, out object value);
        /// <summary>
        /// Removes an item from the cache
        /// </summary>
        /// <param name="key">The key to delete from the cache</param>
        void Remove(string key);
        /// <summary>
        /// Checks if a key exist in the cache
        /// </summary>
        /// <param name="key">The key to check in the cache</param>
        /// <returns>True if it exists, false if not</returns>
        bool ContainsKey(string key);
        /// <summary>
        /// The total number of cache entries in the cache.
        /// </summary>
        long Count { get; }
        /// <summary>
        /// Disposes the cache and creates a new cache to use
        /// </summary>
        void ClearCache();
        /// <summary>
        /// Initializes the cache
        /// </summary>
        /// <param name="name">The name for the cache</param>
        void Initialize(string name);

    }
}