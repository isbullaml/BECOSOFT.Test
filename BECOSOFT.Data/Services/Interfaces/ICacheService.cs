using BECOSOFT.Utilities.Cache;
using System;

namespace BECOSOFT.Data.Services.Interfaces {
    public interface ICacheService : IBaseService {
        IMemoryCacheWrapper GetCache<TService>() where TService : IBaseService;
        void ClearCache<T>();
        void ClearCache(params Type[] types);
    }
}
