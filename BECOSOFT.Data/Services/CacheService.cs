using BECOSOFT.Data.Caching;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Cache;
using System;

namespace BECOSOFT.Data.Services {
    public class CacheService : ICacheService {
        private readonly IDbContextFactory _dbContextFactory;

        public CacheService(IDbContextFactory dbContextFactory) {
            _dbContextFactory = dbContextFactory;
        }
        
        public IMemoryCacheWrapper GetCache<TService>() where TService : IBaseService {
            var connectionString = _dbContextFactory.Connection ?? "";
            var cacheKey = CacheKeyGenerator.GenerateCacheKey<TService>();
            return MemoryCacheHolder.GetCache(connectionString, cacheKey);
        }

        public void ClearCache<T>() {
            ClearCache(typeof(T));
        }

        public void ClearCache(params Type[] types) {
            foreach (var type in types) {
                var connectionString = _dbContextFactory.Connection ?? "";
                var cacheKey = CacheKeyGenerator.GenerateCacheKey(type);
                var cache = MemoryCacheHolder.GetCache(connectionString, cacheKey);
                cache.ClearCache();
            }
        }
    }
}