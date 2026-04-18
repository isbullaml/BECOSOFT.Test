using BECOSOFT.Data.Caching.Interfaces;
using System;

namespace BECOSOFT.Data.Caching {
    [AttributeUsage(AttributeTargets.Field)]
    public class CacheTypeAttribute : Attribute {
        private readonly Type _cacheType;

        public CacheTypeAttribute(Type cacheType) {
            if (typeof(ICacheable).IsAssignableFrom(cacheType)) {
                _cacheType = cacheType;
            }
        }

        public Type GetCacheType() {
            return _cacheType;
        }
    }
}