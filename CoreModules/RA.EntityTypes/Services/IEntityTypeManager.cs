using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Data.Domain.EntityTypes;
using RA.Data.Domain.Settings;
using RA.EntityTypes.Data;
using RA.EntityTypes.Domain;

namespace RA.EntityTypes.Services
{
    public interface IEntityTypeManager
    {

        #region Entity Type CRUD
        Task<EntityType> GetByIdAsync(Guid id);
        Task<EntityType> GetTypeBySystemNameAsync(string systemName);
        Task<List<EntityType>> GetTypesBySystemNameAsync(string systemName);
        Task<EntityType> GetTypeByEntityClassificationNameAsync(string systemName, string entityTypeName);
        Task<IEnumerable<EntityType>> GetListAsync(
            string searchQuery = null, 
            DateTime? createdOn = null, 
            string entityClassificationName = null, 
            bool showAllChildEntities = false,
            Guid? parentEntityTypeId = null);
        Task InsertAsync(EntityType entityType);
        Task UpdateAsync(EntityType entityType);
        Task<IEnumerable<EntityType>> GetChildEntitiesAsync(Guid parentTypeId);
        Task<EntityType> GetParentEntityTypeByChildEntityTypeIdAsync(Guid childEntityTypeId);
        #endregion

        #region Settings
        Task<Setting> GetSettingByIdAsync(Guid id);
        Task<Setting> GetSettingByEntityTypeIdAsync<TEntity, TSettings>(Guid entityTypeId) 
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task<Setting> GetSettingByEntitySystemNameAndEntityTypeIdAsync<TEntity, TSettings>(Guid entityTypeId, string entityTypeSystemName)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task InsertEntitySettingAsync<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task UpdateSettingDataOfEntityAsync<TEntity, TSettings>(TSettings settings, Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task<TSettings> GetSettingDataOfEntityAsync<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task<string> GetCurrentTemplateOfEntityAsync<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task IncreaseTemplateCountOfEntityAsync<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;

        #endregion

        #region Select List

        Task<List<SelectListItem>> GetEntityTypesSelectListAsync();
        Task<List<SelectListItem>> GetEntityTypesSelectListAsync(string systemName);

        #endregion
    }
}