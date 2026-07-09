using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Data.Domain.Application;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Factories.CoreFactories;
using RAerp.Services.ApplicationSettingServices;
using System.Security.Policy;

namespace MMS.Factories.Factories.EntityTypeFactory
{
    /// <summary>
    /// Preparing model of Entities (plugins)
    /// will add form behaviors in the future.
    /// </summary>
    public class EntityTypeModelFactory : IEntityTypeModelFactory
    {
        #region Constants
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IBaseModelFactory _baseModelFactory;
        private readonly IMapper _mapper;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        #endregion

        #region Ctor
        public EntityTypeModelFactory(IEntityTypeManager entityTypeManager, 
            IBaseModelFactory baseModelFactory, 
            IMapper mapper, 
            IApplicationSettingService applicationSettingService)
        {
            _entityTypeManager = entityTypeManager;
            _baseModelFactory = baseModelFactory;
            _mapper = mapper;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync().Result;
        }
        #endregion

        public virtual async Task<EntityTypeModel> PrepareEntityTypeModelAsync(EntityTypeModel model, Guid entityTypeId, bool childEntityCreation = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (entityTypeId.IsNullOrEmpty())
                throw new Exception(EntityTypeMessages.EntityTypeIdNotExists);

            // not child entity type creation
            if (!childEntityCreation)
            {
                var entityType = await _entityTypeManager.GetByIdAsync(entityTypeId);
                if (entityType == null)
                    throw new Exception(EntityTypeMessages.EntityTypeNotExists);

                model = _mapper.Map(entityType, model);

                if (model.InstalledOn != DateTime.MinValue)
                    model.InstalledOn = model.InstalledOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);

                if (model.UnInstalledOn.HasValue)
                    model.UnInstalledOn = model.UnInstalledOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
            }
            // child entity type creation
            else
            {
                model.ParentEntityTypeId = entityTypeId;
            }

            return model;
        }

        public virtual async Task<EntityTypeSearchModel> PrepareEntityTypeSearchModelAsync(EntityTypeSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseModelFactory.PrepareBaseSearchModel(searchModel, pageSize, pageNumber);
            
            if (searchModel.SearchParentEntityTypeId.IsNotNullOrEmpty() && searchModel.ChildEntitySearchEnabled == true)
            {
                searchModel.EntityTypes = await PrepareChildEntityTypeListModelAsync(searchModel);
                searchModel.EntityTypeName = _entityTypeManager.GetByIdAsync(searchModel.SearchParentEntityTypeId.Value).Result.EntityName;
            }
            else
            {
                searchModel.EntityTypes = await PrepareEntityTypeListModelAsync(searchModel);
                searchModel.EntityTypeName = "Entity Types";
            }

            // system name mapping for active menu
            searchModel.EntityTypeSystemName = typeof(EntityType).FullName;

            searchModel.TotalItems = (int)searchModel.EntityTypes.TotalItems;
            searchModel.PageSize = searchModel.EntityTypes.PageSize; 
            searchModel.CurrentItemsShown = searchModel.EntityTypes.Items.Count;

            return searchModel;
        }

        public virtual async Task<EntityTypeListModel> PrepareEntityTypeListModelAsync(EntityTypeSearchModel searchModel)
        {
            var model = new EntityTypeListModel();

            var entityTypeList = await _entityTypeManager.GetListAsync(
                searchQuery: searchModel.SearchQuery,
                createdOn: searchModel.SearchInstalledOn
                );

            var entityTypes = new List<EntityTypeModel>();

            entityTypes = entityTypeList.Select(entityType =>
            {
                var entityTypeModel = new EntityTypeModel();
                entityTypeModel = _mapper.Map(entityType, entityTypeModel);
                entityTypeModel.ModalConfigureEnabled = UseModalConfiguration(entityType.Id);
                entityTypeModel.PluginController = GeneratePluginControllerString(entityType, entityTypeModel.ModalConfigureEnabled);
                entityTypeModel.PluginConfigurationUrl = GetPluginConfigurationUrl(entityTypeModel.PluginController, entityTypeModel.ModalConfigureEnabled);
                entityTypeModel.InstalledOn = entityTypeModel.InstalledOn != DateTime.MinValue ? entityTypeModel.InstalledOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : DateTime.MinValue.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
                entityTypeModel.UnInstalledOn = entityTypeModel.UnInstalledOn.HasValue ? entityTypeModel.UnInstalledOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : null;
                return entityTypeModel;

            }).ToList();

            _baseModelFactory.PrepareBaseListModel(model, entityTypes, searchModel, entityTypeList.Count());

            return model;
        }

        public virtual async Task<EntityTypeListModel> PrepareChildEntityTypeListModelAsync(EntityTypeSearchModel searchModel)
        {
            var model = new EntityTypeListModel();

            var entityTypeList = await _entityTypeManager.GetListAsync(
                searchQuery: searchModel.SearchQuery,
                createdOn: searchModel.SearchInstalledOn, 
                parentEntityTypeId: searchModel.SearchParentEntityTypeId,
                showAllChildEntities: true
                );

            var entityTypes = new List<EntityTypeModel>();

            entityTypes = entityTypeList.Select(entityType =>
            {
                var entityTypeModel = new EntityTypeModel();
                entityTypeModel = _mapper.Map(entityType, entityTypeModel);
                entityTypeModel.ModalConfigureEnabled = UseModalConfiguration(entityType.Id);
                entityTypeModel.PluginController = GeneratePluginControllerString(entityType, entityTypeModel.ModalConfigureEnabled);
                entityTypeModel.PluginConfigurationUrl = GetPluginConfigurationUrl(entityTypeModel.PluginController, entityTypeModel.ModalConfigureEnabled);
                entityTypeModel.InstalledOn = entityTypeModel.InstalledOn != DateTime.MinValue ? entityTypeModel.InstalledOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : DateTime.MinValue.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
                entityTypeModel.UnInstalledOn = entityTypeModel.UnInstalledOn.HasValue ? entityTypeModel.UnInstalledOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : null;

                return entityTypeModel;

            }).ToList();

            _baseModelFactory.PrepareBaseListModel(model, entityTypes, searchModel, entityTypeList.Count());

            return model;
        }

        #region Methods

        private string GeneratePluginControllerString(EntityType entityType, bool modalConfigureEnabled)
        {
            var pluginController = "";

            var entityName = "";

            if (entityType.ParentEntityTypeId.IsNotNullOrEmpty())
            {
                // get the parent entity name
                var initialEntityNameIndex = entityType.EntitySystemName.Substring(entityType.EntitySystemName.IndexOf(".") + 1);

                var domainIndex = initialEntityNameIndex.IndexOf(".Domain") - 1;

                entityName = initialEntityNameIndex.Substring(0, domainIndex);
            }
            else
            {
                entityName = entityType.EntityName;
            }

            if (!string.IsNullOrEmpty(entityName))
            {
                if (entityName.Equals("Category") || entityName.Equals("BusinessEntity"))
                {
                    pluginController = entityName.Replace("y", "ies");
                }
                else
                {
                    pluginController = entityName + "s";
                }

                if (!modalConfigureEnabled)
                {
                    pluginController = "EntityTypes";
                }
            }

            return pluginController;
        }

        private string GetPluginConfigurationUrl(string pluginController, bool modalConfigureEnabled)
        {
            var configUrl = $"/{pluginController}/Configuration";

            if (!modalConfigureEnabled)
            {
                configUrl = $"/{pluginController}/ChildEntityTypeList";
            }

            return configUrl;
        }

        private bool UseModalConfiguration(Guid entityTypeId)
        {
            var entityHasChild = _entityTypeManager.GetChildEntitiesAsync(entityTypeId).Result;

            if (entityHasChild.Any())
                return false;

            var entityType = _entityTypeManager.GetByIdAsync(entityTypeId).Result;

            if (entityType != null)
            {
                if ((entityType.EntityName.Contains("Catalog", StringComparison.InvariantCultureIgnoreCase) &&
                    !entityType.ParentEntityTypeId.HasValue) || 
                    (entityType.EntityName.Contains("BusinessEntity", StringComparison.InvariantCultureIgnoreCase) &&
                    !entityType.ParentEntityTypeId.HasValue) ||
                    (entityType.EntityName.Contains("Category", StringComparison.InvariantCultureIgnoreCase) &&
                    !entityType.ParentEntityTypeId.HasValue))
                    return false;
                else
                    return true;
            }

            return true;
        }

        #endregion
    }
}
