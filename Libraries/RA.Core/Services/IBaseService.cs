using RA.Core.Domain;

namespace RA.Core.Services
{
    public interface IBaseService<TEntity> where TEntity : BaseEntity
    {
        Task<TEntity> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> GetListAsync(string cacheKey = null);
        Task<List<TEntity>> ToPagedListAsync(IEnumerable<TEntity> query, int pageNumber, int pageSize);
        Task InsertAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
    }
}