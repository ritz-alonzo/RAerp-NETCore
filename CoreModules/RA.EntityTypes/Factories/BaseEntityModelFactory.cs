using AutoMapper;
using Microsoft.AspNetCore.Http;
using RA.Core.Domain;
using RA.Core.Helpers;
using RA.Core.Models.BaseModels;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Core.Models.PortableViewModels;
using RA.Core.Models.UserInfaceModels;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Data;
using RA.EntityTypes.Domain;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Factories.CoreFactories;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.UserHelper;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.EntityTypes.Factories
{
    /// <summary>
    /// Will add Access Rights here
    /// </summary>
    public class BaseEntityModelFactory : BaseModelFactory, IBaseEntityModelFactory
    {
        #region Constants
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IAccessControl _accessControl;
        private readonly IMapper _mapper;
        #endregion

        #region Ctor
        public BaseEntityModelFactory(
            IUserIdentity userIdentity,
            IHttpContextAccessor httpContextAccessor,
            ISettingService settingService,
            IEntityTypeManager entityTypeManager,
            IAccessControl accessControl,
            IMapper mapper)
            : base(userIdentity, httpContextAccessor, settingService)
        {
            _entityTypeManager = entityTypeManager;
            _accessControl = accessControl;
            _mapper = mapper;
        }
        #endregion

        #region CRUD
        /// <summary>
        /// Preparing base search model of entity
        /// </summary>
        /// <typeparam name="TSearch"></typeparam>
        /// <param name="searchModel"></param>
        /// <param name="pageSize"></param>
        /// <param name="pageNumber"></param>
        /// <returns></returns>
        public TSearch PrepareBaseEntitySearchModel<TSearch>(TSearch searchModel, EntityType entityType, int pageSize, int pageNumber)
            where TSearch : BaseEntitySearchModel
        {
            searchModel = PrepareBaseSearchModel(searchModel, pageSize, pageNumber);
            searchModel.EntityTypeSystemName = entityType.EntitySystemName;
            searchModel.EntityTypeName = entityType.EntityName;

            return searchModel;
        }

        public TList PrepareBaseEntityListModel<TList, TModel, TSearch>(TList list, List<TModel> listModel, TSearch searchModel, int totalItems)
            where TList : BaseListModel<TModel>
            where TModel : BaseEntityModel
            where TSearch : BaseEntitySearchModel
        {
            list = PrepareBaseListModel(list, listModel, searchModel, totalItems);

            return list;
        }

        /// <summary>
        /// Preparing base model of entity
        /// will add settings here
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TModel> PrepareBaseEntityModelAsync<TModel, TEntity, TSettings>(TModel model, TEntity entity, TSettings settings)
            where TModel : BaseEntityModel
            where TSettings : BaseEntityTypeSetting
            where TEntity : BaseEntityType
        {
            model = await PrepareBaseModelAsync(model);
            model.EntityTypeSystemName = entity.EntitySystemName;
            model.EntityTypeName = GetEntityTypeNameFromSystemName(entity.EntitySystemName);

            // Preparation of UI Access Rights and Settings Model
            model = await PrepareBaseEntityModelUIAccessAsync<TModel, TEntity, TSettings>(model, entity, settings);
            // Preparation of View Components
            model = PrepareBaseEntityViewComponent<TModel, TEntity>(model, entity);

            return model;
        }

        #endregion

        #region Configuration

        public async Task<TConfig> PrepareBaseEntityConfigureModelAsync<TConfig, TEntity, TSettings>(TConfig configureModel, Guid entityTypeId, string entityTypeSystemName = null)
            where TConfig : BaseEntityConfigureModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(TEntity));

            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<TEntity, TSettings>(entityTypeId, entityTypeSystemName);

            configureModel.EntityTypeId = entityTypeId;
            configureModel.Installed = (await _entityTypeManager.GetByIdAsync(entityTypeId)).Installed;

            if (settings == null)
            {
                if (!string.IsNullOrEmpty(entityTypeSystemName))
                    configureModel.SystemName = entityTypeSystemName;
            }
            else
            {
                if (!string.IsNullOrEmpty(entityTypeSystemName) && string.IsNullOrEmpty(settings.SystemName))
                    settings.SystemName = entityTypeSystemName;
                configureModel = _mapper.Map(settings, configureModel);
            }
            return configureModel;
        }

        #endregion

        #region User Interface Access

        public async Task<TModel> PrepareBaseEntityModelUIAccessAsync<TModel, TEntity, TSettings>(TModel model, TEntity entityType, TSettings settings)
            where TModel : BaseEntityModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            if (entityType == null)
                throw new ArgumentNullException(typeof(TEntity).Name);

            if (settings == null)
                throw new ArgumentNullException(typeof(TSettings).Name);

            if (settings.Enabled && !entityType.Deleted)
            {
                // need to add permission here
                model.UserInterface.ViewEnabled = await _accessControl.HasViewAccessAsync<TEntity>();
                model.UserInterface.CreateEnabled = await _accessControl.HasCreateAccessAsync<TEntity>();
                model.UserInterface.UpdateEnabled = await _accessControl.HasUpdateAccessAsync<TEntity>();
                model.UserInterface.DeleteEnabled = await _accessControl.HasDeleteAccessAsync<TEntity>();
                // settings based
                model.UserInterface.AutoGeneratedTemplate = settings.AutoGeneratedTemplate;
                model.UserInterface.RedirectUsingCodeEnabled = settings.RedirectByCodeEnabled;
                model.Enabled = settings.Enabled;
                model.AddressEnabled = settings.AddressEnabled;
            }
            
            return model;
        }

        public async Task<TList> PrepareBaseEntityListModelUIAccessAsync<TList, TModel, TEntity, TSettings>(TList model, Guid entityTypeId)
            where TList : BaseListModel<TModel>
            where TModel : BaseEntityModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            if(entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException("EntitytypeId doesn't exists");

            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<TEntity, TSettings>(entityTypeId);
            if (settings == null)
                throw new ArgumentNullException(typeof(TSettings).Name);

            if (settings.Enabled)
            {
                // need to add permission here
                model.UserInterface.ViewEnabled = await _accessControl.HasViewAccessAsync<TEntity>();
                model.UserInterface.CreateEnabled = await _accessControl.HasCreateAccessAsync<TEntity>();
                model.UserInterface.UpdateEnabled = await _accessControl.HasUpdateAccessAsync<TEntity>();
                model.UserInterface.DeleteEnabled = await _accessControl.HasDeleteAccessAsync<TEntity>();
                // settings based
                model.UserInterface.AutoGeneratedTemplate = settings.AutoGeneratedTemplate;
                model.UserInterface.RedirectUsingCodeEnabled = settings.RedirectByCodeEnabled;
            }

            return model;
        }

        #endregion

        #region View Component Mapping

        public TModel PrepareBaseEntityViewComponent<TModel, TEntity>(TModel model, TEntity entity)
            where TModel : BaseEntityModel
            where TEntity : BaseEntityType
        {
            var pluginAssemblies = PluginAssemblyHelper.GetAllModulesPluginAssemblies();
            if (pluginAssemblies.Any())
            {
                foreach (var assembly in pluginAssemblies)
                {
                    var pluginPortableView = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IPluginViewComponent)))
                        .FirstOrDefault();

                    if (pluginPortableView != null)
                    {
                        var pluginInstance = Activator.CreateInstance(pluginPortableView) as IPluginViewComponent;

                        if (pluginInstance != null)
                        {
                            // Ex. Inventory (referencing BusinessEntities)
                            var viewComponentList = pluginInstance.ManagePluginViewComponent();
                            if (viewComponentList.Any())
                            {
                                var entityComponentList = viewComponentList.Where(c => c.TargetEntity == typeof(TEntity).Name).ToList();
                                foreach (var component in entityComponentList)
                                {
                                    model.PluginComponents.Add(component);
                                }
                            }
                        }
                    }
                }
            }
            return model;
        }

        #endregion

        #region Methods
        private string GetEntityTypeNameFromSystemName(string entityTypeSystemName)
        {
            var entityTypeName = "";

            if (string.IsNullOrEmpty(entityTypeSystemName)) 
                return entityTypeName;

            entityTypeName = entityTypeSystemName.Substring(entityTypeSystemName.LastIndexOf(".") + 1);

            return entityTypeName;
        }

        #endregion
    }
}
