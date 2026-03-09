using RA.EntityTypes.Data;
using RA.EntityTypes.Domain;
using Microsoft.EntityFrameworkCore;

namespace RA.EntityTypes.Services
{
    public interface IEntityTypeService<TEntity, TSettings, TContext> 
        where TEntity : BaseEntityType
        where TSettings : BaseEntityTypeSetting
        where TContext : DbContext
    {
        Task<TEntity> GetByIdAsync(Guid id);
        Task<IEnumerable<TEntity>> GetListAsync(List<Guid> entityTypeIds);
        Task<IEnumerable<TEntity>> GetListAsync(Guid entityTypeId);
        Task InsertAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
    }
}