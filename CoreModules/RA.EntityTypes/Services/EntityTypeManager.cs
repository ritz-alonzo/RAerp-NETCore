using Newtonsoft.Json;
using RA.Data.App_Data;
using RA.Data.Domain.Settings;
using RA.EntityTypes.Data;
using RA.EntityTypes.Domain;
using RA.Data.Domain.EntityTypes;
using RA.WebFramework.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RA.EntityTypes.Services
{
    /// <summary>
    /// Base Setting service where Base Entity is inherited 
    /// Only Entities will be able use this service
    /// </summary>
    /// <typeparam name="TSettings"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityTypeManager : IEntityTypeManager
    {
        private readonly RAerpContext _erpContext;

        public EntityTypeManager(RAerpContext erpContext)
        {
            _erpContext = erpContext;
        }

        #region Entity Type CRUD

        public virtual async Task<EntityType> GetByIdAsync(Guid id)
        {
            return await _erpContext.EntityType.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<EntityType> GetTypeBySystemNameAsync(string systemName)
        {
            return await _erpContext.EntityType.FirstOrDefaultAsync(c => c.EntitySystemName.ToLower() == systemName.ToLower());
        }

        public virtual async Task<List<EntityType>> GetTypesBySystemNameAsync(string systemName)
        {
            return await _erpContext.EntityType.Where(c => c.EntitySystemName.Contains(systemName) && c.ParentEntityTypeId.HasValue).ToListAsync();
        }

        public virtual async Task<EntityType> GetTypeByEntityClassificationNameAsync(string systemName, string entityTypeName)
        {
            return await _erpContext.EntityType.FirstOrDefaultAsync(c => c.EntitySystemName.ToLower() == systemName.ToLower() && c.EntityClassificationName.ToLower() == entityTypeName.ToLower());
        }

        public virtual async Task<IEnumerable<EntityType>> GetListAsync(
            string searchQuery = null, 
            DateTime? createdOn = null, 
            string entityClassificationName = null, 
            bool showAllChildEntities = false,
            Guid? parentEntityTypeId = null)
        {
            var query = _erpContext.EntityType.AsEnumerable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.EntityName.ToLower().Contains(searchQuery.ToLower()) ||
                c.EntitySystemName.ToLower().Contains(searchQuery.ToLower()));
            
            if (!string.IsNullOrEmpty(entityClassificationName))
                query = query.Where(c => c.EntityClassificationName.Contains(entityClassificationName, StringComparison.InvariantCultureIgnoreCase));

            if (createdOn.HasValue)
            {
                createdOn = createdOn.ConvertToUTC();
                query = query.Where(c => c.InstalledOn.ConvertToUTC() >= createdOn.Value);
            }

            if (showAllChildEntities)
                query = query.Where(c => c.ParentEntityTypeId.HasValue);
            else
                query = query.Where(c => !c.ParentEntityTypeId.HasValue);

            if (parentEntityTypeId.IsNotNullOrEmpty())
                query = query.Where(c => c.ParentEntityTypeId == parentEntityTypeId);

            query = query.Where(c => c.Installed).OrderBy(c => c.InstalledOn);

            return query.ToList();
        }

        public virtual async Task InsertAsync(EntityType entityType)
        {
            entityType.InstalledOn = DateTime.UtcNow;
            await _erpContext.AddAsync(entityType);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task UpdateAsync(EntityType entityType)
        {
            _erpContext.Update(entityType);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<EntityType>> GetChildEntitiesAsync(Guid parentTypeId)
        {
            var query = _erpContext.EntityType.AsEnumerable();

            if (parentTypeId.IsNullOrEmpty())
                throw new Exception("Parent type id cannot be null");

            query = query.Where(c => c.ParentEntityTypeId.HasValue && c.ParentEntityTypeId == parentTypeId).OrderBy(c => c.InstalledOn);

            return query.ToList();
        }

        public virtual async Task<EntityType> GetParentEntityTypeByChildEntityTypeIdAsync(Guid childEntityTypeId)
        {
            if (childEntityTypeId.IsNullOrEmpty()) 
                throw new Exception("Child Entity Type Id cannot be null");

            var childEntityType = await GetByIdAsync(childEntityTypeId);

            if (childEntityType == null) 
                throw new Exception("Child Entity Type doesn't exists");

            if (!childEntityType.ParentEntityTypeId.HasValue)
                throw new Exception("Parent Entity Type Id cannot be null");

            var parentEntityType = await GetByIdAsync(childEntityType.ParentEntityTypeId.Value);

            if (parentEntityType == null)
                throw new Exception("Parent Entity Type doesn't exists");

            return parentEntityType;

        }
        #endregion

        #region Settings

        public async Task<Setting> GetSettingByIdAsync(Guid id)
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<Setting> GetSettingByEntityTypeIdAsync<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.Name.Equals(typeof(TEntity).Name) && c.EntityTypeId == entityTypeId);
        }


        public virtual async Task<Setting> GetSettingByEntitySystemNameAndEntityTypeIdAsync<TEntity, TSettings>(Guid entityTypeId, string entityTypeSystemName)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.SystemName.Equals(entityTypeSystemName) && c.EntityTypeId == entityTypeId);
        }

        /// <summary>
        /// Insert setting will insert TSettings 
        /// already converted from model to Setting of Entity
        /// Data of Setting will be converted here
        /// </summary>
        /// <param name="settings"></param>
        public virtual async Task InsertEntitySettingAsync<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            var setting = new Setting()
            {
                Id = Guid.NewGuid(),
                Name = typeof(TEntity).Name,
                SystemName = entitySystemName,
                EntityTypeId = entityTypeId,
                Data = "{}",
                CreatedOn = DateTime.UtcNow,
            };
            await _erpContext.Setting.AddAsync(setting);
            await _erpContext.SaveChangesAsync();
        }

        /// <summary>
        /// Insert setting will insert TSettings 
        /// already converted from model to Setting of Entity
        /// Data of Setting will be converted here
        /// </summary>
        /// <param name="settings"></param>
        public virtual async Task UpdateSettingDataOfEntityAsync<TEntity, TSettings>(TSettings settings, Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            var settingData = JsonConvert.SerializeObject(settings);

            var setting = await GetSettingByEntityTypeIdAsync<TEntity, TSettings>(entityTypeId);
            if (setting != null)
            {
                setting.Data = settingData;
                setting.ModifiedOn = DateTime.UtcNow;
            }
            else
            {
                return;
            }

            _erpContext.Setting.Update(setting);
            await _erpContext.SaveChangesAsync();
        }

        /// <summary>
        /// This method will be used to get saved setting of Entity
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public virtual async Task<TSettings> GetSettingDataOfEntityAsync<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            Setting setting = null;

            if (string.IsNullOrEmpty(entitySystemName))
                setting = await _erpContext.Setting.FirstOrDefaultAsync(c => c.Name.Equals(typeof(TEntity).Name) && c.EntityTypeId == entityTypeId);
            else
                setting = await _erpContext.Setting.FirstOrDefaultAsync(c => c.SystemName.Equals(entitySystemName) && c.EntityTypeId == entityTypeId);

            if (setting == null)
            {
                if (string.IsNullOrEmpty(entitySystemName))
                {
                    await InsertEntitySettingAsync<TEntity, TSettings>(entityTypeId);
                    setting = await _erpContext.Setting.FirstOrDefaultAsync(c => c.Name.Equals(typeof(TEntity).Name) && c.EntityTypeId == entityTypeId);
                }
                else
                {
                    await InsertEntitySettingAsync<TEntity, TSettings>(entityTypeId, entitySystemName);
                    setting = await _erpContext.Setting.FirstOrDefaultAsync(c => c.SystemName.Equals(entitySystemName) && c.EntityTypeId == entityTypeId);
                }
            }

            TSettings settingsData = new BaseEntityTypeSetting() as TSettings;
            if (setting.Data != null && setting.Data != "{}")
                settingsData = JsonConvert.DeserializeObject<TSettings>(setting.Data);

            return settingsData;

        }

        // will be used in insert, update of Entity
        public virtual async Task<string> GetCurrentTemplateOfEntityAsync<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            var setting = await GetSettingDataOfEntityAsync<TEntity, TSettings>(entityTypeId);

            if (setting == null)
                throw new Exception(nameof(setting));

            if (string.IsNullOrEmpty(setting.Template))
                return null;

            return (setting.TemplateCount + setting.TemplateIncrementCount).ToString(setting.Template);
        }

        // will be used in insert, update of Entity
        public virtual async Task IncreaseTemplateCountOfEntityAsync<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            var setting = await GetSettingDataOfEntityAsync<TEntity, TSettings>(entityTypeId);

            if (setting == null)
                throw new Exception(nameof(setting));

            setting.TemplateCount += setting.TemplateIncrementCount;

            await UpdateSettingDataOfEntityAsync<TEntity, TSettings>(setting, entityTypeId);
        }

        #endregion

        #region Select list

        public virtual async Task<List<SelectListItem>> GetEntityTypesSelectListAsync()
        {
            var entityTypeList = await GetListAsync(showAllChildEntities: true);

            var entityTypeSelectList = new List<SelectListItem>
            {
                // show default
                new SelectListItem()
                {
                    Value = Guid.Empty.ToString(),
                    Text = "None"
                }
            };

            foreach (var entityType in entityTypeList)
            {
                var parentEntity = await GetByIdAsync(entityType.ParentEntityTypeId.Value);
                var entitySelectListText = entityType.EntityName;
                if (parentEntity != null)
                    entitySelectListText = entitySelectListText + " - " + parentEntity.EntityName;

                entityTypeSelectList.Add(new SelectListItem()
                {
                    Value = entityType.Id.ToString(),
                    Text = entitySelectListText
                });
            }

            return entityTypeSelectList;
        }

        public virtual async Task<List<SelectListItem>> GetEntityTypesSelectListAsync(string systemName)
        {
            var entityTypeList = await GetListAsync(searchQuery: systemName, showAllChildEntities: true);

            var entityTypeSelectList = new List<SelectListItem>
            {
                // show default
                new SelectListItem()
                {
                    Value = Guid.Empty.ToString(),
                    Text = "None"
                }
            };

            foreach (var entityType in entityTypeList)
            {
                var parentEntity = await GetByIdAsync(entityType.ParentEntityTypeId.Value);
                var entitySelectListText = entityType.EntityName;
                if (parentEntity != null)
                    entitySelectListText = entitySelectListText + " - " + parentEntity.EntityName;

                entityTypeSelectList.Add(new SelectListItem()
                {
                    Value = entityType.Id.ToString(),
                    Text = entitySelectListText
                });
            }

            return entityTypeSelectList;
        }

        #endregion
    }
}
