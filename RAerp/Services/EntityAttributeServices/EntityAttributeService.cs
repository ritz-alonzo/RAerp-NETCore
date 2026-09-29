using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RA.Core.Domain;
using RA.WebFramework.Extensions;
using RA.WebFramework.Models.Pagination;
using RAerp.App_Data;
using RAerp.Domain.EntityAttributes;

namespace RAerp.Services.EntityAttributeServices
{
    public class EntityAttributeService : IEntityAttributeService
    {
        #region Constants
        private readonly RAerpContext _context;
        private readonly ICacheManager<EntityAttribute> _entityAttributeCacheManager;
        private readonly ICacheManager<EntityAttributeOption> _entityAttributeOptionCacheManager;
        private readonly ICacheManager<EntityAttributeValue> _entityAttributeValueCacheManager;

        public EntityAttributeService(RAerpContext context,
            ICacheManager<EntityAttribute> entityAttributeCacheManager,
            ICacheManager<EntityAttributeOption> entityAttributeOptionCacheManager,
            ICacheManager<EntityAttributeValue> entityAttributeValueCacheManager)
        {
            _context = context;
            _entityAttributeCacheManager = entityAttributeCacheManager;
            _entityAttributeOptionCacheManager = entityAttributeOptionCacheManager;
            _entityAttributeValueCacheManager = entityAttributeValueCacheManager;
        }
        #endregion

        #region Entity Attribute
        public async Task<EntityAttribute> GetEntityAttributeByIdAsync(Guid id)
        {
            return await _context.EntityAttribute.FirstOrDefaultAsync(c => c.Id == id && c.IsActive == true);
        }

        public async Task<EntityAttribute> GetEntityAttributeByAttributeNameAsync(string attributeName)
        {
            return await _context.EntityAttribute.FirstOrDefaultAsync(c => c.AttributeName.ToLower() == attributeName.ToLower());
        }

        // List of All Entity Attributes
        public async Task<IEnumerable<EntityAttribute>> GetEntityAttributeListAsync(string searchAttributeName = null,
            string searchSystemName = null,
            List<int> controlTypeIds = null,
            bool showActiveOnly = false)
        {
            var query = new List<EntityAttribute>().AsEnumerable();

            string cacheKey = typeof(EntityAttribute).FullName;
            if (!string.IsNullOrEmpty(searchSystemName))
            {
                cacheKey = cacheKey + '-' + searchSystemName;
                if (_entityAttributeCacheManager.EntityCacheNotExists(cacheKey))
                {
                    query = await _context.EntityAttribute.Where(c => c.SystemName.ToLower() == searchSystemName.ToLower()).AsNoTracking().ToListAsync();
                    await _entityAttributeCacheManager.GenerateCacheAsync(query, cacheKey);
                }
                else
                {
                    query = _entityAttributeCacheManager.GetEntityCacheData(cacheKey);
                }
            }
            else
            {
                if (_entityAttributeCacheManager.EntityCacheNotExists(cacheKey))
                {
                    query = await _context.EntityAttribute.AsNoTracking().ToListAsync();
                    await _entityAttributeCacheManager.GenerateCacheAsync(query, cacheKey);
                }
                else
                {
                    query = _entityAttributeCacheManager.GetEntityCacheData(cacheKey);
                }
            }

            if (!string.IsNullOrEmpty(searchAttributeName))
                query = query.Where(c => c.AttributeName.ToLower().Contains(searchAttributeName.ToLower()));

            if (controlTypeIds.HasAny())
                query = query.Where(c => controlTypeIds.Contains(c.ControlTypeId));

            if (showActiveOnly)
                query = query.Where(c => c.IsActive == true);

            return query.ToList();
        }

        public async Task<PagedResult<EntityAttribute>> GetEntityAttributePagedResultListAsync(string searchAttributeName = null,
            string searchSystemName = null,
            List<int> controlTypeIds = null,
            bool showActiveOnly = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            var query = new List<EntityAttribute>().AsEnumerable();

            string cacheKey = typeof(EntityAttribute).FullName;
            if (!string.IsNullOrEmpty(searchSystemName))
            {
                cacheKey = cacheKey + '-' + searchSystemName;
                if (_entityAttributeCacheManager.EntityCacheNotExists(cacheKey))
                {
                    query = await _context.EntityAttribute.Where(c => c.SystemName.ToLower() == searchSystemName.ToLower()).AsNoTracking().ToListAsync();
                    await _entityAttributeCacheManager.GenerateCacheAsync(query, cacheKey);
                }
                else
                {
                    query = _entityAttributeCacheManager.GetEntityCacheData(cacheKey);
                }
            }
            else
            {
                if (_entityAttributeCacheManager.EntityCacheNotExists(cacheKey))
                {
                    query = await _context.EntityAttribute.AsNoTracking().ToListAsync();
                    await _entityAttributeCacheManager.GenerateCacheAsync(query, cacheKey);
                }
                else
                {
                    query = _entityAttributeCacheManager.GetEntityCacheData(cacheKey);
                }
            }

            if (!string.IsNullOrEmpty(searchAttributeName))
                query = query.Where(c => c.AttributeName.ToLower().Contains(searchAttributeName.ToLower()));

            if (controlTypeIds.HasAny())
                query = query.Where(c => controlTypeIds.Contains(c.ControlTypeId));

            if (showActiveOnly)
                query = query.Where(c => c.IsActive == true);

            return query.ToPagedResult(pageNumber, pageSize);
        }

        public async Task<IEnumerable<EntityAttribute>> GetEntityAttributeListOfEntityAsync(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                throw new ArgumentNullException(nameof(systemName));

            return _entityAttributeCacheManager.EntityCacheNotExists(typeof(EntityAttribute).FullName) ?
                    await _entityAttributeCacheManager.GenerateCacheAsync(await _context.EntityAttribute.Where(c => c.SystemName.ToLower() == systemName.ToLower()).ToListAsync(), typeof(EntityAttribute).FullName + '-' + systemName) : _entityAttributeCacheManager.GetEntityCacheData(typeof(EntityAttribute).FullName + '-' + systemName);
        }

        public async Task InsertEntityAttributeAsync(EntityAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.SystemName))
                throw new ArgumentNullException(nameof(attribute.SystemName));

            if (string.IsNullOrEmpty(attribute.AttributeName))
                throw new ArgumentNullException(nameof(attribute.AttributeName));

            attribute.Id = Guid.NewGuid();
            attribute.CreatedOn = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.EntityAttribute.AddAsync(attribute);
                await _context.SaveChangesAsync();
                _entityAttributeCacheManager.ClearCache(typeof(EntityAttribute).FullName);
                _entityAttributeCacheManager.ClearCache(typeof(EntityAttribute).FullName + '-' + attribute.SystemName);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public async Task UpdateEntityAttributeAsync(EntityAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.SystemName))
                throw new ArgumentNullException(nameof(attribute.SystemName));

            attribute.ModifiedOn = DateTime.UtcNow;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EntityAttribute.Update(attribute);
                await _context.SaveChangesAsync();
                _entityAttributeCacheManager.ClearCache(typeof(EntityAttribute).FullName);
                _entityAttributeCacheManager.ClearCache(typeof(EntityAttribute).FullName + '-' + attribute.SystemName);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public async Task DeleteEntityAttributeAsync(EntityAttribute attribute)
        {
            if (string.IsNullOrEmpty(attribute.SystemName))
                throw new ArgumentNullException(nameof(attribute.SystemName));

            attribute.DeletedOn = DateTime.UtcNow;
            attribute.Deleted = true;

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EntityAttribute.Update(attribute);
                await _context.SaveChangesAsync();
                _entityAttributeCacheManager.ClearCache(typeof(EntityAttribute).FullName);
                _entityAttributeCacheManager.ClearCache(typeof(EntityAttribute).FullName + '-' + attribute.SystemName);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }
        #endregion

        #region Entity Attribute Option
        public async Task<IEnumerable<EntityAttributeOption>> GetEntityAttributeOptionListByAttributeIdAsync(Guid attributeId)
        {
            return _entityAttributeOptionCacheManager.EntityCacheNotExists(typeof(EntityAttributeOption).FullName + '-' + attributeId) ?
                    await _entityAttributeOptionCacheManager.GenerateCacheAsync(await _context.EntityAttributeOption.Where(c => c.AttributeId == attributeId).AsNoTracking().ToListAsync(), typeof(EntityAttributeOption).FullName + '-' + attributeId) : _entityAttributeOptionCacheManager.GetEntityCacheData(typeof(EntityAttributeOption).FullName + '-' + attributeId);
        }

        public async Task<IEnumerable<EntityAttributeOption>> GetEntityAttributeOptionListAsync(Guid? attributeId = null,
            string searchOptionName = null,
            string searchOptionValue = null)
        {
            var query = new List<EntityAttributeOption>().AsEnumerable();

            if (attributeId.IsNotNullOrEmpty())
                query = _entityAttributeOptionCacheManager.EntityCacheNotExists(typeof(EntityAttributeOption).FullName + '-' + attributeId) ?
                        await _entityAttributeOptionCacheManager.GenerateCacheAsync(await _context.EntityAttributeOption.Where(c => c.AttributeId == attributeId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeOption).FullName + '-' + attributeId) : _entityAttributeOptionCacheManager.GetEntityCacheData(typeof(EntityAttributeOption).FullName + '-' + attributeId.Value);
            else
                query = _entityAttributeOptionCacheManager.EntityCacheNotExists(typeof(EntityAttributeOption).FullName) ?
                        await _entityAttributeOptionCacheManager.GenerateCacheAsync(await _context.EntityAttributeOption.Where(c => c.AttributeId == attributeId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeOption).FullName) : _entityAttributeOptionCacheManager.GetEntityCacheData(typeof(EntityAttributeOption).FullName);

            if (!string.IsNullOrEmpty(searchOptionName))
                query = query.Where(c => c.OptionName.ToLower().Contains(searchOptionName.ToLower()));

            if (!string.IsNullOrEmpty(searchOptionValue))
                query = query.Where(c => c.OptionValue.ToLower().Contains(searchOptionValue.ToLower()));

            return query.ToList();
        }

        public async Task<PagedResult<EntityAttributeOption>> GetEntityAttributeOptionPagedResultListAsync(Guid? attributeId = null,
            string searchOptionName = null,
            string searchOptionValue = null,
            int? pageNumber = 0,
            int? pageSize = 0)
        {
            var query = new List<EntityAttributeOption>().AsEnumerable();

            if (attributeId.IsNotNullOrEmpty())
                query = _entityAttributeOptionCacheManager.EntityCacheNotExists(typeof(EntityAttributeOption).FullName + '-' + attributeId) ?
                        await _entityAttributeOptionCacheManager.GenerateCacheAsync(await _context.EntityAttributeOption.Where(c => c.AttributeId == attributeId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeOption).FullName + '-' + attributeId) : _entityAttributeOptionCacheManager.GetEntityCacheData(typeof(EntityAttributeOption).FullName + '-' + attributeId.Value);
            else
                query = _entityAttributeOptionCacheManager.EntityCacheNotExists(typeof(EntityAttributeOption).FullName) ?
                        await _entityAttributeOptionCacheManager.GenerateCacheAsync(await _context.EntityAttributeOption.Where(c => c.AttributeId == attributeId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeOption).FullName) : _entityAttributeOptionCacheManager.GetEntityCacheData(typeof(EntityAttributeOption).FullName);

            if (!string.IsNullOrEmpty(searchOptionName))
                query = query.Where(c => c.OptionName.ToLower().Contains(searchOptionName.ToLower()));

            if (!string.IsNullOrEmpty(searchOptionValue))
                query = query.Where(c => c.OptionValue.ToLower().Contains(searchOptionValue.ToLower()));

            return query.ToPagedResult(pageNumber, pageSize);
        }

        public async Task InsertEntityAttributeOptionAsync(EntityAttributeOption option)
        {
            if (string.IsNullOrEmpty(option.OptionName))
                throw new ArgumentNullException(nameof(option.OptionName));

            if (string.IsNullOrEmpty(option.OptionValue))
                throw new ArgumentNullException(nameof(option.OptionValue));

            option.Id = Guid.NewGuid();

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.EntityAttributeOption.AddAsync(option);
                await _context.SaveChangesAsync();
                _entityAttributeOptionCacheManager.ClearCache(typeof(EntityAttributeOption).FullName);
                _entityAttributeOptionCacheManager.ClearCache(typeof(EntityAttributeOption).FullName + '-' + option.AttributeId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public async Task UpdateEntityAttributeOptionAsync(EntityAttributeOption option)
        {
            if (string.IsNullOrEmpty(option.OptionName))
                throw new ArgumentNullException(nameof(option.OptionName));

            if (string.IsNullOrEmpty(option.OptionValue))
                throw new ArgumentNullException(nameof(option.OptionValue));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EntityAttributeOption.Update(option);
                await _context.SaveChangesAsync();
                _entityAttributeOptionCacheManager.ClearCache(typeof(EntityAttributeOption).FullName);
                _entityAttributeOptionCacheManager.ClearCache(typeof(EntityAttributeOption).FullName + '-' + option.AttributeId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public async Task DeleteEntityAttributeOptionAsync(EntityAttributeOption option)
        {
            if (string.IsNullOrEmpty(option.OptionName))
                throw new ArgumentNullException(nameof(option.OptionName));

            if (string.IsNullOrEmpty(option.OptionValue))
                throw new ArgumentNullException(nameof(option.OptionValue));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EntityAttributeOption.Remove(option);
                await _context.SaveChangesAsync();
                _entityAttributeOptionCacheManager.ClearCache(typeof(EntityAttributeOption).FullName);
                _entityAttributeOptionCacheManager.ClearCache(typeof(EntityAttributeOption).FullName + '-' + option.AttributeId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }
        #endregion

        #region Entity Attribute Value
        public async Task<EntityAttributeValue> GetEntityAttributeValueByAttributeIdAsync(Guid attributeId)
        {
            if (attributeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(attributeId));

            return await _context.EntityAttributeValue.FirstOrDefaultAsync(c => c.AttributeId == attributeId);
        }

        public async Task<EntityAttributeValue> GetEntityAttributeValueByAttributeIdAndEntityIdAsync(Guid attributeId, Guid entityId)
        {
            if (attributeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(attributeId));
            if (entityId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(entityId));

            return await _context.EntityAttributeValue.FirstOrDefaultAsync(c => c.AttributeId == attributeId && c.EntityId == entityId);
        }

        public async Task<IEnumerable<EntityAttributeValue>> GetEntityAttributeValueListAsync(Guid? attributeId = null,
            Guid? entityId = null)
        {
            var query = new List<EntityAttributeValue>().AsEnumerable();

            if (attributeId.IsNotNullOrEmpty() && entityId.IsNotNullOrEmpty())
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value + '-' + entityId.Value) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.Where(c => c.AttributeId == attributeId.Value && c.EntityId == entityId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName + '-' + attributeId.Value + '-' + entityId.Value)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value + '-' + entityId.Value);
            else if (attributeId.IsNotNullOrEmpty())
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.Where(c => c.AttributeId == attributeId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName + '-' + attributeId.Value)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value);
            else if (entityId.IsNotNullOrEmpty())
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName + '-' + entityId.Value) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.Where(c => c.EntityId == entityId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName + '-' + entityId.Value)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName + '-' + entityId.Value);
            else
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName);

            return query.ToList();
        }

        public async Task<PagedResult<EntityAttributeValue>> GetEntityAttributeValuePagedResultListAsync(Guid? attributeId = null,
            Guid? entityId = null,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            var query = new List<EntityAttributeValue>().AsEnumerable();

            if (attributeId.IsNotNullOrEmpty() && entityId.IsNotNullOrEmpty())
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value + '-' + entityId.Value) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.Where(c => c.AttributeId == attributeId.Value && c.EntityId == entityId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName + '-' + attributeId.Value + '-' + entityId.Value)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value + '-' + entityId.Value);
            else if (attributeId.IsNotNullOrEmpty())
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.Where(c => c.AttributeId == attributeId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName + '-' + attributeId.Value)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName + '-' + attributeId.Value);
            else if (entityId.IsNotNullOrEmpty())
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName + '-' + entityId.Value) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.Where(c => c.EntityId == entityId.Value).AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName + '-' + entityId.Value)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName + '-' + entityId.Value);
            else
                query = _entityAttributeValueCacheManager.EntityCacheNotExists(typeof(EntityAttributeValue).FullName) ?
                        await _entityAttributeValueCacheManager.GenerateCacheAsync(await _context.EntityAttributeValue.AsNoTracking().ToListAsync(), typeof(EntityAttributeValue).FullName)
                        : _entityAttributeValueCacheManager.GetEntityCacheData(typeof(EntityAttributeValue).FullName);

            return query.ToPagedResult(pageNumber, pageSize);
        }

        public async Task InsertEntityAttributeValueAsync(EntityAttributeValue value)
        {
            if (value.AttributeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(value));
            if (value.EntityId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(value));

            value.Id = Guid.NewGuid();
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.EntityAttributeValue.AddAsync(value);
                await _context.SaveChangesAsync();
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.AttributeId + '-' + value.EntityId);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.AttributeId);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.EntityId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public async Task UpdateEntityAttributeValueAsync(EntityAttributeValue value)
        {
            if (value.AttributeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(value));
            if (value.EntityId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(value));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EntityAttributeValue.Update(value);
                await _context.SaveChangesAsync();
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.AttributeId + '-' + value.EntityId);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.AttributeId);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.EntityId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }

        public async Task DeleteEntityAttributeValueAsync(EntityAttributeValue value)
        {
            if (value.AttributeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(value));
            if (value.EntityId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(value));

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.EntityAttributeValue.Remove(value);
                await _context.SaveChangesAsync();
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.AttributeId + '-' + value.EntityId);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.AttributeId);
                _entityAttributeValueCacheManager.ClearCache(typeof(EntityAttributeValue).FullName + '-' + value.EntityId);
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception(ex?.Message);
            }
        }
        #endregion

        #region Entity - EntityAttribute Values Mapping
        public async Task<List<EntityAttributeValue>> MapEntityAttributeValuesAsync<TEntity>(IEnumerable<TEntity> entities)
            where TEntity : BaseEntity
        {
            if (entities == null || !entities.Any())
                throw new ArgumentNullException(nameof(entities));

            List<EntityAttributeValue> values = new List<EntityAttributeValue>();
            // Get Attributes of Entity
            List<EntityAttribute> attributes = (await GetEntityAttributeListOfEntityAsync(typeof(TEntity).FullName)).ToList();
            foreach (TEntity entity in entities)
            {
                if (entity.Id.IsNullOrEmpty()) continue;

                foreach (EntityAttribute attribute in attributes)
                {
                    EntityAttributeValue value = await GetEntityAttributeValueByAttributeIdAndEntityIdAsync(attribute.Id, entity.Id);
                    if (value == null) continue;

                    values.Add(new EntityAttributeValue
                    {
                        AttributeId = attribute.Id,
                        EntityId = entity.Id,
                        Value = value.Value
                    });
                }
            }

            return values;
        }

        public async Task<List<EntityAttributeValue>> MapEntityAttributeValuesAsync<TEntity>(TEntity entity)
            where TEntity : BaseEntity
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));
            if (entity.Id.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(entity));

            List<EntityAttributeValue> values = new List<EntityAttributeValue>();
            // Get Attributes of Entity
            List<EntityAttribute> attributes = (await GetEntityAttributeListOfEntityAsync(typeof(TEntity).FullName)).ToList();
            
            foreach (EntityAttribute attribute in attributes)
            {
                EntityAttributeValue value = await GetEntityAttributeValueByAttributeIdAndEntityIdAsync(attribute.Id, entity.Id);
                if (value == null) continue;

                values.Add(new EntityAttributeValue
                {
                    AttributeId = attribute.Id,
                    EntityId = entity.Id,
                    Value = value.Value
                });
            }

            return values;
        }

        public async Task InsertOrUpdateEntityAttributeValuesMappingAsync(List<EntityAttributeValue> values, Guid entityId)
        {
            if (entityId.IsNullOrEmpty())
                throw new ArgumentNullException();

            foreach (EntityAttributeValue value in values)
            {
                if (value.AttributeId.IsNullOrEmpty() || string.IsNullOrEmpty(value.Value))
                    throw new ArgumentNullException();

                var existingValue = await GetEntityAttributeValueByAttributeIdAndEntityIdAsync(value.AttributeId, entityId);
                if (existingValue != null)
                {
                    existingValue.Value = value.Value;
                    await UpdateEntityAttributeValueAsync(existingValue);
                }
                else
                {
                    value.EntityId = entityId;
                    await InsertEntityAttributeValueAsync(value);
                }
            }
        }
        #endregion
    }
}
