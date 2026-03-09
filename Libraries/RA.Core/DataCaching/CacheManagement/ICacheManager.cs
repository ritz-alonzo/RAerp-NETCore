using RA.Core.Domain;

namespace RA.Core.DataCaching.CacheManagement
{
    public interface ICacheManager<TEntity> where TEntity : BaseEntity
    {
        void ClearCache(Guid typeId);
        void ClearCache(List<Guid> typeIds);
        void ClearCache(string systemName);
        bool EntityCacheNotExists(Guid typeId);
        bool EntityCacheNotExists(List<Guid> typeIds);
        bool EntityCacheNotExists(string systemName);
        Task<IEnumerable<TEntity>> GenerateCacheAsync(IEnumerable<TEntity> entityList, Guid typeId);
        Task<IEnumerable<TEntity>> GenerateCacheAsync(IEnumerable<TEntity> entityList, List<Guid> typeIds);
        Task<IEnumerable<TEntity>> GenerateCacheAsync(IEnumerable<TEntity> entityList, string systemName);
        string GenerateEntityCacheKey(Guid typeId);
        string GenerateEntityCacheKey(List<Guid> typeIds);
        string GenerateEntityCacheKey(string systemName);
        IEnumerable<TEntity> GetEntityCacheData(Guid typeId);
        IEnumerable<TEntity> GetEntityCacheData(List<Guid> typeIds);
        IEnumerable<TEntity> GetEntityCacheData(string systemName);
    }
}