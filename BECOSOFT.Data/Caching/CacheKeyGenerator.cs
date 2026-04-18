using BECOSOFT.Data.Services.Interfaces;
using System;

namespace BECOSOFT.Data.Caching {
    public static class CacheKeyGenerator {
        public static string GenerateCacheKey(Type type) {
            return type.ToString();
        }

        public static string GenerateCacheKey<TService>() where TService : IBaseService {
            return $"service:{typeof(TService)}";
        }
    }
}