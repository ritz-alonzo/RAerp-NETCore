using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.Services;
using RA.Core.Data;
using RA.Core.Models.PluginModels.Catalogs;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.Core.PluginData.FormTypes;
using RA.FormTypes.Data;
using RA.FormTypes.Helpers;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Factories.Orders;
using RA.OrdersManagement.Services.Orders;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Controllers.Orders
{
    public class OrdersController : AdminController
    {
        #region Constants
        private readonly IOrderService _orderService;
        private readonly IOrderModelFactory _orderModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly OrderSetting _orderSettings;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Ctor
        public OrdersController(IOrderService orderService,
            IOrderModelFactory orderModelFactory,
            IFormTypeManager formTypeManager,
            IMapper mapper,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            ICatalogService catalogService)
        {
            _orderService = orderService;
            _orderModelFactory = orderModelFactory;
            _formTypeManager = formTypeManager;
            _mapper = mapper;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _orderSettings = _formTypeManager.GetSettingDataOfFormAsync<Order, OrderSetting>().Result;
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

            var orderConfigureModel = await _orderModelFactory.PrepareOrderConfigureModelAsync(systemName);

            return View("~/Plugins/RA.OrdersManagement/Views/Orders/Configuration.cshtml", orderConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(OrderConfigureModel orderConfigureModel)
        {
            if (orderConfigureModel == null)
                return JsonError(FormTypeMessages.FormTypeSystemNameNotExists);

            if (!_accessControl.HasSuperAdminAccessAsync().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                await _formTypeManager.InsertFormSettingAsync<Order, OrderSetting>();

            var settings = _mapper.Map(orderConfigureModel, _orderSettings);

            await _formTypeManager.UpdateSettingDataOfFormAsync<Order, OrderSetting>(settings);

            return NullJsonResult();
        }
        #endregion

        #region CRUD Form
        public async Task<IActionResult> List(int page = 1)
        {
            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled)
                return NotFound();

            var model = await _orderModelFactory.PrepareOrderSearchModelAsync(new OrderSearchModel(), _orderSettings.ItemsPageSize, page);

            return View("~/Plugins/RA.OrdersManagement/Views/Orders/List.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> OrderListSearch(OrderSearchModel searchModel)
        {
            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled)
                return NotFound();

            var model = await _orderModelFactory.PrepareOrderListModelAsync(searchModel);

            return PartialView("~/Plugins/RA.OrdersManagement/Views/Orders/_OrderListSearch.cshtml", model);
        }

        public async Task<IActionResult> Index(Guid formId)
        {
            if (formId.IsNullOrEmpty())
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled)
                return NotFound();

            var orderForm = await _orderService.GetFormByIdAsync(formId);
            if (orderForm == null)
                return NotFound();

            var model = await _orderModelFactory.PrepareOrderModelAsync(new OrderModel(), orderForm);

            return View("~/Plugins/RA.OrdersManagement/Views/Orders/Index.cshtml", model);
        }

        public async Task<IActionResult> Create()
        {
            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled)
                return NotFound();

            var model = await _orderModelFactory.PrepareOrderModelAsync(new OrderModel(), null);

            return View("~/Plugins/RA.OrdersManagement/Views/Orders/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OrderModel model)
        {
            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled)
                return NotFound();

            if (ModelState.IsValid)
            {
                var orderForm = _mapper.Map<Order>(model);

                orderForm.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                var orderItems = await _orderService.GetItemsByFormIdAsync(orderForm.Id);

                #region Total Computations
                orderForm.TotalQty = orderItems.Sum(c => c.Qty);
                orderForm.TotalDiscountAmount = orderItems.Sum(c => c.DiscountAmount);
                orderForm.TotalGrossAmount = orderItems.Sum(c => c.SubTotal);
                orderForm.TotalVatAmount = orderForm.TotalGrossAmount * 0.12m;
                orderForm.TotalNetAmount = orderForm.TotalGrossAmount * 1.12m;
                #endregion

                await _orderService.InsertFormAsync(orderForm);
                // sanity check
                model.Id = orderForm.Id;

                SuccessNotification(model, "Successfully created Order");
            }
            else
            {
                ErrorNotification(model, "Failed to create Order");
                return View("Create");
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(OrderModel model, string actionType)
        {
            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled)
                return NotFound();

            if (ModelState.IsValid)
            {
                var orderForm = _mapper.Map<Order>(model);
                switch (actionType)
                {
                    case FormTypeAction.Approve:
                        if (_orderSettings.ApprovalEnabled)
                            orderForm.Status = FormStatus.Approved;
                        break;

                    case FormTypeAction.Release:
                        orderForm.Status = FormStatus.Open;
                        break;

                    case FormTypeAction.Cancel:
                        orderForm.Status = FormStatus.Cancelled;
                        break;

                    default:
                        break;
                }

                // for enum with int value mapping
                //entity.TypeId = model.TypeId;
                var orderItems = await _orderService.GetItemsByFormIdAsync(orderForm.Id);

                #region Total Computations
                orderForm.TotalQty = orderItems.Sum(c => c.Qty);
                orderForm.TotalDiscountAmount = orderItems.Sum(c => c.DiscountAmount);
                orderForm.TotalGrossAmount = orderItems.Sum(c => c.SubTotal);
                orderForm.TotalVatAmount = orderForm.TotalGrossAmount * 0.12m;
                orderForm.TotalNetAmount = orderForm.TotalGrossAmount * 1.12m;
                #endregion

                orderForm.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _orderService.UpdateFormAsync(orderForm);

                SuccessNotification(model, "Successfully updated Order");
            }
            else
            {
                ErrorNotification(model, "Failed to update Order");
                return View("Index", new { id = model.Id });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled)
                return NotFound();

            var orderForm = await _orderService.GetFormByIdAsync(id);
            if (orderForm == null)
                return NotFound();

            await _orderService.DeleteFormAsync(orderForm);
            SuccessNotification(new OrderModel() { Id = orderForm.Id }, "Successfully deleted Order");

            return RedirectToAction("List");
        }
        #endregion

        #region CRUD Form Item
        public async Task<IActionResult> OrderItemList(OrderModel orderModel, int page = 1)
        {
            if (orderModel == null)
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return NotFound();

            if (!_orderSettings.Enabled || !_orderSettings.AllowAddItemEnabled)
                return NotFound();

            var itemModel = await _orderModelFactory.PrepareOrderItemListModelAsync(orderModel, page);
            
            return PartialView("~/Plugins/RA.OrdersManagement/Views/Orders/_CreateAndEdit.Items.cshtml", itemModel);
        }

        public async Task<IActionResult> InsertItem(Guid formId, List<Guid> catalogIds)
        {
            if (!catalogIds.HasAny())
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return JsonError("Order form settings not yet configured");

            if (!_orderSettings.Enabled || !_orderSettings.AllowAddItemEnabled)
                return JsonError("Order form settings not yet enabled");

            int lineNbr = 1;
            
            var existingOrderItems = await _orderService.GetItemsByFormIdAsync(formId);
            if (existingOrderItems.Any())
            {
                lineNbr = existingOrderItems.Max(c => c.LineNbr);
            }

            foreach (var catalogId in catalogIds)
            {
                var catalog = await _catalogService.GetByIdAsync(catalogId);
                if (catalog == null)
                    continue;

                var orderItem = new OrderItem();
                orderItem.FormId = formId;
                orderItem.CatalogId = catalogId;
                orderItem.LineNbr = lineNbr;
                orderItem.CategoryId = catalog.UOMId;
                orderItem.Description = catalog.Description;
                orderItem.Qty = 0m;
                orderItem.Price = catalog.Price;
                orderItem.DiscountAmount = 0m;
                orderItem.SubTotal = 0m;
                orderItem.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                orderItem.CreatedOn = DateTime.Now;

                await _orderService.InsertTempItemAsync(orderItem, DataChangeStatus.Insert);
                lineNbr ++;
            }

            return NullJsonResult();
        }

        public async Task<IActionResult> EditItem(OrderItemModel itemModel)
        {
            if (itemModel == null)
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return JsonError("Order form settings not yet configured");

            if (!_orderSettings.Enabled || !_orderSettings.AllowAddItemEnabled)
                return JsonError("Order form settings not yet enabled");

            if (itemModel.FormId.IsNullOrEmpty())
                return JsonError("Order form id doesnt exists");

            var orderItem = _mapper.Map<OrderItem>(itemModel);
            orderItem.SubTotal = (orderItem.Qty * orderItem.Price) - orderItem.DiscountAmount;

            await _orderService.InsertTempItemAsync(orderItem, DataChangeStatus.Update);

            // need to pass form totals json result
            var orderItems = await _orderService.GetItemsByFormIdAsync(itemModel.FormId);

            #region Total Computations
            decimal totalQty = orderItems.Any() ? orderItems.Sum(c => c.Qty) : 0m;
            decimal totalDiscountAmount = orderItems.Any() ? orderItems.Sum(c => c.DiscountAmount) : 0m;
            decimal totalGrossAmount = orderItems.Any() ? orderItems.Sum(c => c.SubTotal) : 0m;
            decimal totalVatAmount = totalGrossAmount > 0m ? totalGrossAmount * 0.12m : 0m;
            decimal totalNetAmount = (totalGrossAmount > 0m && totalVatAmount > 0m) ? totalGrossAmount * 1.12m : 0m;
            #endregion

            var jsonDataResponse = new
            {
                TotalQty = totalQty,
                TotalDiscountAmount = totalDiscountAmount,
                TotalGrossAmount = totalGrossAmount,
                TotalVatAmount = totalVatAmount,
                TotalNetAmount = totalNetAmount
            };

            return Json(jsonDataResponse);
        }

        public async Task<IActionResult> DeleteItem(OrderItemModel itemModel)
        {
            if (!_accessControl.HasViewAccessAsync<Order>().Result)
                return UnauthorizedAccess();

            if (_orderSettings == null)
                return JsonError("Order form settings not yet configured");

            if (!_orderSettings.Enabled || !_orderSettings.AllowAddItemEnabled)
                return JsonError("Order form settings not yet enabled");

            var orderItem = _mapper.Map<OrderItem>(itemModel);

            await _orderService.InsertTempItemAsync(orderItem, DataChangeStatus.Delete);

            // need to pass form totals json result
            var orderItems = await _orderService.GetItemsByFormIdAsync(itemModel.FormId);

            #region Total Computations
            decimal totalQty = orderItems.Any() ? orderItems.Sum(c => c.Qty) : 0m;
            decimal totalDiscountAmount = orderItems.Any() ? orderItems.Sum(c => c.DiscountAmount) : 0m;
            decimal totalGrossAmount = orderItems.Any() ? orderItems.Sum(c => c.SubTotal) : 0m;
            decimal totalVatAmount = totalGrossAmount > 0m ? totalGrossAmount * 0.12m : 0m;
            decimal totalNetAmount = (totalGrossAmount > 0m && totalVatAmount > 0m) ? totalGrossAmount - totalVatAmount : 0m;
            #endregion

            var jsonDataResponse = new
            {
                TotalQty = totalQty,
                TotalDiscountAmount = totalDiscountAmount,
                TotalGrossAmount = totalGrossAmount,
                TotalVatAmount = totalVatAmount,
                TotalNetAmount = totalNetAmount
            };

            return Json(jsonDataResponse);
        }
        #endregion
    }
}
