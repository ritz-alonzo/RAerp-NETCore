using AutoMapper;
using RA.Catalogs.Services;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.FormTypes.Factories;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Services.Carts;
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

namespace RA.OrdersManagement.Factories.Carts
{
    public class CartModelFactory : ICartModelFactory
    {
        #region Constants
        private readonly IBaseFormModelFactory _baseFormModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly ICatalogService _catalogService;
        private readonly ICartService _cartService;
        private readonly CartSetting _cartSettings;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        #endregion

        #region Ctor
        public CartModelFactory(IBaseFormModelFactory baseFormModelFactory,
            IFormTypeManager formTypeManager,
            IMapper mapper,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            ICatalogService catalogService,
            ICartService cartService,
            IApplicationSettingService applicationSettingService)
        {
            _baseFormModelFactory = baseFormModelFactory;
            _formTypeManager = formTypeManager;
            _mapper = mapper;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _catalogService = catalogService;
            _cartService = cartService;
            _cartSettings = _formTypeManager.GetSettingDataOfFormAsync<Cart, CartSetting>().Result;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
        }
        #endregion

        #region CRUD Form
        public virtual async Task<CartSearchModel> PrepareCartSearchModelAsync(CartSearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            _baseFormModelFactory.PrepareBaseFormSearchModel<CartSearchModel, Cart>(searchModel, pageSize, pageNumber);

            if (_cartSettings != null)
            {
                if (_cartSettings.MappedServiceTypeIds.HasAny())
                    searchModel.AvailableServiceTypes = await _catalogService.GetCatalogsSelectListAsync(_cartSettings.MappedServiceTypeIds, (int)CatalogType.Service);
            }

            searchModel.Items = await PrepareCartListModelAsync(searchModel);
            searchModel.TotalItems = (int)searchModel.Items.TotalItems;
            searchModel.PageSize = pageSize;

            return searchModel;
        }

        public virtual async Task<CartListModel> PrepareCartListModelAsync(CartSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            CartListModel model = new CartListModel();
            List<CartModel> cartModelList = new List<CartModel>();

            var cartList = await _cartService.GetCartListAsync(
                searchQuery: searchModel.SearchQuery,
                searchCustomerName: searchModel.SearchCustomerName,
                searchServiceIds: searchModel.SearchServiceId.HasValue ? new List<Guid> { searchModel.SearchServiceId.Value } : null,
                searchCreatedOn: searchModel.SearchCreatedOn,
                formStatusIds: searchModel.SearchStatusId > 0 ? new List<int> { searchModel.SearchStatusId } : null,
                pageNumber: searchModel.PageNumber,
                pageSize: searchModel.PageSize
                );

            cartModelList = cartList.Select(cart =>
            {
                CartModel cartModel = new CartModel();
                cartModel = _mapper.Map(cart, cartModel);

                var createdByUser = _userIdentity.GetUserDetailsAsync(cart.CreatedById).Result;
                if (createdByUser != null)
                    cartModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);

                if (_cartSettings != null)
                {
                    if (_cartSettings.MappedServiceTypeIds.Any())
                        cartModel.AvailableServices = _catalogService.GetCatalogsSelectListAsync(_cartSettings.MappedServiceTypeIds, (int)CatalogType.Service).Result;
                }

                if (cartModel.CreatedOn != DateTime.MinValue)
                    cartModel.CreatedOn = cartModel.CreatedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);

                if (cartModel.ModifiedOn.HasValue)
                    cartModel.ModifiedOn = cartModel.ModifiedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);

                return cartModel;

            }).ToList();

            await _baseFormModelFactory.PrepareBaseFormListModelAsync<CartListModel, CartModel, CartSearchModel, Cart, CartSetting>(model, cartModelList, searchModel, cartModelList.Count());

            return model;
        }

        public virtual async Task<CartModel> PrepareCartModelAsync(CartModel cartModel, Cart cartForm)
        {
            if (cartModel == null)
                throw new ArgumentNullException(nameof(cartModel));

            if (cartForm == null)
            {
                cartForm = _cartService.CreateTempForm();
            }

            cartModel = _mapper.Map(cartForm, cartModel);

            cartModel = await _baseFormModelFactory.PrepareBaseFormModelAsync(cartModel, cartForm, _cartSettings);

            if (_cartSettings != null)
            {
                if (_cartSettings.MappedServiceTypeIds.Any())
                    cartModel.AvailableServices = await _catalogService.GetCatalogsSelectListAsync(_cartSettings.MappedServiceTypeIds, (int)CatalogType.Service);
            }

            cartModel.Items = await PrepareCartItemListModelAsync(cartModel, 1);

            return cartModel;
        }

        #endregion

        #region CRUD Form Item

        public virtual async Task<CartItemListModel> PrepareCartItemListModelAsync(CartModel cartModel, int pageNumber)
        {
            if (cartModel == null)
                throw new ArgumentNullException(nameof(cartModel));

            CartItemListModel model = new CartItemListModel();
            List<CartItemModel> cartItemModelList = new List<CartItemModel>();

            var formItemList = await _cartService.GetItemsByFormIdAsync(cartModel.Id);

            cartItemModelList = formItemList.Select(formItem =>
            {
                CartItemModel cartItemModel = new CartItemModel();
                cartItemModel = _mapper.Map(formItem, cartItemModel);
                cartItemModel.CreatedOn = cartItemModel.CreatedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
                cartItemModel.ModifiedOn = cartItemModel.ModifiedOn.HasValue ? cartItemModel.ModifiedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : null;
                return cartItemModel;

            }).ToList();

            _baseFormModelFactory.PrepareBaseFormItemListModel(model, cartItemModelList, _cartSettings, pageNumber, cartItemModelList.Count());

            return model;
        }

        #endregion

        #region Configuration

        public virtual async Task<CartConfigureModel> PrepareCartConfigureModelAsync(string systemName)
        {
            CartConfigureModel cartConfigureModel = new CartConfigureModel();

            cartConfigureModel = await _baseFormModelFactory.PrepareBaseFormConfigureModelAsync<CartConfigureModel, Cart, CartSetting>(cartConfigureModel);

            if (_cartSettings != null)
            {
                if (_cartSettings.MappedCatalogTypeIds.Any())
                {
                    cartConfigureModel.MappedCatalogTypeIds = _cartSettings.MappedCatalogTypeIds;
                }

                if (_cartSettings.MappedServiceTypeIds.Any())
                {
                    cartConfigureModel.MappedServiceTypeIds = _cartSettings.MappedServiceTypeIds;
                }
            }

            cartConfigureModel.AvailableCatalogTypes = await _catalogService.GetCatalogTypesSelectListAsync();
            cartConfigureModel.AvailableServiceTypes = await _catalogService.GetCatalogTypesSelectListAsync(type: CatalogType.Service);

            return cartConfigureModel;
        }

        #endregion
    }
}
