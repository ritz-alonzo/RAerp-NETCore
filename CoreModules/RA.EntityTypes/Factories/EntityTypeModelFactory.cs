using AutoMapper;
using Microsoft.IdentityModel.Tokens;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Factories.CoreFactories;
using System.Security.Policy;

namespace MMS.Factories.Factories.EntityTypeFactory
{
    /// <summary>
    /// Preparing model of Entities (plugins)
    /// will add form behaviors in the future.
    /// </summary>
    public class EntityTypeModelFactory : IEntityTypeModelFactory
    {
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IBaseModelFactory _baseSearchModelFactory;
        private readonly IMapper _mapper;

        public EntityTypeModelFactory(IEntityTypeManager entityTypeManager, IBaseModelFactory baseSearchModelFactory, IMapper mapper)
        {
            _entityTypeManager = entityTypeManager;
            _baseSearchModelFactory = baseSearchModelFactory;
            _mapper = mapper;
        }

        public virtual async Task<EntityTypeModel> PrepareEntityTypeModel(EntityTypeModel model, Guid entityTypeId, bool childEntityCreation = false)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            if (entityTypeId.IsNullOrEmpty())
                throw new Exception(EntityTypeMessages.EntityTypeIdNotExists);

            // not child entity type creation
            if (!childEntityCreation)
            {
                var entityType = await _entityTypeManager.GetById(entityTypeId);
                if (entityType == null)
                    throw new Exception(EntityTypeMessages.EntityTypeNotExists);

                model = _mapper.Map(entityType, model);
            }
            // child entity type creation
            else
            {
                model.ParentEntityTypeId = entityTypeId;
            }

            return model;
        }

        public virtual async Task<EntityTypeSearchModel> PrepareEntityTypeSearchModel(EntityTypeSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseSearchModelFactory.PrepareBaseSearchModel(searchModel, pageSize, pageNumber);
            
            if (searchModel.SearchParentEntityTypeId.IsNotNullOrEmpty() && searchModel.ChildEntitySearchEnabled == true)
            {
                searchModel.EntityTypes = await PrepareChildEntityTypeListModel(searchModel);
                searchModel.EntityTypeName = _entityTypeManager.GetById(searchModel.SearchParentEntityTypeId.Value).Result.EntityName;
            }
            else
            {
                searchModel.EntityTypes = await PrepareEntityTypeListModel(searchModel);
                searchModel.EntityTypeName = "Entity Types";
            }

            // system name mapping for active menu
            searchModel.EntityTypeSystemName = typeof(EntityType).FullName;

            searchModel.TotalItems = (int)searchModel.EntityTypes.TotalItems;
            searchModel.PageSize = searchModel.EntityTypes.PageSize; 
            searchModel.CurrentItemsShown = searchModel.EntityTypes.Items.Count;

            return searchModel;
        }

        public virtual async Task<EntityTypeListModel> PrepareEntityTypeListModel(EntityTypeSearchModel searchModel)
        {
            var model = new EntityTypeListModel();

            var entityTypeList = await _entityTypeManager.GetList(
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

                return entityTypeModel;

            }).ToList();

            _baseSearchModelFactory.PrepareBaseListModel(model, entityTypes, searchModel, entityTypeList.Count());

            return model;
        }

        public virtual async Task<EntityTypeListModel> PrepareChildEntityTypeListModel(EntityTypeSearchModel searchModel)
        {
            var model = new EntityTypeListModel();

            var entityTypeList = await _entityTypeManager.GetList(
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

                return entityTypeModel;

            }).ToList();

            _baseSearchModelFactory.PrepareBaseListModel(model, entityTypes, searchModel, entityTypeList.Count());

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
            var entityHasChild = _entityTypeManager.GetChildEntities(entityTypeId).Result;

            if (entityHasChild.Any())
                return false;

            var entityType = _entityTypeManager.GetById(entityTypeId).Result;

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
