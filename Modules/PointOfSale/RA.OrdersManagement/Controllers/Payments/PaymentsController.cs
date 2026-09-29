using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using RA.Catalogs.Domain;
using RA.Catalogs.Services;
using RA.Core.Data;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.Core.Models.PluginModels.OrdersManagement.Payments;
using RA.Core.PluginData.FormTypes;
using RA.Core.PluginData.FormTypes.OrdersManagement.Payments;
using RA.FormTypes.Data;
using RA.FormTypes.Helpers;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using RA.OrdersManagement.Factories.Payments;
using RA.OrdersManagement.Services.Orders;
using RA.OrdersManagement.Services.Payments;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Controllers.Payments
{
    public class PaymentsController : AdminController
    {
        #region Constants
        private readonly IPaymentService _paymentService;
        private readonly IPaymentModelFactory _paymentModelFactory;
        private readonly IFormTypeManager _formTypeManager;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        private readonly IAccessControl _accessControl;
        private readonly PaymentSetting _paymentSettings;
        private readonly ICatalogService _catalogService;
        private readonly IOrderService _orderService;
        #endregion

        #region Ctor
        public PaymentsController(IPaymentService paymentService,
            IPaymentModelFactory paymentModelFactory,
            IFormTypeManager formTypeManager,
            IMapper mapper,
            IUserIdentity userIdentity,
            IAccessControl accessControl,
            ICatalogService catalogService,
            IOrderService orderService)
        {
            _paymentService = paymentService;
            _paymentModelFactory = paymentModelFactory;
            _formTypeManager = formTypeManager;
            _mapper = mapper;
            _userIdentity = userIdentity;
            _accessControl = accessControl;
            _paymentSettings = _formTypeManager.GetSettingDataOfFormAsync<Payment, PaymentSetting>().Result;
            _catalogService = catalogService;
            _orderService = orderService;
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

            var paymentConfigureModel = await _paymentModelFactory.PreparePaymentConfigureModelAsync(systemName);

            return View(paymentConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(PaymentConfigureModel paymentConfigureModel)
        {
            if (paymentConfigureModel == null)
                return JsonError(FormTypeMessages.FormTypeSystemNameNotExists);

            if (!_accessControl.HasSuperAdminAccessAsync().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                await _formTypeManager.InsertFormSettingAsync<Payment, PaymentSetting>();

            var settings = _mapper.Map(paymentConfigureModel, _paymentSettings);

            await _formTypeManager.UpdateSettingDataOfFormAsync<Payment, PaymentSetting>(settings);

            return NullJsonResult();
        }
        #endregion

        #region CRUD Form
        public async Task<IActionResult> List(int page = 1)
        {
            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            var model = await _paymentModelFactory.PreparePaymentSearchModelAsync(new PaymentSearchModel(), _paymentSettings.ItemsPageSize, page);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> OrderListSearch(PaymentSearchModel searchModel)
        {
            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            var model = await _paymentModelFactory.PreparePaymentListModelAsync(searchModel);

            return PartialView(model);
        }

        public async Task<IActionResult> Index(Guid formId)
        {
            if (formId.IsNullOrEmpty())
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            var paymentForm = await _paymentService.GetFormByIdAsync(formId);
            if (paymentForm == null)
                return NotFound();

            var model = await _paymentModelFactory.PreparePaymentModelAsync(new PaymentModel(), paymentForm);

            return View(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            var model = await _paymentModelFactory.PreparePaymentModelAsync(new PaymentModel(), null);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PaymentModel model)
        {
            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            if (ModelState.IsValid)
            {
                var paymentForm = _mapper.Map<Payment>(model);

                paymentForm.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                var paymentItems = await _paymentService.GetItemsByFormIdAsync(paymentForm.Id);

                #region Total Computations
                paymentForm.TotalQty = paymentItems.Sum(c => c.Qty);
                paymentForm.TotalDiscountAmount = paymentItems.Sum(c => c.DiscountAmount);
                paymentForm.TotalGrossAmount = paymentItems.Sum(c => c.SubTotal);
                paymentForm.TotalVatAmount = paymentForm.TotalGrossAmount * 1.12m;
                paymentForm.TotalNetAmount = paymentForm.TotalGrossAmount - paymentForm.TotalVatAmount;
                #endregion

                await _paymentService.InsertFormAsync(paymentForm);
                // sanity check
                model.Id = paymentForm.Id;

                SuccessNotification(model, "Successfully created Payment");
            }
            else
            {
                ErrorNotification(model, "Failed to create Payment");
                return View("Create");
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(PaymentModel model, string actionType)
        {
            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            if (ModelState.IsValid)
            {
                var paymentForm = _mapper.Map<Payment>(model);
                if (!string.IsNullOrEmpty(actionType))
                {
                    switch (actionType)
                    {
                        case FormTypeAction.ForApproval:
                            if (_paymentSettings.ApprovalEnabled)
                                paymentForm.Status = FormStatus.AwaitingApproval;
                            break;
                        case FormTypeAction.Approve:
                            if (_paymentSettings.ApprovalEnabled)
                                paymentForm.Status = FormStatus.Approved;
                            break;

                        case FormTypeAction.Release:
                            paymentForm.Status = FormStatus.Open;
                            paymentForm.PaymentStatus = PaymentStatus.Completed;
                            break;

                        case FormTypeAction.Cancel:
                            paymentForm.Status = FormStatus.Cancelled;
                            paymentForm.PaymentStatus = PaymentStatus.Voided;
                            break;
                    }
                }
                else
                {
                    paymentForm.PaymentStatus = PaymentStatus.ForConfirmation;
                }

                var paymentItems = await _paymentService.GetItemsByFormIdAsync(paymentForm.Id);

                #region Total Computations
                paymentForm.TotalQty = paymentItems.Sum(c => c.Qty);
                paymentForm.TotalDiscountAmount = paymentItems.Sum(c => c.DiscountAmount);
                paymentForm.TotalGrossAmount = paymentItems.Sum(c => c.SubTotal);
                paymentForm.TotalVatAmount = paymentForm.TotalGrossAmount * 1.12m;
                paymentForm.TotalNetAmount = paymentForm.TotalGrossAmount - paymentForm.TotalVatAmount;
                #endregion

                paymentForm.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _paymentService.UpdateFormAsync(paymentForm);

                SuccessNotification(model, "Successfully updated Payment");
            }
            else
            {
                ErrorNotification(model, "Failed to update Payment");
                return View("Index", new { id = model.Id });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            var paymentForm = await _paymentService.GetFormByIdAsync(id);
            if (paymentForm == null)
                return NotFound();

            await _paymentService.DeleteFormAsync(paymentForm);
            SuccessNotification(new PaymentModel() { Id = paymentForm.Id }, "Successfully deleted Payment");

            return RedirectToAction("List");
        }
        #endregion

        #region CRUD Form Item
        public async Task<IActionResult> PaymentItemList(PaymentModel paymentModel, int page = 1)
        {
            if (paymentModel == null)
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return NotFound();

            if (!_paymentSettings.Enabled)
                return NotFound();

            var itemModel = await _paymentModelFactory.PreparePaymentItemListModelAsync(paymentModel, page);

            return PartialView("Views/Payments/_CreateAndEdit.Items.cshtml", itemModel);
        }

        public async Task<IActionResult> LoadOrderItems(PaymentModel model)
        {
            if (model == null)
                return JsonError("Payment doesn't exists");

            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return JsonError("Payment form settings not yet configured");

            if (!_paymentSettings.Enabled)
                return JsonError("Payment form settings not yet enabled");

            var order = await _orderService.GetFormByIdAsync(model.OrderId);
            if (order == null)
                return NotFound();

            var orderItems = await _orderService.GetItemsByFormIdAsync(model.OrderId);
            foreach (var orderItem in orderItems)
            {
                var paymentItem = new PaymentItem();
                paymentItem.FormId = model.Id;
                paymentItem.CatalogId = orderItem.CatalogId;
                paymentItem.CategoryId = orderItem.CategoryId;
                paymentItem.Description = orderItem.Description;
                paymentItem.Qty = orderItem.Qty;
                paymentItem.Price = orderItem.Price;
                paymentItem.SubTotal = orderItem.SubTotal;
                paymentItem.DiscountAmount = orderItem.DiscountAmount;
                paymentItem.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                paymentItem.CreatedOn = DateTime.UtcNow;
                await _paymentService.InsertTempItemAsync(paymentItem, DataChangeStatus.Insert);
            }

            #region Total Mappings
            model.TotalQty = order.TotalQty;
            model.TotalDiscountAmount = order.TotalDiscountAmount;
            model.TotalGrossAmount = order.TotalGrossAmount;
            model.TotalVatAmount = order.TotalVatAmount;
            model.TotalNetAmount = order.TotalNetAmount;
            #endregion

            return Json(model);
        }

        // adding deleted items
        public async Task<IActionResult> InsertItem(Guid formId, List<Guid> orderItemIds)
        {
            if (!orderItemIds.HasAny())
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return JsonError("Payment form settings not yet configured");

            if (!_paymentSettings.Enabled)
                return JsonError("Payment form settings not yet enabled");

            foreach (var orderItemId in orderItemIds)
            {
                var orderItem = await _orderService.GetItemByIdAsync(orderItemId);
                if (orderItem == null)
                    continue;

                var paymentItem = new PaymentItem();
                paymentItem.FormId = formId;
                paymentItem.CatalogId = orderItem.CatalogId;
                paymentItem.CategoryId = orderItem.CategoryId;
                paymentItem.Description = orderItem.Description;
                paymentItem.Qty = orderItem.Qty;
                paymentItem.Price = orderItem.Price;
                paymentItem.SubTotal = orderItem.SubTotal;
                paymentItem.DiscountAmount = orderItem.DiscountAmount;
                paymentItem.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                paymentItem.CreatedOn = DateTime.UtcNow;
                await _paymentService.InsertTempItemAsync(paymentItem, DataChangeStatus.Insert);
            }

            return NullJsonResult();
        }

        public async Task<IActionResult> EditItem(PaymentItemModel itemModel)
        {
            if (itemModel == null)
                return NotFound();

            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return JsonError("Payment form settings not yet configured");

            if (!_paymentSettings.Enabled)
                return JsonError("Payment form settings not yet enabled");

            var paymentItem = _mapper.Map<PaymentItem>(itemModel);
            paymentItem.SubTotal = (paymentItem.Qty * paymentItem.Price) - paymentItem.DiscountAmount;

            await _paymentService.InsertTempItemAsync(paymentItem, DataChangeStatus.Update);

            return NullJsonResult();
        }

        public async Task<IActionResult> DeleteItem(PaymentItemModel itemModel)
        {
            if (!_accessControl.HasViewAccessAsync<Payment>().Result)
                return UnauthorizedAccess();

            if (_paymentSettings == null)
                return JsonError("Payment form settings not yet configured");

            if (!_paymentSettings.Enabled)
                return JsonError("Payment form settings not yet enabled");

            var paymentItem = _mapper.Map<PaymentItem>(itemModel);

            await _paymentService.InsertTempItemAsync(paymentItem, DataChangeStatus.Delete);

            return NullJsonResult();
        }
        #endregion
    }
}
