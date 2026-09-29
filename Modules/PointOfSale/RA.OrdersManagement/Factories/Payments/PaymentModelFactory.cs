using AutoMapper;
using RA.Catalogs.Services;
using RA.Categories.Services;
using RA.Core.Helpers;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.Core.Models.PluginModels.OrdersManagement.Payments;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.Core.PluginData.FormTypes.OrdersManagement.Payments;
using RA.FormTypes.Factories;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Payments;
using RA.OrdersManagement.Services.Payments;
using RA.WebFramework.Extensions;
using RAerp.Domain.Application;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Factories.Payments
{
    public class PaymentModelFactory : IPaymentModelFactory
    {
        #region Constants
        private readonly IBaseFormModelFactory _baseFormModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly ICatalogService _catalogService;
        private readonly IPaymentService _paymentService;
        private readonly PaymentSetting _paymentSettings;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly ICategoryService _categoryService;
        #endregion

        #region Ctor
        public PaymentModelFactory(IBaseFormModelFactory baseFormModelFactory,
            IFormTypeManager formTypeManager,
            IMapper mapper,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            ICatalogService catalogService,
            IPaymentService paymentService,
            IApplicationSettingService applicationSettingService,
            ICategoryService categoryService)
        {
            _baseFormModelFactory = baseFormModelFactory;
            _formTypeManager = formTypeManager;
            _mapper = mapper;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _catalogService = catalogService;
            _paymentService = paymentService;
            _paymentSettings = _formTypeManager.GetSettingDataOfFormAsync<Payment, PaymentSetting>().Result;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _categoryService = categoryService;
        }
        #endregion

        #region CRUD Form
        public virtual async Task<PaymentSearchModel> PreparePaymentSearchModelAsync(PaymentSearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseFormModelFactory.PrepareBaseFormSearchModel<PaymentSearchModel, Payment>(searchModel, pageSize, pageNumber);

            searchModel.AvailablePaymentStatus = PluginDataHelper.EnumToSelectListItems<PaymentStatus>(showDefaultNoneValue: true);

            searchModel.Items = await PreparePaymentListModelAsync(searchModel);
            searchModel.TotalItems = (int)searchModel.Items.TotalItems;
            searchModel.PageSize = pageSize;

            return searchModel;
        }

        public virtual async Task<PaymentListModel> PreparePaymentListModelAsync(PaymentSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            PaymentListModel model = new PaymentListModel();
            List<PaymentModel> paymentModelList = new List<PaymentModel>();

            var paymentList = await _paymentService.GetPaymentListAsync(
                searchQuery: searchModel.SearchQuery,
                searchPaymentOrderNbr: searchModel.SearchOrderNbr,
                searchPaymentRefNbr: searchModel.SearchPaymentRefNbr,
                searchPaymentDate: searchModel.SearchPaymentDate,
                paymentStatusIds: searchModel.SearchPaymentStatusId > 0 ? new List<int> { searchModel.SearchStatusId } : null,
                searchCreatedDate: searchModel.SearchCreatedOn,
                formStatusIds: searchModel.SearchStatusId > 0 ? new List<int> { searchModel.SearchStatusId } : null,
                pageNumber: searchModel.PageNumber,
                pageSize: searchModel.PageSize
                );

            paymentModelList = paymentList.Select(payment =>
            {
                PaymentModel paymentModel = new PaymentModel();
                paymentModel = _mapper.Map(payment, paymentModel);

                var createdByUser = _userIdentity.GetUserDetailsAsync(payment.CreatedById).Result;
                if (createdByUser != null)
                    paymentModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);
                paymentModel.CreatedOn = paymentModel.CreatedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
                paymentModel.ModifiedOn = paymentModel.ModifiedOn.HasValue ? paymentModel.ModifiedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : null;
                return paymentModel;

            }).ToList();

            await _baseFormModelFactory.PrepareBaseFormListModelAsync<PaymentListModel, PaymentModel, PaymentSearchModel, Payment, PaymentSetting>(model, paymentModelList, searchModel, paymentModelList.Count());

            return model;
        }

        public virtual async Task<PaymentModel> PreparePaymentModelAsync(PaymentModel paymentModel, Payment paymentForm)
        {
            if (paymentModel == null)
                throw new ArgumentNullException(nameof(paymentModel));

            if (paymentForm == null)
            {
                paymentForm = _paymentService.CreateTempForm();
            }

            paymentModel = _mapper.Map(paymentForm, paymentModel);

            paymentModel = await _baseFormModelFactory.PrepareBaseFormModelAsync(paymentModel, paymentForm, _paymentSettings);
            // should auto populate grid upon selecting order no 
            paymentModel.Items = await PreparePaymentItemListModelAsync(paymentModel, 1);

            return paymentModel;
        }

        #endregion

        #region CRUD Form Item

        public virtual async Task<PaymentItemListModel> PreparePaymentItemListModelAsync(PaymentModel paymentModel, int pageNumber)
        {
            if (paymentModel == null)
                throw new ArgumentNullException(nameof(paymentModel));

            PaymentItemListModel model = new PaymentItemListModel();
            List<PaymentItemModel> paymentItemModelList = new List<PaymentItemModel>();

            var formItemList = await _paymentService.GetItemsByFormIdAsync(paymentModel.Id);

            paymentItemModelList = formItemList.Select(formItem =>
            {
                PaymentItemModel paymentItemModel = new PaymentItemModel();
                paymentItemModel = _mapper.Map(formItem, paymentItemModel);
                paymentItemModel.CreatedOn = paymentItemModel.CreatedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
                paymentItemModel.ModifiedOn = paymentItemModel.ModifiedOn.HasValue ? paymentItemModel.ModifiedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : null;
                return paymentItemModel;

            }).ToList();

            _baseFormModelFactory.PrepareBaseFormItemListModel(model, paymentItemModelList, _paymentSettings, pageNumber, paymentItemModelList.Count());

            return model;
        }

        #endregion

        #region Configuration

        public virtual async Task<PaymentConfigureModel> PreparePaymentConfigureModelAsync(string systemName)
        {
            PaymentConfigureModel paymentConfigureModel = new PaymentConfigureModel();

            paymentConfigureModel = await _baseFormModelFactory.PrepareBaseFormConfigureModelAsync<PaymentConfigureModel, Payment, PaymentSetting>(paymentConfigureModel);

            if (_paymentSettings != null)
            {
                if (_paymentSettings.MappedCatalogTypeIds.Any())
                {
                    paymentConfigureModel.MappedCatalogTypeIds = _paymentSettings.MappedCatalogTypeIds;
                }

                if (_paymentSettings.MappedCategoryTypeIds.Any())
                {
                    paymentConfigureModel.MappedCategoryTypeIds = _paymentSettings.MappedCategoryTypeIds;
                }
            }
            paymentConfigureModel.AvailableCatalogTypes = await _catalogService.GetCatalogTypesSelectListAsync();
            paymentConfigureModel.AvailableCategoryTypes = await _categoryService.GetCategoryTypesSelectListAsync();

            return paymentConfigureModel;
        }

        #endregion
    }
}
