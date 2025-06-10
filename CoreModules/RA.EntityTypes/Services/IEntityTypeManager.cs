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
        Task<EntityType> GetById(Guid id);
        Task<EntityType> GetTypeBySystemName(string systemName);
        Task<List<EntityType>> GetTypesBySystemName(string systemName);
        Task<EntityType> GetTypeByEntityClassificationName(string systemName, string entityTypeName);
        Task<IEnumerable<EntityType>> GetList(
            string searchQuery = null, 
            DateTime? createdOn = null, 
            string entityClassificationName = null, 
            bool showAllChildEntities = false,
            Guid? parentEntityTypeId = null);
        Task Insert(EntityType entityType);
        Task Update(EntityType entityType);
        Task<IEnumerable<EntityType>> GetChildEntities(Guid parentTypeId);
        Task<EntityType> GetParentEntityTypeByChildEntityTypeId(Guid childEntityTypeId);
        #endregion

        #region Settings
        Task<Setting> GetSettingById(Guid id);
        Task<Setting> GetSettingByEntityTypeId<TEntity, TSettings>(Guid entityTypeId) 
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task<Setting> GetSettingByEntitySystemNameAndEntityTypeId<TEntity, TSettings>(Guid entityTypeId, string entityTypeSystemName)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task InsertEntitySetting<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task UpdateSettingDataOfEntity<TEntity, TSettings>(TSettings settings, Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task<TSettings> GetSettingDataOfEntity<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task<string> GetCurrentTemplateOfEntity<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;
        Task IncreaseTemplateCountOfEntity<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting;

        #endregion

        #region Select List

        Task<List<SelectListItem>> GetEntityTypesSelectList();

        #endregion
    }
}