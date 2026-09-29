using RA.Core.Domain;
using RA.WebFramework.Models.Pagination;
using RAerp.Domain.EntityAttributes;

namespace RAerp.Services.EntityAttributeServices
{
    public interface IEntityAttributeService
    {
        Task DeleteEntityAttributeAsync(EntityAttribute attribute);
        Task DeleteEntityAttributeOptionAsync(EntityAttributeOption option);
        Task DeleteEntityAttributeValueAsync(EntityAttributeValue value);
        Task<EntityAttribute> GetEntityAttributeByAttributeNameAsync(string attributeName);
        Task<EntityAttribute> GetEntityAttributeByIdAsync(Guid id);
        Task<IEnumerable<EntityAttribute>> GetEntityAttributeListAsync(string searchAttributeName = null, string searchSystemName = null, List<int> controlTypeIds = null, bool showActiveOnly = false);
        Task<IEnumerable<EntityAttribute>> GetEntityAttributeListOfEntityAsync(string systemName);
        Task<IEnumerable<EntityAttributeOption>> GetEntityAttributeOptionListAsync(Guid? attributeId = null, string searchOptionName = null, string searchOptionValue = null);
        Task<IEnumerable<EntityAttributeOption>> GetEntityAttributeOptionListByAttributeIdAsync(Guid attributeId);
        Task<PagedResult<EntityAttributeOption>> GetEntityAttributeOptionPagedResultListAsync(Guid? attributeId = null, string searchOptionName = null, string searchOptionValue = null, int? pageNumber = 0, int? pageSize = 0);
        Task<PagedResult<EntityAttribute>> GetEntityAttributePagedResultListAsync(string searchAttributeName = null, string searchSystemName = null, List<int> controlTypeIds = null, bool showActiveOnly = false, int? pageNumber = 0, int? pageSize = int.MaxValue);
        Task<EntityAttributeValue> GetEntityAttributeValueByAttributeIdAndEntityIdAsync(Guid attributeId, Guid entityId);
        Task<EntityAttributeValue> GetEntityAttributeValueByAttributeIdAsync(Guid attributeId);
        Task<IEnumerable<EntityAttributeValue>> GetEntityAttributeValueListAsync(Guid? attributeId = null, Guid? entityId = null);
        Task<PagedResult<EntityAttributeValue>> GetEntityAttributeValuePagedResultListAsync(Guid? attributeId = null, Guid? entityId = null, int? pageNumber = 0, int? pageSize = int.MaxValue);
        Task InsertEntityAttributeAsync(EntityAttribute attribute);
        Task InsertEntityAttributeOptionAsync(EntityAttributeOption option);
        Task InsertEntityAttributeValueAsync(EntityAttributeValue value);
        Task UpdateEntityAttributeAsync(EntityAttribute attribute);
        Task UpdateEntityAttributeOptionAsync(EntityAttributeOption option);
        Task UpdateEntityAttributeValueAsync(EntityAttributeValue value);
        Task<List<EntityAttributeValue>> MapEntityAttributeValuesAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : BaseEntity;
        Task InsertOrUpdateEntityAttributeValuesMappingAsync(List<EntityAttributeValue> values, Guid entityId);
    }
}