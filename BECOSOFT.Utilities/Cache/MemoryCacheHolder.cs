using System.Collections.Concurrent;

namespace BECOSOFT.Utilities.Cache {
    public static class MemoryCacheHolder {
        private static readonly ConcurrentDictionary<string, MemoryCacheCollection> Caches = new ConcurrentDictionary<string, MemoryCacheCollection>();

        internal static IMemoryCacheWrapper GetCache(string connectionString, string cacheKey) {
            var collection = Caches.GetOrAdd(connectionString, _ => new MemoryCacheCollection());
            return collection.GetCache(cacheKey);
        }

        internal static bool RemoveCache(string connectionString) {
            return Caches.TryRemove(connectionString, out _);
        }

        public static void ExpireCaches(string connectionString = null) {
            if (connectionString == null) {
                Caches.Clear();
            } else {
                RemoveCache(connectionString);
            }
        }
    }

    internal class MemoryCacheCollection {
        private readonly ConcurrentDictionary<string, IMemoryCacheWrapper> _caches = new ConcurrentDictionary<string, IMemoryCacheWrapper>();

        internal IMemoryCacheWrapper GetCache(string cacheKey) {
            return _caches.GetOrAdd(cacheKey, key => {
                var cache = new MemoryCacheWrapper();
                cache.Initialize(cacheKey);
                return cache;
            });
        }
    }
}