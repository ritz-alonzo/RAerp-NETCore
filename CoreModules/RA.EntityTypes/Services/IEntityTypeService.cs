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
        Task<TEntity> GetById(Guid id);
        Task<IEnumerable<TEntity>> GetList(List<Guid> entityTypeIds);
        Task<IEnumerable<TEntity>> GetList(Guid entityTypeId);
        Task Insert(TEntity entity);
        Task Update(TEntity entity);
        Task Delete(TEntity entity);
    }
}