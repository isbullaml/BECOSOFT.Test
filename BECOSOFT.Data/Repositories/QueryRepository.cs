using BECOSOFT.Data.Caching;
using BECOSOFT.Data.Context;
using BECOSOFT.Data.Models.Base;
using BECOSOFT.Data.Repositories.Interfaces;
using System;
using System.Linq.Expressions;
using BECOSOFT.Utilities.Cache;
using System.Threading;

namespace BECOSOFT.Data.Repositories {
    public abstract class QueryRepository<T> : IQueryRepository<T> where T : IEntity {
        private static readonly bool IsCacheableResult = typeof(ICacheableResult).IsAssignableFrom(typeof(T));

        private string _tableName;
        protected string TableName => LazyInitializer.EnsureInitialized(ref _tableName, Entity.GetFullTable<T>);
        protected const string UnionAll = " UNION ALL ";
        protected IMemoryCacheWrapper Cache => GetCache();
        protected static string CacheKey => CacheKeyGenerator.GenerateCacheKey(typeof(T));
        public virtual bool IsCachingPossible { get; } = IsCacheableResult;
        public bool IsCachingEnabled { get; set; }
        protected bool UseCaching => IsCachingPossible && IsCachingEnabled;
        protected IDbContextFactory DbContextFactory;

        public Type RepositoryType => typeof(T);

        protected QueryRepository(IDbContextFactory dbContextFactory) {
            DbContextFactory = dbContextFactory;
        }

        public void RefreshCache() {
            if (!UseCaching) {
                return;
            }
            Cache.ClearCache();
            ReloadCache();
        }

        public IMemoryCacheWrapper GetCache() {
            if (!UseCaching) { throw new InvalidOperationException(Resources.Caching_CachingDisabledException); }
            var connectionString = DbContextFactory.Connection;
            return MemoryCacheHolder.GetCache(connectionString, CacheKey);
        }

        protected virtual void ReloadCache() {
        }

        protected static string GetColumn(Expression<Func<T, object>> prop) {
            return Entity.GetColumn(prop);
        }

        /// <summary>
        /// If the cache is enabled for the repository:
        /// Executes the provided <see cref="func"/> when the cache does not contain an entry for the provided <see cref="keyFunc"/>.
        /// If the cache is disabled (default) for the repository:
        /// Executes the <see cref="func"/> and returns the results.
        /// </summary>
        /// <typeparam name="TU"></typeparam>
        /// <param name="func"></param>
        /// <param name="keyFunc"></param>
        /// <returns>Either the cached results or the function result.</returns>
        protected TU Execute<TU>(Func<TU> func, Func<string> keyFunc) {
            if (UseCaching) {
                var key = keyFunc();
                return Cache.Retrieve(key, func);
            }
            return func();
        }
    }
}
