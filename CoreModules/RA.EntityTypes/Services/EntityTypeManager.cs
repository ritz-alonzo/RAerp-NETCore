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

        public virtual async Task<EntityType> GetById(Guid id)
        {
            return await _erpContext.EntityType.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<EntityType> GetTypeBySystemName(string systemName)
        {
            return await _erpContext.EntityType.FirstOrDefaultAsync(c => c.EntitySystemName.ToLower() == systemName.ToLower());
        }

        public virtual async Task<List<EntityType>> GetTypesBySystemName(string systemName)
        {
            return await _erpContext.EntityType.Where(c => c.EntitySystemName.Contains(systemName) && c.ParentEntityTypeId.HasValue).ToListAsync();
        }

        public virtual async Task<EntityType> GetTypeByEntityClassificationName(string systemName, string entityTypeName)
        {
            return await _erpContext.EntityType.FirstOrDefaultAsync(c => c.EntitySystemName.ToLower() == systemName.ToLower() && c.EntityClassificationName.ToLower() == entityTypeName.ToLower());
        }

        public virtual async Task<IEnumerable<EntityType>> GetList(
            string searchQuery = null, 
            DateTime? createdOn = null, 
            string entityClassificationName = null, 
            bool showAllChildEntities = false,
            Guid? parentEntityTypeId = null)
        {
            var query = _erpContext.EntityType.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.EntityName.ToLower().Contains(searchQuery.ToLower()) ||
                c.EntitySystemName.ToLower().Contains(searchQuery.ToLower()));
            
            if (!string.IsNullOrEmpty(entityClassificationName))
                query = query.Where(c => c.EntityClassificationName.Contains(entityClassificationName, StringComparison.InvariantCultureIgnoreCase));

            if (createdOn.HasValue)
                query = query.Where(c => c.InstalledOn >= createdOn.Value);

            if (showAllChildEntities)
                query = query.Where(c => c.ParentEntityTypeId.HasValue);
            else
                query = query.Where(c => !c.ParentEntityTypeId.HasValue);

            if (parentEntityTypeId.IsNotNullOrEmpty())
                query = query.Where(c => c.ParentEntityTypeId == parentEntityTypeId);

            query = query.OrderBy(c => c.InstalledOn);

            return await query.ToListAsync();
        }

        public virtual async Task Insert(EntityType entityType)
        {
            entityType.InstalledOn = DateTime.Now;
            await _erpContext.AddAsync(entityType);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task Update(EntityType entityType)
        {
            _erpContext.Update(entityType);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task<IEnumerable<EntityType>> GetChildEntities(Guid parentTypeId)
        {
            var query = _erpContext.EntityType.AsQueryable();

            if (parentTypeId.IsNullOrEmpty())
                throw new Exception("Parent type id cannot be null");

            query = query.Where(c => c.ParentEntityTypeId.HasValue && c.ParentEntityTypeId == parentTypeId).OrderBy(c => c.InstalledOn);

            return await query.ToListAsync();
        }

        public virtual async Task<EntityType> GetParentEntityTypeByChildEntityTypeId(Guid childEntityTypeId)
        {
            if (childEntityTypeId.IsNullOrEmpty()) 
                throw new Exception("Child Entity Type Id cannot be null");

            var childEntityType = await GetById(childEntityTypeId);

            if (childEntityType == null) 
                throw new Exception("Child Entity Type doesn't exists");

            if (!childEntityType.ParentEntityTypeId.HasValue)
                throw new Exception("Parent Entity Type Id cannot be null");

            var parentEntityType = await GetById(childEntityType.ParentEntityTypeId.Value);

            if (parentEntityType == null)
                throw new Exception("Parent Entity Type doesn't exists");

            return parentEntityType;

        }
        #endregion

        #region Settings

        public async Task<Setting> GetSettingById(Guid id)
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<Setting> GetSettingByEntityTypeId<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.Name.Equals(typeof(TEntity).Name) && c.EntityTypeId == entityTypeId);
        }


        public virtual async Task<Setting> GetSettingByEntitySystemNameAndEntityTypeId<TEntity, TSettings>(Guid entityTypeId, string entityTypeSystemName)
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
        public virtual async Task InsertEntitySetting<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
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
                CreatedOn = DateTime.Now,
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
        public virtual async Task UpdateSettingDataOfEntity<TEntity, TSettings>(TSettings settings, Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            var settingData = JsonConvert.SerializeObject(settings);

            var setting = await GetSettingByEntityTypeId<TEntity, TSettings>(entityTypeId);
            if (setting != null)
            {
                setting.Data = settingData;
                setting.ModifiedOn = DateTime.Now;
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
        public virtual async Task<TSettings> GetSettingDataOfEntity<TEntity, TSettings>(Guid entityTypeId, string entitySystemName = null)
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
                    await InsertEntitySetting<TEntity, TSettings>(entityTypeId);
                    setting = await _erpContext.Setting.FirstOrDefaultAsync(c => c.Name.Equals(typeof(TEntity).Name) && c.EntityTypeId == entityTypeId);
                }
                else
                {
                    await InsertEntitySetting<TEntity, TSettings>(entityTypeId, entitySystemName);
                    setting = await _erpContext.Setting.FirstOrDefaultAsync(c => c.SystemName.Equals(entitySystemName) && c.EntityTypeId == entityTypeId);
                }
            }

            TSettings settingsData = new BaseEntityTypeSetting() as TSettings;
            if (setting.Data != null && setting.Data != "{}")
                settingsData = JsonConvert.DeserializeObject<TSettings>(setting.Data);

            return settingsData;

        }

        // will be used in insert, update of Entity
        public virtual async Task<string> GetCurrentTemplateOfEntity<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            var setting = await GetSettingDataOfEntity<TEntity, TSettings>(entityTypeId);

            if (setting == null)
                throw new Exception(nameof(setting));

            if (string.IsNullOrEmpty(setting.Template))
                return null;

            return (setting.TemplateCount + setting.TemplateIncrementCount).ToString(setting.Template);
        }

        // will be used in insert, update of Entity
        public virtual async Task IncreaseTemplateCountOfEntity<TEntity, TSettings>(Guid entityTypeId)
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            var setting = await GetSettingDataOfEntity<TEntity, TSettings>(entityTypeId);

            if (setting == null)
                throw new Exception(nameof(setting));

            setting.TemplateCount += setting.TemplateIncrementCount;

            await UpdateSettingDataOfEntity<TEntity, TSettings>(setting, entityTypeId);
        }

        #endregion

        #region Select list

        public virtual async Task<List<SelectListItem>> GetEntityTypesSelectList()
        {
            var entityTypeList = await GetList(showAllChildEntities: true);

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
                var parentEntity = await GetById(entityType.ParentEntityTypeId.Value);
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
