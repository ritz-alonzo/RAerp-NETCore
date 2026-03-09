using AutoMapper;
using RA.Core.Models.BaseModels;
using RA.Core.Models.PluginModels.FormTypes;
using RA.FormTypes.Helpers;
using RA.FormTypes.Services;
using RAerp.Factories.CoreFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Factories
{
    public class FormTypeModelFactory : IFormTypeModelFactory
    {
        #region Constants
        private readonly IBaseModelFactory _baseModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IMapper _mapper;
        #endregion

        #region Ctor
        public FormTypeModelFactory(IBaseModelFactory baseModelFactory, IFormTypeManager formTypeManager, IMapper mapper)
        {
            _baseModelFactory = baseModelFactory;
            _formTypeManager = formTypeManager;
            _mapper = mapper;
        }
        #endregion

        // prepare list of all plugins that inherits BaseForm
        // serves as settings configuration
        public virtual FormTypeSearchModel PrepareFormTypeSearchModel(FormTypeSearchModel searchModel, int pageNumber, int pageSize)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseModelFactory.PrepareBaseSearchModel(searchModel, pageSize, pageNumber);

            searchModel.FormTypes = PrepareFormTypeListModel(searchModel);
            searchModel.FormTypeName = "Form Types";

            // system name mapping for active menu
            searchModel.FormTypeSystemName = "RA.FormTypes";

            searchModel.TotalItems = (int)searchModel.FormTypes.TotalItems;
            searchModel.PageSize = searchModel.FormTypes.PageSize;
            searchModel.CurrentItemsShown = searchModel.FormTypes.Items.Count;

            return searchModel;
        }

        public virtual FormTypeListModel PrepareFormTypeListModel(FormTypeSearchModel searchModel)
        {
            var model = new FormTypeListModel();

            var formTypeList = FormTypeHelper.GetClassesOfBaseForm();
            var formTypes = new List<FormTypeModel>();

            formTypes = formTypeList.Select(formType =>
            {
                var formTypeModel = new FormTypeModel();
                formTypeModel.FormTypeName = formType.Name;
                formTypeModel.FormTypeSystemName = formType.FullName;
                formTypeModel.ModalConfigureEnabled = true;
                formTypeModel.PluginController = GeneratePluginControllerString(formType);
                formTypeModel.PluginConfigurationUrl = $"/{formTypeModel.PluginController}/Configuration";
                formTypeModel.Installed = true;
                return formTypeModel;

            }).ToList();

            _baseModelFactory.PrepareBaseListModel(model, formTypes, searchModel, formTypeList.Count());

            return model;
        }

        #region Methods

        private string GeneratePluginControllerString(Type formType)
        {
            var pluginController = "";
            var formTypeName = formType.Name;

            if (!string.IsNullOrEmpty(formTypeName))
            {
                var formTypeNameCharLength = formTypeName.Length;
                if (formTypeName.Substring(formTypeNameCharLength).Equals("y"))
                {
                    pluginController = formTypeName.Replace("y", "ies");
                }
                else
                {
                    pluginController = formTypeName + "s";
                }
            }

            return pluginController;
        }

        #endregion
    }
}
