using Microsoft.Extensions.Caching.Memory;
using RA.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.DataCaching.CacheManagement
{
    /// <summary>
    /// This class 
    /// will prepare cache key
    /// will generate cache key
    /// will check if cached data exists
    /// will set cache data
    /// will remove cache data
    /// </summary>
    /// <typeparam name="TEntity">BaseEntity</typeparam>
    public class CacheManager<TEntity> : ICacheManager<TEntity> where TEntity : BaseEntity
    {
        private readonly IMemoryCache _memoryCache;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public CacheManager(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        #region Original Methods

        public IEnumerable<TEntity> GetEntityCacheData(Guid typeId)
        {
            IEnumerable<TEntity> entityList = null;
            var key = GenerateEntityCacheKey(typeId);

            if (_memoryCache.TryGetValue(key, out IEnumerable<TEntity> entities))
            {
                entityList = entities;
            }

            return entityList;
        }

        public bool EntityCacheNotExists(Guid typeId)
        {
            return GetEntityCacheData(typeId) == null;
        }
        
        public string GenerateEntityCacheKey(Guid typeId)
        {
            return typeof(TEntity).Name + typeId.ToString() + "_List";
        }

        // after every post of entity
        // clear cache
        public void ClearCache(Guid typeId)
        {
            var key = GenerateEntityCacheKey(typeId);
            if (key != null)
                _memoryCache.Remove(key);
        }

        public IEnumerable<TEntity> GenerateCache(IEnumerable<TEntity> entityList, Guid typeId)
        {
            try
            {
                semaphore.Wait();

                if (EntityCacheNotExists(typeId))
                {
                    var key = GenerateEntityCacheKey(typeId);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                    _memoryCache.Set(key, entityList, cacheEntryOptions);

                    return entityList;
                }
                else
                {
                    return GetEntityCacheData(typeId);
                }

            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Overloaded Methods

        #region Entity Types

        public IEnumerable<TEntity> GetEntityCacheData(List<Guid> typeIds)
        {
            IEnumerable<TEntity> entityList = null;
            var key = GenerateEntityCacheKey(typeIds);

            if (_memoryCache.TryGetValue(key, out IEnumerable<TEntity> entities))
            {
                entityList = entities;
            }

            return entityList;
        }

        public bool EntityCacheNotExists(List<Guid> typeIds)
        {
            return GetEntityCacheData(typeIds) == null;
        }

        public string GenerateEntityCacheKey(List<Guid> typeIds)
        {
            var entityName = typeof(TEntity).Name;
            foreach (var typeId in typeIds)
            {
                entityName = entityName + "-" + typeId.ToString();
            }
            entityName = entityName + "_List";
            return entityName;
        }

        public void ClearCache(List<Guid> typeIds)
        {
            var key = GenerateEntityCacheKey(typeIds);
            if (key != null)
                _memoryCache.Remove(key);
        }

        public IEnumerable<TEntity> GenerateCache(IEnumerable<TEntity> entityList, List<Guid> typeIds)
        {
            try
            {
                semaphore.Wait();

                if (EntityCacheNotExists(typeIds))
                {
                    var key = GenerateEntityCacheKey(typeIds);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                    _memoryCache.Set(key, entityList, cacheEntryOptions);

                    return entityList;
                }
                else
                {
                    return GetEntityCacheData(typeIds);
                }

            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #region Forms

        public IEnumerable<TEntity> GetEntityCacheData(string systemName)
        {
            IEnumerable<TEntity> entityList = null;
            var key = GenerateEntityCacheKey(systemName);

            if (_memoryCache.TryGetValue(key, out IEnumerable<TEntity> entities))
            {
                entityList = entities;
            }

            return entityList;
        }

        public bool EntityCacheNotExists(string systemName)
        {
            return GetEntityCacheData(systemName) == null;
        }

        public string GenerateEntityCacheKey(string systemName)
        {
            return typeof(TEntity).Name + systemName + "_List";
        }

        // after every post of entity
        // clear cache
        public void ClearCache(string systemName)
        {
            var key = GenerateEntityCacheKey(systemName);
            if (key != null)
                _memoryCache.Remove(key);
        }

        public IEnumerable<TEntity> GenerateCache(IEnumerable<TEntity> entityList, string systemName)
        {
            try
            {
                semaphore.Wait();

                if (EntityCacheNotExists(systemName))
                {
                    var key = GenerateEntityCacheKey(systemName);

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromSeconds(60))
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(3600))
                    .SetPriority(CacheItemPriority.Normal)
                    .SetSize(1024);

                    _memoryCache.Set(key, entityList, cacheEntryOptions);

                    return entityList;
                }
                else
                {
                    return GetEntityCacheData(systemName);
                }

            }
            finally
            {
                semaphore.Release();
            }
        }

        #endregion

        #endregion
    }
}
