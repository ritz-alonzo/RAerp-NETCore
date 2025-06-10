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
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IAccessControl _accessControl;
        public BaseEntityModelFactory(
            IUserIdentity userIdentity,
            IHttpContextAccessor httpContextAccessor,
            ISettingService settingService,
            IEntityTypeManager entityTypeManager,
            IAccessControl accessControl)
            : base(userIdentity, httpContextAccessor, settingService)
        {
            _entityTypeManager = entityTypeManager;
            _accessControl = accessControl;
        }

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

            return searchModel;
        }

        /// <summary>
        /// Preparing base model of entity
        /// will add settings here
        /// </summary>
        /// <typeparam name="TModel"></typeparam>
        /// <param name="model"></param>
        /// <returns></returns>
        public TModel PrepareBaseEntityModel<TModel, TEntity, TSettings>(TModel model, TEntity entity, TSettings settings)
            where TModel : BaseEntityModel
            where TSettings : BaseEntityTypeSetting
            where TEntity : BaseEntityType
        {
            model = PrepareBaseModel(model);
            model.EntityTypeSystemName = entity.EntitySystemName;
            model.EntityTypeName = GetEntityTypeNameFromEntity(entity.EntitySystemName);

            // Preparation of UI Access Rights and Settings Model
            model = PrepareBaseEntityModelUIAccess<TModel, TEntity, TSettings>(model, entity, settings);

            model = PrepareBaseEntityPortableView<TModel, TEntity>(model, entity);

            return model;
        }

        public TListModel PrepareBaseEntityListModel<TListModel, TModelList, TSearch>(TListModel list, List<TModelList> listModel, TSearch searchModel, int totalItems)
            where TListModel : BaseListModel<TModelList>
            where TModelList : BaseEntityModel
            where TSearch : BaseEntitySearchModel
        {
            list = PrepareBaseListModel(list, listModel, searchModel, totalItems);

            return list;
        }

        #endregion

        #region Configuration

        public TModel PrepareBaseEntityConfigureModel<TModel, TEntity, TSettings>(TModel configureModel, Guid entityTypeId, string entityTypeSystemName = null)
            where TModel : BaseEntityConfigureModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(TEntity));

            var settings = _entityTypeManager.GetSettingDataOfEntity<TEntity, TSettings>(entityTypeId, entityTypeSystemName).Result;

            configureModel.EntityTypeId = entityTypeId;
            configureModel.Installed = _entityTypeManager.GetById(entityTypeId).Result.Installed;

            if (settings == null)
            {
                if (!string.IsNullOrEmpty(entityTypeSystemName))
                    configureModel.SystemName = entityTypeSystemName;
            }
            else
            {
                if (!string.IsNullOrEmpty(entityTypeSystemName) && string.IsNullOrEmpty(settings.SystemName))
                    settings.SystemName = entityTypeSystemName;
                configureModel.SystemName = settings.SystemName;
                configureModel.Template = settings.Template;
                configureModel.AutoGeneratedTemplate = settings.AutoGeneratedTemplate;
                configureModel.TemplateIncrementCount = settings.TemplateIncrementCount;
                configureModel.TemplateCount = settings.TemplateCount;
                configureModel.Installed = settings.Installed;
                configureModel.Enabled = settings.Enabled;
                configureModel.CreateEnabled = settings.CreateEnabled;
                configureModel.DeleteEnabled = settings.DeleteEnabled;
                configureModel.UpdateEnabled = settings.UpdateEnabled;
                configureModel.AddressEnabled = settings.AddressEnabled;
            }
            return configureModel;
        }

        #endregion

        #region User Interface Access

        public TModel PrepareBaseEntityModelUIAccess<TModel, TEntity, TSettings>(TModel model, TEntity entityType, TSettings settings)
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
                model.UserInterface.ViewEnabled = _accessControl.HasViewAccess<TEntity>().Result;
                model.UserInterface.CreateEnabled = _accessControl.HasCreateAccess<TEntity>().Result;
                model.UserInterface.UpdateEnabled = _accessControl.HasUpdateAccess<TEntity>().Result;
                model.UserInterface.DeleteEnabled = _accessControl.HasDeleteAccess<TEntity>().Result;
                // settings based
                model.UserInterface.AutoGeneratedTemplate = settings.AutoGeneratedTemplate;
                model.Enabled = settings.Enabled;
                model.AddressEnabled = settings.AddressEnabled;
            }
            
            return model;
        }

        public TList PrepareBaseEntityListModelUIAccess<TList, TModel, TEntity, TSettings>(TList model, Guid entityTypeId)
            where TList : BaseListModel<TModel>
            where TModel : BaseEntityModel
            where TEntity : BaseEntityType
            where TSettings : BaseEntityTypeSetting
        {
            if(entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException("EntitytypeId doesn't exists");

            var settings = _entityTypeManager.GetSettingDataOfEntity<TEntity, TSettings>(entityTypeId).Result;
            if (settings == null)
                throw new ArgumentNullException(typeof(TSettings).Name);

            if (settings.Enabled)
            {
                // need to add permission here
                model.UserInterface.ViewEnabled = _accessControl.HasViewAccess<TEntity>().Result;
                model.UserInterface.CreateEnabled = _accessControl.HasCreateAccess<TEntity>().Result;
                model.UserInterface.UpdateEnabled = _accessControl.HasUpdateAccess<TEntity>().Result;
                model.UserInterface.DeleteEnabled = _accessControl.HasDeleteAccess<TEntity>().Result;
                // settings based
                model.UserInterface.AutoGeneratedTemplate = settings.AutoGeneratedTemplate;
            }

            return model;
        }

        #endregion

        #region Portable View Mapping

        public TModel PrepareBaseEntityPortableView<TModel, TEntity>(TModel model, TEntity entity)
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
                                model.PluginComponents = entityComponentList.Any() ? entityComponentList : new List<PluginViewComponentModel>();
                            }
                        }
                    }
                }
            }
            return model;
        }

        #endregion

        #region Methods
        private string GetEntityTypeNameFromEntity(string entityTypeSystemName)
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
