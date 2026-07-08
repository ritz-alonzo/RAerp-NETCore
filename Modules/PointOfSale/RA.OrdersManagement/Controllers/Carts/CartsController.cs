using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RA.Catalogs.Services;
using RA.Core.Data;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.Core.PluginData.FormTypes;
using RA.FormTypes.Data;
using RA.FormTypes.Helpers;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Factories.Carts;
using RA.OrdersManagement.Services.Carts;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Controllers.Carts
{
    public class CartsController : AdminController
    {
        #region Constants
        private readonly ICartService _cartService;
        private readonly ICartModelFactory _cartModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly CartSetting _cartSettings;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Ctor
        public CartsController(ICartService cartService,
            ICartModelFactory cartModelFactory,
            IFormTypeManager formTypeManager,
            IMapper mapper,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            ICatalogService catalogService)
        {
            _cartService = cartService;
            _cartModelFactory = cartModelFactory;
            _formTypeManager = formTypeManager;
            _mapper = mapper;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _cartSettings = _formTypeManager.GetSettingDataOfFormAsync<Cart, CartSetting>().Result;
            _catalogService = catalogService;
        }
        #endregion

        #region Configuration
        [HttpGet]
        public async Task<IActionResult> Configuration(string systemName)
        {
            if (string.IsNullOrEmpty(systemName))
                return JsonError(FormTypeMessages.FormTypeSystemNameNotExists);

            if (!_accessControl.HasSuperAdminAccessAsync().Result)
                return UnauthorizedAccess();

            var cartConfigureModel = await _cartModelFactory.PrepareCartConfigureModelAsync(systemName);

            return View(cartConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(CartConfigureModel cartConfigureModel)
        {
            if (cartConfigureModel == null)
                return JsonError(FormTypeMessages.FormTypeSystemNameNotExists);

            if (!_accessControl.HasSuperAdminAccessAsync().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                await _formTypeManager.InsertFormSettingAsync<Cart, CartSetting>();

            var settings = _mapper.Map(cartConfigureModel, _cartSettings);

            await _formTypeManager.UpdateSettingDataOfFormAsync<Cart, CartSetting>(settings);

            return NullJsonResult();
        }
        #endregion

        #region CRUD Form
        public async Task<IActionResult> List(int page = 1)
        {
            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled)
                return NotFound();

            var model = await _cartModelFactory.PrepareCartSearchModelAsync(new CartSearchModel(), _cartSettings.ItemsPageSize, page);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CartListSearch(CartSearchModel searchModel)
        {
            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled)
                return NotFound();

            var model = await _cartModelFactory.PrepareCartListModelAsync(searchModel);

            return PartialView(model);
        }

        public async Task<IActionResult> Index(Guid formId)
        {
            if (formId.IsNullOrEmpty())
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled)
                return NotFound();

            var cartForm = await _cartService.GetFormByIdAsync(formId);
            if (cartForm == null)
                return NotFound();

            var model = await _cartModelFactory.PrepareCartModelAsync(new CartModel(), cartForm);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled)
                return NotFound();

            var model = await _cartModelFactory.PrepareCartModelAsync(new CartModel(), null);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CartModel model)
        {
            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled)
                return NotFound();

            if (ModelState.IsValid)
            {
                var cartForm = _mapper.Map<Cart>(model);

                cartForm.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                var orderItems = await _cartService.GetItemsByFormIdAsync(cartForm.Id);

                #region Total Computations
                cartForm.TotalQty = orderItems.Sum(c => c.Qty);
                cartForm.TotalAmount = orderItems.Sum(c => c.SubTotal);
                #endregion

                await _cartService.InsertFormAsync(cartForm);
                // sanity check
                model.Id = cartForm.Id;

                SuccessNotification(model, "Successfully created Cart");
            }
            else
            {
                ErrorNotification(model, "Failed to create Cart");
                return View("Create");
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CartModel model, string actionType)
        {
            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled)
                return NotFound();

            if (ModelState.IsValid)
            {
                var cartForm = _mapper.Map<Cart>(model);
                switch (actionType)
                {
                    case FormTypeAction.Approve:
                        if (_cartSettings.ApprovalEnabled)
                            cartForm.Status = FormStatus.Approved;
                        break;

                    case FormTypeAction.Release:
                        cartForm.Status = FormStatus.Open;
                        break;

                    case FormTypeAction.Cancel:
                        cartForm.Status = FormStatus.Cancelled;
                        break;

                    default:
                        break;
                }

                // for enum with int value mapping
                //entity.TypeId = model.TypeId;
                var cartItems = await _cartService.GetItemsByFormIdAsync(cartForm.Id);

                #region Total Computations
                cartForm.TotalQty = cartItems.Sum(c => c.Qty);
                cartForm.TotalAmount = cartItems.Sum(c => c.SubTotal);
                #endregion

                cartForm.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _cartService.UpdateFormAsync(cartForm);

                SuccessNotification(model, "Successfully updated Cart");
            }
            else
            {
                ErrorNotification(model, "Failed to update Cart");
                return View("Index", new { id = model.Id });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled)
                return NotFound();

            var cartForm = await _cartService.GetFormByIdAsync(id);
            if (cartForm == null)
                return NotFound();

            await _cartService.DeleteFormAsync(cartForm);
            SuccessNotification(new CartModel() { Id = cartForm.Id }, "Successfully deleted Cart");

            return RedirectToAction("List");
        }
        #endregion

        #region CRUD Form Item
        public async Task<IActionResult> CartItemList(CartModel cartModel, int page = 1)
        {
            if (cartModel == null)
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return NotFound();

            if (!_cartSettings.Enabled || !_cartSettings.AllowAddItemEnabled)
                return NotFound();

            var itemModel = await _cartModelFactory.PrepareCartItemListModelAsync(cartModel, page);

            return PartialView("/Views/Carts/_CreateAndEdit.Items.cshtml", itemModel);
        }

        public async Task<IActionResult> InsertItem(Guid formId, List<Guid> catalogIds)
        {
            if (!catalogIds.HasAny())
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return JsonError("Cart form settings not yet configured");

            if (!_cartSettings.Enabled || !_cartSettings.AllowAddItemEnabled)
                return JsonError("Cart form settings not yet enabled");

            foreach (var catalogId in catalogIds)
            {
                var catalog = await _catalogService.GetByIdAsync(catalogId);
                if (catalog == null)
                    continue;

                var cartItem = new CartItem();
                cartItem.FormId = formId;
                cartItem.CatalogId = catalogId;
                cartItem.CategoryId = catalog.UOMId;
                cartItem.Description = catalog.Description;
                cartItem.Qty = 0m;
                cartItem.Price = catalog.Price;
                cartItem.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                cartItem.CreatedOn = DateTime.UtcNow;

                await _cartService.InsertTempItemAsync(cartItem, DataChangeStatus.Insert);
            }

            return NullJsonResult();
        }

        public async Task<IActionResult> EditItem(CartItemModel itemModel)
        {
            if (itemModel == null)
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return JsonError("Cart form settings not yet configured");

            if (!_cartSettings.Enabled || !_cartSettings.AllowAddItemEnabled)
                return JsonError("Cart form settings not yet enabled");

            var cartItem = _mapper.Map<CartItem>(itemModel);
            cartItem.SubTotal = cartItem.Qty * cartItem.Price;

            await _cartService.InsertTempItemAsync(cartItem, DataChangeStatus.Update);

            return NullJsonResult();
        }

        public async Task<IActionResult> DeleteItem(CartItemModel itemModel)
        {
            if (!_accessControl.HasViewAccessAsync<Cart>().Result)
                return UnauthorizedAccess();

            if (_cartSettings == null)
                return JsonError("Cart form settings not yet configured");

            if (!_cartSettings.Enabled || !_cartSettings.AllowAddItemEnabled)
                return JsonError("Cart form settings not yet enabled");

            var cartItem = _mapper.Map<CartItem>(itemModel);

            await _cartService.InsertTempItemAsync(cartItem, DataChangeStatus.Delete);

            return NullJsonResult();
        }
        #endregion
    }
}
