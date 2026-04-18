using BECOSOFT.Data.Services.Interfaces;
using BECOSOFT.Utilities.Cache;

namespace BECOSOFT.Data.Extensions {
    public static class BaseServiceExtensions {
        public static IMemoryCacheWrapper GetCache<TService>(this TService service, ICacheService cacheService) where TService : IBaseService {
            return cacheService.GetCache<TService>();
        }
    }
}