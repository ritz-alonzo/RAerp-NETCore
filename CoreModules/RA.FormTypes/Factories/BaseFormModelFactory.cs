using AutoMapper;
using Microsoft.AspNetCore.Http;
using RA.Core.Helpers;
using RA.Core.Models.BaseModels;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Core.Models.PluginModels.FormTypes;
using RA.Core.Models.PortableViewModels;
using RA.Core.PluginData.FormTypes;
using RA.FormTypes.Data;
using RA.FormTypes.Domain;
using RA.FormTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Factories.CoreFactories;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.UserHelper;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Factories
{
    public class BaseFormModelFactory : BaseModelFactory, IBaseFormModelFactory
    {
        #region Constants
        private readonly IFormTypeManager _formTypeManager;
        private readonly IAccessControl _accessControl;
        private readonly IMapper _mapper;
        #endregion

        #region Ctor
        public BaseFormModelFactory(IUserIdentity userIdentity,
            IHttpContextAccessor httpContextAccessor,
            ISettingService settingService,
            IFormTypeManager formTypeManager,
            IAccessControl accessControl,
            IMapper mapper)
            : base(userIdentity, httpContextAccessor, settingService)
        {
            _formTypeManager = formTypeManager;
            _accessControl = accessControl;
            _mapper = mapper;
        }
        #endregion

        #region CRUD Form
        public TSearch PrepareBaseFormSearchModel<TSearch, TForm>(TSearch searchModel, int pageSize, int pageNumber)
            where TSearch : BaseFormSearchModel
            where TForm : BaseForm
        {
            searchModel = PrepareBaseSearchModel(searchModel, pageSize, pageNumber);
            searchModel.FormTypeSystemName = typeof(TForm).FullName;
            searchModel.FormTypeName = typeof(TForm).Name;
            searchModel.AvailableFormStatus = PluginDataHelper.EnumToSelectListItems<FormStatus>(showDefaultNoneValue: true);

            return searchModel;
        }

        public async Task<TList> PrepareBaseFormListModelAsync<TList, TModel, TSearch, TForm, TSettings>(TList list, List<TModel> listModel, TSearch searchModel, int totalItems)
            where TList : BaseListModel<TModel>
            where TModel : BaseFormModel
            where TSearch : BaseFormSearchModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            list = PrepareBaseListModel(list, listModel, searchModel, totalItems);
            // Preparation of UI Access Rights and Settings Model
            list = await PrepareBaseFormListModelUIAccessAsync<TList, TModel, TForm, TSettings>(list);

            return list;
        }

        public async Task<TModel> PrepareBaseFormModelAsync<TModel, TForm, TSettings>(TModel model, TForm form, TSettings settings)
            where TModel : BaseFormModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            model = await PrepareBaseModelAsync(model);
            model.FormTypeSystemName = typeof(TForm).FullName;
            model.FormTypeName = typeof(TForm).Name;
            if (settings != null)
                model.FormSettings = _mapper.Map(settings, model.FormSettings);
            // Preparation of UI Access Rights and Settings Model
            model = await PrepareBaseFormModelUIAccessAsync<TModel, TForm, TSettings>(model, form, settings);
            // Preparation of View Components
            model = PrepareBaseFormViewComponent<TModel, TForm>(model, form);

            return model;
        }

        public TItemListModel PrepareBaseFormItemListModel<TItemListModel, TItemModel, TSettings>(TItemListModel listModel, List<TItemModel> formItemModelList, TSettings settings, int pageNumber, int totalItems)
            where TItemListModel : BaseFormItemListModel<TItemModel>
            where TItemModel : BaseFormItemModel
            where TSettings : BaseFormSetting
        {
            if (settings != null)
                listModel.FormSettings = _mapper.Map(settings, listModel.FormSettings);

            #region Pagination
            if (formItemModelList.Count == 1)
                listModel.Items = formItemModelList.Take(settings.ItemsPageSize).ToList();
            else
                listModel.Items = formItemModelList.Skip((pageNumber - 1) * settings.ItemsPageSize).Take(settings.ItemsPageSize).ToList();

            var totalPages = (int)Math.Ceiling(totalItems / (double)settings.ItemsPageSize);

            if (pageNumber > 2)
                listModel.TotalItems = pageNumber;
            else
                listModel.TotalItems = totalPages;

            listModel.PageSize = settings.ItemsPageSize;
            listModel.PageNumber = pageNumber;
            #endregion

            return listModel;
        }
        #endregion

        #region Configuration

        public async Task<TConfig> PrepareBaseFormConfigureModelAsync<TConfig, TForm, TSettings>(TConfig configureModel)
            where TConfig : BaseFormConfigureModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var settings = await _formTypeManager.GetSettingDataOfFormAsync<TForm, TSettings>();
            if (settings == null)
            {
                configureModel.SystemName = typeof(TForm).FullName;
            }
            else
            {
                if (string.IsNullOrEmpty(settings.SystemName))
                    settings.SystemName = typeof(TForm).FullName;
                configureModel = _mapper.Map(settings, configureModel);
            }
            return configureModel;
        }

        #endregion

        #region User Interface Access

        public async Task<TModel> PrepareBaseFormModelUIAccessAsync<TModel, TForm, TSettings>(TModel model, TForm form, TSettings settings)
            where TModel : BaseFormModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            if (form == null)
                throw new ArgumentNullException(typeof(TForm).Name);

            if (settings == null)
                throw new ArgumentNullException(typeof(TSettings).Name);

            if (settings.Enabled && !form.Deleted)
            {
                // need to add permission here
                model.UserInterface.ViewEnabled = await _accessControl.HasViewAccessAsync<TForm>();
                model.UserInterface.CreateEnabled = await _accessControl.HasCreateAccessAsync<TForm>();
                model.UserInterface.UpdateEnabled = await _accessControl.HasUpdateAccessAsync<TForm>();
                model.UserInterface.DeleteEnabled = await _accessControl.HasDeleteAccessAsync<TForm>();
                // settings based
                model.UserInterface.AutoGeneratedTemplate = settings.AutoGeneratedFormNbr;
                model.UserInterface.RedirectUsingCodeEnabled = settings.RedirectUsingFormNbrEnabled;
                model.FormSettings.Enabled = settings.Enabled;
                model.FormSettings.AllowAddItemEnabled = settings.AllowAddItemEnabled;
                model.FormSettings.AllowEditDateEnabled = settings.AllowEditDateEnabled;
                model.FormSettings.InlineAddEnabled = settings.InlineAddEnabled;
                model.FormSettings.ApprovalEnabled = settings.ApprovalEnabled;
                model.FormSettings.OpenDocOnCreate = settings.OpenDocOnCreate;
                model.FormSettings.InventoryEnabled = settings.InventoryEnabled;
                model.FormSettings.RedirectUsingFormNbrEnabled = settings.RedirectUsingFormNbrEnabled;
                model.FormSettings.ItemsPageSize = settings.ItemsPageSize;
            }

            return model;
        }

        public async Task<TList> PrepareBaseFormListModelUIAccessAsync<TList, TModel, TForm, TSettings>(TList model)
            where TList : BaseListModel<TModel>
            where TModel : BaseFormModel
            where TForm : BaseForm
            where TSettings : BaseFormSetting
        {
            var settings = await _formTypeManager.GetSettingDataOfFormAsync<TForm, TSettings>();
            if (settings == null)
                throw new ArgumentNullException(typeof(TSettings).Name);

            if (settings.Enabled)
            {
                // need to add permission here
                model.UserInterface.ViewEnabled = await _accessControl.HasViewAccessAsync<TForm>();
                model.UserInterface.CreateEnabled = await _accessControl.HasCreateAccessAsync<TForm>();
                model.UserInterface.UpdateEnabled = await _accessControl.HasUpdateAccessAsync<TForm>();
                model.UserInterface.DeleteEnabled = await _accessControl.HasDeleteAccessAsync<TForm>();
                // settings based
                model.UserInterface.AutoGeneratedTemplate = settings.AutoGeneratedFormNbr;
                model.UserInterface.RedirectUsingCodeEnabled = settings.RedirectUsingFormNbrEnabled;
                model.FormSettings.Enabled = settings.Enabled;
                model.FormSettings.AllowAddItemEnabled = settings.AllowAddItemEnabled;
                model.FormSettings.AllowEditDateEnabled = settings.AllowEditDateEnabled;
                model.FormSettings.InlineAddEnabled = settings.InlineAddEnabled;
                model.FormSettings.ApprovalEnabled = settings.ApprovalEnabled;
                model.FormSettings.OpenDocOnCreate = settings.OpenDocOnCreate;
                model.FormSettings.InventoryEnabled = settings.InventoryEnabled;
                model.FormSettings.RedirectUsingFormNbrEnabled = settings.RedirectUsingFormNbrEnabled;
                model.FormSettings.ItemsPageSize = settings.ItemsPageSize;
            }

            return model;
        }

        #endregion

        #region View Component Mapping

        public TModel PrepareBaseFormViewComponent<TModel, TForm>(TModel model, TForm entity)
            where TModel : BaseFormModel
            where TForm : BaseForm
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
                                var entityComponentList = viewComponentList.Where(c => c.TargetEntity == typeof(TForm).Name).ToList();
                                model.PluginComponents = entityComponentList.Any() ? entityComponentList : new List<PluginViewComponentModel>();
                            }
                        }
                    }
                }
            }
            return model;
        }

        #endregion
    }
}
