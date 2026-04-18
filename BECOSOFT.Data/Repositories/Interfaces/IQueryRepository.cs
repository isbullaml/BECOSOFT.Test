using BECOSOFT.Data.Caching.Interfaces;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Utilities.Cache;
using System;

namespace BECOSOFT.Data.Repositories.Interfaces {
    public interface IQueryRepository<T> : ICacheable where T : IEntity {
        Type RepositoryType { get; }

        void RefreshCache();
        IMemoryCacheWrapper GetCache();
    }
}