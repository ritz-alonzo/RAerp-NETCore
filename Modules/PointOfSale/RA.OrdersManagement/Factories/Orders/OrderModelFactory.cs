using AutoMapper;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using NuGet.Configuration;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.Services;
using RA.Categories.Services;
using RA.Core.Helpers;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Catalogs;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.FormTypes.Factories;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Services.Orders;
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

namespace RA.OrdersManagement.Factories.Orders
{
    public class OrderModelFactory : IOrderModelFactory
    {
        #region Constants
        private readonly IBaseFormModelFactory _baseFormModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly ICatalogService _catalogService;
        private readonly IOrderService _orderService;
        private readonly OrderSetting _orderSettings;
        private readonly ICategoryService _categoryService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        #endregion

        #region Ctor
        public OrderModelFactory(IBaseFormModelFactory baseFormModelFactory,
            IFormTypeManager formTypeManager,
            IMapper mapper,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            ICatalogService catalogService,
            IOrderService orderService,
            ICategoryService categoryService,
            IApplicationSettingService applicationSettingService)
        {
            _baseFormModelFactory = baseFormModelFactory;
            _formTypeManager = formTypeManager;
            _mapper = mapper;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _catalogService = catalogService;
            _orderService = orderService;
            _orderSettings = _formTypeManager.GetSettingDataOfFormAsync<Order, OrderSetting>().Result;
            _categoryService = categoryService;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
        }
        #endregion

        #region CRUD Form
        public virtual async Task<OrderSearchModel> PrepareOrderSearchModelAsync(OrderSearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseFormModelFactory.PrepareBaseFormSearchModel<OrderSearchModel, Order>(searchModel, pageSize, pageNumber);

            if (_orderSettings != null)
            {
                if (_orderSettings.MappedServiceTypeIds.HasAny())
                    searchModel.AvailableServiceTypes = await _catalogService.GetCatalogsSelectListAsync(_orderSettings.MappedServiceTypeIds, (int)CatalogType.Service);
            }

            searchModel.Items = await PrepareOrderListModelAsync(searchModel);
            searchModel.TotalItems = (int)searchModel.Items.TotalItems;
            searchModel.PageSize = pageSize;

            return searchModel;
        }

        public virtual async Task<OrderListModel> PrepareOrderListModelAsync(OrderSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            OrderListModel model = new OrderListModel();
            List<OrderModel> orderModelList = new List<OrderModel>();

            var orderList = await _orderService.GetOrderListAsync(
                searchQuery: searchModel.SearchQuery,
                searchCustomerName: searchModel.SearchCustomerName,
                searchServiceIds: searchModel.SearchServiceId.HasValue ? new List<Guid> { searchModel.SearchServiceId.Value } : null,
                searchOrderDate: searchModel.SearchOrderDate,
                searchCreatedDate: searchModel.SearchCreatedOn,
                formStatusIds: searchModel.SearchStatusId > 0 ? new List<int> { searchModel.SearchStatusId } : null, 
                pageNumber: searchModel.PageNumber, 
                pageSize: searchModel.PageSize
                );

            List<SelectListItem> availableServices = new List<SelectListItem>();
            if (_orderSettings != null)
            {
                if (_orderSettings.MappedServiceTypeIds.Any())
                    availableServices = await _catalogService.GetCatalogsSelectListAsync(_orderSettings.MappedServiceTypeIds, (int)CatalogType.Service);
            }

            orderModelList = orderList.Select(order =>
            {
                OrderModel orderModel = new OrderModel();
                orderModel = _mapper.Map(order, orderModel);

                var createdByUser = _userIdentity.GetUserDetailsAsync(order.CreatedById).Result;
                if (createdByUser != null)
                    orderModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);
                orderModel.AvailableServices = availableServices;

                if (orderModel.CreatedOn != DateTime.MinValue)
                    orderModel.CreatedOn = orderModel.CreatedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);

                if (orderModel.ModifiedOn.HasValue)
                    orderModel.ModifiedOn = orderModel.ModifiedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);

                return orderModel;

            }).ToList();

            await _baseFormModelFactory.PrepareBaseFormListModelAsync<OrderListModel, OrderModel, OrderSearchModel, Order, OrderSetting>(model, orderModelList, searchModel, orderModelList.Count());

            return model;
        }

        public virtual async Task<OrderModel> PrepareOrderModelAsync(OrderModel orderModel, Order orderForm)
        {
            if (orderModel == null)
                throw new ArgumentNullException(nameof(orderModel));

            if (orderForm == null)
            {
                orderForm = _orderService.CreateTempForm();
                orderModel.IsNewDoc = true;
                orderModel.Status = Core.PluginData.FormTypes.FormStatus.Pending;
            }

            orderModel = _mapper.Map(orderForm, orderModel);

            orderModel = await _baseFormModelFactory.PrepareBaseFormModelAsync(orderModel, orderForm, _orderSettings);

            if (_orderSettings != null)
            {
                if (_orderSettings.MappedServiceTypeIds.Any())
                    orderModel.AvailableServices = await _catalogService.GetCatalogsSelectListAsync(_orderSettings.MappedServiceTypeIds, (int)CatalogType.Service);

                if (_orderSettings.OpenDocOnCreate && orderModel.IsNewDoc)
                    orderModel.Status = Core.PluginData.FormTypes.FormStatus.Open;
            
            }

            orderModel.Items = await PrepareOrderItemListModelAsync(orderModel, 1);

            return orderModel;
        }

        #endregion

        #region CRUD Form Item

        public virtual async Task<OrderItemListModel> PrepareOrderItemListModelAsync(OrderModel orderModel, int pageNumber)
        {
            if (orderModel == null)
                throw new ArgumentNullException(nameof(orderModel));

            OrderItemListModel model = new OrderItemListModel();
            List<OrderItemModel> orderItemModelList = new List<OrderItemModel>();

            var formItemList = await _orderService.GetItemsByFormIdAsync(orderModel.Id);

            List<SelectListItem> availableCatalogs = new List<SelectListItem>();
            List<SelectListItem> availableServices = new List<SelectListItem>();
            if (_orderSettings != null)
            {
                if (_orderSettings.MappedCatalogTypeIds.Any())
                    availableCatalogs = await _catalogService.GetCatalogsSelectListAsync(_orderSettings.MappedCatalogTypeIds, (int)CatalogType.Product);
                if (_orderSettings.MappedServiceTypeIds.Any())
                    availableServices = await _catalogService.GetCatalogsSelectListAsync(_orderSettings.MappedServiceTypeIds, (int)CatalogType.Service);
            }

            orderItemModelList = formItemList.Select(formItem =>
            {
                OrderItemModel orderItemModel = new OrderItemModel();
                orderItemModel = _mapper.Map(formItem, orderItemModel);

                orderItemModel.AvailableCatalogs = availableCatalogs;
                
                if (formItem.CatalogId.IsNotNullOrEmpty())
                {
                    var catalog = _catalogService.GetByIdAsync(formItem.CatalogId).Result;
                    if (catalog != null)
                    {
                        orderItemModel.CatalogCode = catalog.Code;
                        if (catalog.UOMId.IsNotNullOrEmpty())
                        {
                            var category = _categoryService.GetByIdAsync(catalog.UOMId.Value).Result;
                            if (category != null)
                                orderItemModel.AvailableCategories = _categoryService.GetCategoriesSelectListAsync(new List<Guid> { category.EntityTypeId }).Result;
                        }
                    }
                }

                orderItemModel.CreatedOn = orderItemModel.CreatedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
                orderItemModel.ModifiedOn = orderItemModel.ModifiedOn.HasValue ? orderItemModel.ModifiedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : null;

                return orderItemModel;

            }).ToList();

            _baseFormModelFactory.PrepareBaseFormItemListModel(model, orderItemModelList, _orderSettings, pageNumber, orderItemModelList.Count());

            model.FormSettings.MappedCatalogTypeIds = _orderSettings?.MappedCatalogTypeIds ?? new List<Guid>();
            model.FormSettings.MappedServiceTypeIds = _orderSettings?.MappedServiceTypeIds ?? new List<Guid>();

            return model;
        }

        #endregion

        #region Configuration

        public virtual async Task<OrderConfigureModel> PrepareOrderConfigureModelAsync(string systemName)
        {
            OrderConfigureModel orderConfigureModel = new OrderConfigureModel();

            orderConfigureModel = await _baseFormModelFactory.PrepareBaseFormConfigureModelAsync<OrderConfigureModel, Order, OrderSetting>(orderConfigureModel);

            if (_orderSettings != null)
            {
                if (_orderSettings.MappedCatalogTypeIds.Any())
                {
                    orderConfigureModel.MappedCatalogTypeIds = _orderSettings.MappedCatalogTypeIds;
                }

                if (_orderSettings.MappedServiceTypeIds.Any())
                {
                    orderConfigureModel.MappedServiceTypeIds = _orderSettings.MappedServiceTypeIds;
                }
            }

            orderConfigureModel.AvailableCatalogTypes = await _catalogService.GetCatalogTypesSelectListAsync();
            orderConfigureModel.AvailableServiceTypes = await _catalogService.GetCatalogTypesSelectListAsync(type: CatalogType.Service);

            return orderConfigureModel;
        }

        #endregion
    }
}
