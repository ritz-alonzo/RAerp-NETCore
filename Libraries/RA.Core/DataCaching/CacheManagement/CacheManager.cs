using Microsoft.Extensions.Caching.Memory;
using RA.Core.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RA.Core.DataCaching.CacheManagement
{
    /// <summary>
    /// Prepares cache keys, checks existence, stores, and removes cached
    /// entity lists.
    ///
    /// IMPORTANT LIFETIME NOTE: this class MUST be registered as a
    /// Singleton in DI. If it's registered Scoped/Transient, each request
    /// gets its own SemaphoreSlim instance, which means the stampede-guard
    /// lock below provides NO real mutual exclusion across requests —
    /// concurrent cache misses would all hit the database simultaneously
    /// regardless of the lock code being present.
    /// </summary>
    /// <typeparam name="TEntity">BaseEntity</typeparam>
    public class CacheManager<TEntity> : ICacheManager<TEntity> where TEntity : BaseEntity
    {
        private readonly IMemoryCache _memoryCache;

        // One lock PER CACHE KEY, not one global lock for the whole entity
        // type. This means a miss for entityTypeIds [A,B] doesn't block a
        // concurrent miss for [C,D] — only genuinely identical requests
        // serialize against each other.
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _locks = new();

        public CacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        #region Original Methods

        public IEnumerable<TEntity> GetEntityCacheData(Guid typeId)
        {
            var key = GenerateEntityCacheKey(typeId);
            return _memoryCache.TryGetValue(key, out List<TEntity> entities) ? entities : null;
        }

        public bool EntityCacheNotExists(Guid typeId) => GetEntityCacheData(typeId) == null;

        public string GenerateEntityCacheKey(Guid typeId) =>
            typeof(TEntity).Name + typeId.ToString() + "_List";

        public void ClearCache(Guid typeId)
        {
            var key = GenerateEntityCacheKey(typeId);
            _memoryCache.Remove(key);
        }

        public async Task<IEnumerable<TEntity>> GenerateCacheAsync(IEnumerable<TEntity> entityList, Guid typeId)
        {
            var key = GenerateEntityCacheKey(typeId);
            var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await gate.WaitAsync();
            try
            {
                if (_memoryCache.TryGetValue(key, out List<TEntity> cached))
                    return cached; // another caller already populated it while we waited

                // .ToList() here is NOT optional. It forces immediate execution
                // and materialization RIGHT NOW, regardless of what the caller
                // passed in (IQueryable, deferred LINQ, tracked entities, etc).
                // Whatever goes into _memoryCache.Set is guaranteed to be a
                // plain, already-evaluated List<TEntity> with no live ties
                // back to any DbContext.
                var materialized = entityList.ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                _memoryCache.Set(key, materialized, cacheEntryOptions);
                return materialized;
            }
            finally
            {
                gate.Release();
            }
        }

        #endregion

        #region Overloaded Methods

        #region Entity Types

        public IEnumerable<TEntity> GetEntityCacheData(List<Guid> typeIds)
        {
            var key = GenerateEntityCacheKey(typeIds);
            return _memoryCache.TryGetValue(key, out List<TEntity> entities) ? entities : null;
        }

        public bool EntityCacheNotExists(List<Guid> typeIds) => GetEntityCacheData(typeIds) == null;

        public string GenerateEntityCacheKey(List<Guid> typeIds)
        {
            // Normalized: order-independent and duplicate-independent, so
            // [A,B] and [B,A] (or [A,B,B]) hit the SAME cache entry instead
            // of silently creating separate ones.
            var normalized = typeIds.Distinct().OrderBy(x => x);
            return typeof(TEntity).Name + "-" + string.Join("-", normalized) + "_List";
        }

        public void ClearCache(List<Guid> typeIds)
        {
            var key = GenerateEntityCacheKey(typeIds);
            _memoryCache.Remove(key);
        }

        public async Task<IEnumerable<TEntity>> GenerateCacheAsync(IEnumerable<TEntity> entityList, List<Guid> typeIds)
        {
            var key = GenerateEntityCacheKey(typeIds);
            var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await gate.WaitAsync();
            try
            {
                if (_memoryCache.TryGetValue(key, out List<TEntity> cached))
                    return cached;

                var materialized = entityList.ToList(); // same guarantee as above — non-negotiable

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                _memoryCache.Set(key, materialized, cacheEntryOptions);
                return materialized;
            }
            finally
            {
                gate.Release();
            }
        }

        #endregion

        #region Forms

        public IEnumerable<TEntity> GetEntityCacheData(string systemName)
        {
            var key = GenerateEntityCacheKey(systemName);
            return _memoryCache.TryGetValue(key, out List<TEntity> entities) ? entities : null;
        }

        public bool EntityCacheNotExists(string systemName) => GetEntityCacheData(systemName) == null;

        public string GenerateEntityCacheKey(string systemName) =>
            typeof(TEntity).Name + systemName + "_List";

        public void ClearCache(string systemName)
        {
            var key = GenerateEntityCacheKey(systemName);
            _memoryCache.Remove(key);
        }

        public async Task<IEnumerable<TEntity>> GenerateCacheAsync(IEnumerable<TEntity> entityList, string systemName)
        {
            var key = GenerateEntityCacheKey(systemName);
            var gate = _locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));

            await gate.WaitAsync();
            try
            {
                if (_memoryCache.TryGetValue(key, out List<TEntity> cached))
                    return cached;

                var materialized = entityList.ToList();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                _memoryCache.Set(key, materialized, cacheEntryOptions);
                return materialized;
            }
            finally
            {
                gate.Release();
            }
        }

        #endregion

        #endregion
    }
}