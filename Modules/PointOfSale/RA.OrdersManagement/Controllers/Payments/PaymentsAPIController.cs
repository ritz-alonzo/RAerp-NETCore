using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.OrdersManagement.Payments;
using RA.Core.PluginData.FormTypes;
using RA.Core.PluginData.FormTypes.OrdersManagement.Payments;
using RA.Discounts.Domain;
using RA.Discounts.Services;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using RA.OrdersManagement.DTO.Payments;
using RA.OrdersManagement.Services.Carts;
using RA.OrdersManagement.Services.Orders;
using RA.OrdersManagement.Services.Payments;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Domain.Application;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Controllers.Payments
{
    [ApiController]
    [Route("api/v{version:apiVersion}/payments")]
    [Authorize]
    [ApiVersion("1.0")]
    public class PaymentsAPIController : AdminApiController
    {
        #region Constants
        private readonly IAccessControl _accessControl;
        private readonly IUserIdentity _userIdentity;
        private readonly IPaymentService _paymentService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly IDiscountService _discountService;
        private readonly ICartService _cartService;
        #endregion

        #region Ctor
        public PaymentsAPIController(IAccessControl accessControl,
            IUserIdentity userIdentity,
            IPaymentService paymentService,
            IApplicationSettingService applicationSettingService,
            IMapper mapper,
            IOrderService orderService,
            IDiscountService discountService,
            ICartService cartService)
        {
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _paymentService = paymentService;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _mapper = mapper;
            _orderService = orderService;
            _discountService = discountService;
            _cartService = cartService;
        }
        #endregion

        #region Version 1.0

        #region Payment
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetPaymentList([FromQuery] PaymentQueryRequestDto paymentSearchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            List<PaymentResponseDto> paymentResponseList = new List<PaymentResponseDto>();
            var paymentList = await _paymentService.GetPaymentPagedResultListAsync(
                searchQuery: paymentSearchModel?.SearchQuery,
                searchPaymentRefNbr: paymentSearchModel?.SearchPaymentRefNbr,
                searchPaymentOrderNbr: paymentSearchModel?.SearchPaymentOrderNbr,
                searchCustomerName: paymentSearchModel?.SearchCustomerName,
                searchPaymentDate: paymentSearchModel.SearchPaymentDate,
                searchCreatedDate: paymentSearchModel.SearchCreatedOn,
                paymentStatusIds: paymentSearchModel.SearchPaymentStatusIds,
                formStatusIds: paymentSearchModel.SearchStatusIds,
                showDeleted: paymentSearchModel.ShowDeleted,
                pageNumber: paymentSearchModel.PageNumber,
                pageSize: paymentSearchModel.PageSize
            );

            if (paymentList.Items.Any())
            {
                paymentResponseList = paymentList.Items.Select(payment =>
                {
                    PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);
                    return paymentResponse;

                }).ToList();
            }

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successful", dataList: paymentResponseList));
        }

        [HttpGet("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetPaymentByIdOrFormNbr(string idOrFormNbr)
        {
            if (!string.IsNullOrEmpty(idOrFormNbr))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId or FormNbr doesn't have value"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            Payment payment = null;
            if (Guid.TryParse(idOrFormNbr, out Guid result))
                payment = await _paymentService.GetFormByIdAsync(result);
            else
                payment = await _paymentService.GetFormByFormNbrAsync(idOrFormNbr);

            if (payment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);

            return Ok(GenerateResponseModel<PaymentResponseDto>(HttpStatusCode.OK, "Successful", paymentResponse));
        }

        [HttpGet("order/{orderId:guid}")]
        public async Task<IActionResult> GetForConfirmationPaymentByOrderId(Guid orderId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (orderId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "OrderId is invalid"));

            Payment payment = await _paymentService.GetForConfirmationPaymentByOrderIdAsync(orderId);
            if (payment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);

            return Ok(GenerateResponseModel<PaymentResponseDto>(HttpStatusCode.OK, "Successful", paymentResponse));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreatePayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            
            Payment payment = _mapper.Map<Payment>(paymentRequest);
            if (payment.OrderId.IsNotNullOrEmpty())
            {
                Order order = await _orderService.GetFormByIdAsync(payment.OrderId);
                if (order == null)
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));
                payment.CustomerId = order.CustomerId;
                payment.CustomerName = order.CustomerName;
            }
            payment.CreatedById = currentUser.Id;
            payment.PaymentStatus = PaymentStatus.ForConfirmation;
            await _paymentService.InsertFormAsync(payment);

            //await UpdateOrderToOpen(payment, currentUser.Id);

            //await CopyOrderItemsAndCreate(payment, currentUser.Id);

            PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);

            return Ok(GenerateResponseModel<PaymentResponseDto>(HttpStatusCode.OK, "Successfully Created Payment", paymentResponse));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdatePayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (paymentRequest.Id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment Id is invalid"));

            Payment existingPayment = await _paymentService.GetFormByIdAsync(paymentRequest.Id);
            if (existingPayment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            Payment payment = _mapper.Map(paymentRequest, existingPayment);

            payment.ModifiedById = currentUser?.Id;
            await _paymentService.UpdateFormAsync(payment);

            PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);

            return Ok(GenerateResponseModel<PaymentResponseDto>(HttpStatusCode.OK, "Successfully Updated Payment", paymentResponse));
        }

        [HttpDelete("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeletePayment(string idOrFormNbr)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            Payment payment = null;
            if (Guid.TryParse(idOrFormNbr, out Guid id))
                payment = await _paymentService.GetFormByIdAsync(id);
            else
                payment = await _paymentService.GetFormByFormNbrAsync(idOrFormNbr);

            if (payment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            payment.ModifiedById = currentUser?.Id;
            await _paymentService.DeleteFormAsync(payment);

            return NoContent();
        }

        [HttpPost("for-confirmation"), MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateForConfirmationPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            Payment payment = _mapper.Map<Payment>(paymentRequest);
            if (payment.OrderId.IsNotNullOrEmpty())
            {
                Order order = await _orderService.GetFormByIdAsync(payment.OrderId);
                if (order == null)
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));
                payment.CustomerId = order.CustomerId;
                payment.CustomerName = order.CustomerName;
            }
            payment.CreatedById = currentUser.Id;
            payment.PaymentStatus = PaymentStatus.ForConfirmation;
            await _paymentService.InsertFormAsync(payment);

            await UpdateOrderToOpen(payment, currentUser.Id);

            await CopyOrderItemsAndCreate(payment, currentUser.Id);

            PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);

            return Ok(GenerateResponseModel<PaymentResponseDto>(HttpStatusCode.OK, "Successfully Created Payment", paymentResponse));
        }

        [HttpPut("complete"), MapToApiVersion("1.0")]
        public async Task<IActionResult> CompletePayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (paymentRequest.Id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment Id is invalid"));

            Payment existingPayment = await _paymentService.GetFormByIdAsync(paymentRequest.Id);
            if (existingPayment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            Payment payment = _mapper.Map(paymentRequest, existingPayment);

            payment.ModifiedById = currentUser?.Id;
            payment.PaymentStatus = PaymentStatus.Completed;
            payment.Status = FormStatus.Closed;
            await _paymentService.UpdateFormAsync(payment);

            if (payment.OrderId.IsNotNullOrEmpty())
            {
                // Update Order Status
                Order order = await _orderService.GetFormByIdAsync(payment.OrderId);
                if (order != null)
                {
                    order.ModifiedById = currentUser?.Id;
                    order.Status = FormStatus.Processing;
                    await _orderService.UpdateFormAsync(order);

                    // Update Cart Items remove Open Order Items
                    if (order.CartId.IsNotNullOrEmpty())
                    {
                        Cart cart = await _cartService.GetFormByIdAsync(order.CartId.Value);
                        var orderItems = await _orderService.GetItemsByFormIdAsync(payment.OrderId);
                        var cartItems = await _cartService.GetItemsByFormIdAsync(order.CartId.Value);
                        if (orderItems != null && orderItems.Any() && cart != null && cartItems != null && cartItems.Any())
                        {
                            decimal totalUpdatedCartQty = 0;
                            decimal totalUpdatedCartAmount = 0;
                            foreach (var orderItem in orderItems)
                            {
                                var cartItem = cartItems.FirstOrDefault(x => x.CatalogId == orderItem.CatalogId);
                                if (cartItem != null)
                                {
                                    if (cartItem.Qty == orderItem.Qty)
                                    {
                                        totalUpdatedCartQty += cartItem.Qty;
                                        totalUpdatedCartAmount += cartItem.SubTotal;
                                        await _cartService.DeleteItemAsync(cartItem, saveChangesToDb: true);
                                    }
                                    else if (cartItem.Qty > orderItem.Qty)
                                    {
                                        decimal remainingQty = cartItem.Qty - orderItem.Qty;
                                        decimal remainingSubTotal = remainingQty * cartItem.Price;
                                        totalUpdatedCartQty += remainingQty;
                                        totalUpdatedCartAmount += remainingSubTotal;
                                        cartItem.Qty = remainingQty;
                                        cartItem.SubTotal = remainingSubTotal;
                                        await _cartService.UpdateItemAsync(cartItem, saveChangesToDb: true);
                                    }
                                }
                            }

                            // Recompute Cart Total Amount
                            if (totalUpdatedCartQty > 0 || totalUpdatedCartAmount > 0)
                            {
                                cart.TotalQty = cart.TotalQty - totalUpdatedCartQty;
                                cart.TotalAmount = cart.TotalAmount - totalUpdatedCartAmount;
                                await _cartService.UpdateFormAsync(cart);
                            }
                        }
                    }
                }
            }

            PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);

            return Ok(GenerateResponseModel<PaymentResponseDto>(HttpStatusCode.OK, "Successfully Updated Payment", paymentResponse));
        }

        [HttpPut("void"), MapToApiVersion("1.0")]
        public async Task<IActionResult> VoidPayment([FromBody] PaymentRequestDto paymentRequest)
        {
            if (paymentRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (paymentRequest.Id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment Id is invalid"));

            Payment existingPayment = await _paymentService.GetFormByIdAsync(paymentRequest.Id);
            if (existingPayment == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment doesn't exists"));

            Payment payment = _mapper.Map(paymentRequest, existingPayment);

            payment.ModifiedById = currentUser?.Id;
            payment.PaymentStatus = PaymentStatus.Voided;
            payment.Status = FormStatus.Cancelled;
            await _paymentService.UpdateFormAsync(payment);

            // Update Order Status

            PaymentResponseDto paymentResponse = _mapper.Map<PaymentResponseDto>(payment);

            return Ok(GenerateResponseModel<PaymentResponseDto>(HttpStatusCode.OK, "Successfully Updated Payment", paymentResponse));
        }

        //[HttpPost("pending"), MapToApiVersion("1.0")]
        //public async Task<IActionResult> CreatePendingPayment([FromBody] Payment payment)
        //{
        //    if (payment == null)
        //        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment is empty"));

        //    ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
        //    if (validationModel != null && validationModel.IsPassed == false)
        //        return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

        //    // Create Pending Payment
        //    // Update Order Status
        //    var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

        //    payment.CreatedById = currentUser.Id;
        //    await _paymentService.InsertFormAsync(payment);

        //    return Ok(GenerateResponseModel<Payment>(HttpStatusCode.OK, "Successfully Created Payment", payment));
        //}
        #endregion

        #region Payment Item
        [HttpGet("item/{formId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetPaymentItemList(string formId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            List<PaymentItem> paymentItemList = new List<PaymentItem>();
            if (Guid.TryParse(formId, out Guid result))
            {
                if (result.IsNotNullOrEmpty())
                    paymentItemList = _paymentService.GetItemsByFormIdAsync(result).Result.ToList();
            }

            return Ok(GenerateListResponseModel<PaymentItem>(HttpStatusCode.OK, "Succesful", dataList: paymentItemList));
        }

        [HttpPost("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> InsertPaymentItem([FromBody] List<PaymentItemRequestDto> paymentItems)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (!paymentItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            List<PaymentItemResponseDto> paymentItemResponseList = new List<PaymentItemResponseDto>();
            foreach (var item in paymentItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                PaymentItem paymentItem = _mapper.Map<PaymentItem>(item);

                paymentItem.CreatedById = currentUser.Id;
                await _paymentService.InsertItemAsync(paymentItem, saveChangesToDb: true);

                PaymentItemResponseDto paymentItemResponse = _mapper.Map<PaymentItemResponseDto>(paymentItem);

                paymentItemResponseList.Add(paymentItemResponse);
            }

            return Ok(GenerateListResponseModel<PaymentItemResponseDto>(HttpStatusCode.OK, "Successfully added payment items", dataList: paymentItemResponseList));
        }

        [HttpPut("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdatePaymentItem([FromBody] List<PaymentItemRequestDto> paymentItems)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (!paymentItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            List<PaymentItemResponseDto> paymentItemResponseList = new List<PaymentItemResponseDto>();
            foreach (var item in paymentItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                PaymentItem existingPaymentItem = await _paymentService.GetItemByIdAsync(item.Id);
                if (existingPaymentItem == null)
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment item doesn't exists"));

                PaymentItem paymentItem = _mapper.Map(item, existingPaymentItem);

                paymentItem.ModifiedById = currentUser?.Id;
                await _paymentService.UpdateItemAsync(paymentItem, saveChangesToDb: true);

                PaymentItemResponseDto paymentItemResponse = _mapper.Map<PaymentItemResponseDto>(paymentItem);

                paymentItemResponseList.Add(paymentItemResponse);
            }

            return Ok(GenerateListResponseModel<PaymentItemResponseDto>(HttpStatusCode.OK, "Successfully added payment items", dataList: paymentItemResponseList));
        }

        [HttpDelete("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeletePaymentItem([FromBody] List<PaymentItem> paymentItems)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (!paymentItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Payment item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            foreach (var item in paymentItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                item.ModifiedById = currentUser?.Id;
                await _paymentService.DeleteItemAsync(item, saveChangesToDb: true);
            }

            return Ok(GenerateListResponseModel<PaymentItem>(HttpStatusCode.OK, "Successfully added order items", dataList: paymentItems));
        }
        #endregion

        #endregion

        #region Version 2.0
        [HttpGet, MapToApiVersion("2.0")]
        public async Task<IActionResult> GetPaymentListv2([FromQuery] PaymentSearchModel paymentSearchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Payment>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));


            var paymentList = await _paymentService.GetPaymentListAsync(
                searchQuery: paymentSearchModel?.SearchQuery,
                searchPaymentRefNbr: paymentSearchModel?.SearchPaymentRefNbr,
                searchPaymentOrderNbr: paymentSearchModel?.SearchOrderNbr,
                searchPaymentDate: paymentSearchModel.SearchPaymentDate,
                searchCreatedDate: paymentSearchModel.SearchCreatedOn,
                paymentStatusIds: paymentSearchModel.SearchPaymentStatusId > 0 ? new List<int> { paymentSearchModel.SearchPaymentStatusId } : null,
                formStatusIds: paymentSearchModel.SearchStatusId > 0 ? new List<int> { paymentSearchModel.SearchStatusId } : null,
                showDeleted: paymentSearchModel.ShowDeleted,
                pageNumber: paymentSearchModel.PageNumber,
                pageSize: paymentSearchModel.PageSize
            );

            return Ok(GenerateListResponseModel<Payment>(HttpStatusCode.OK, "Successful", paymentSearchModel.PageNumber, paymentSearchModel.PageSize, paymentList));
        }
        #endregion

        #region Methods

        private async Task UpdateOrderToOpen(Payment payment, Guid currentUserId)
        {
            if (payment.OrderId.IsNullOrEmpty())
                return;

            // Update Order Status
            Order order = await _orderService.GetFormByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.ModifiedById = currentUserId;
                order.Status = FormStatus.Open;
                await _orderService.UpdateFormAsync(order);
            }

            // Update Discount Redemption
            List<DiscountRedemption> discountRedemption = (List<DiscountRedemption>)await _discountService.GetPendingDiscountRedemptionListByOrderIdAsync(payment.OrderId);
            if (discountRedemption != null && discountRedemption.Any())
            {
                foreach (var item in discountRedemption)
                {
                    item.IsPending = false;
                    item.RedeemedAt = DateTime.UtcNow;
                    await _discountService.UpdateDiscountRedemptionAsync(item);
                }
            }

            // Update Cart Items remove Open Order Items
            var orderItems = await _orderService.GetItemsByFormIdAsync(payment.OrderId);
            if (order.CartId.IsNullOrEmpty())
                return;
            var cartItems = await _cartService.GetItemsByFormIdAsync(order.CartId.Value);
            if (orderItems != null && orderItems.Any() && cartItems != null && cartItems.Any())
            {
                foreach (var orderItem in orderItems)
                {
                    var cartItem = cartItems.FirstOrDefault(x => x.CatalogId == orderItem.CatalogId);
                    if (cartItem != null)
                    {
                        await _cartService.DeleteItemAsync(cartItem, saveChangesToDb: true);
                    }
                }
            }
        }

        private async Task CopyOrderItemsAndCreate(Payment payment, Guid currentUserId)
        {
            if (payment.OrderId.IsNullOrEmpty())
                return;
            // Get Order Items
            var orderItems = await _orderService.GetItemsByFormIdAsync(payment.OrderId);
            if (orderItems != null && orderItems.Any())
            {
                var lineNbr = 1;
                foreach (var orderItem in orderItems)
                {
                    PaymentItem paymentItem = new PaymentItem
                    {
                        FormId = payment.Id,
                        CatalogId = orderItem.CatalogId,
                        CategoryId = orderItem.CategoryId.GetValueOrDefault(),
                        LineNbr = lineNbr,
                        Qty = orderItem.Qty,
                        Price = orderItem.Price,
                        DiscountAmount = orderItem.DiscountAmount,
                        SubTotal = orderItem.SubTotal,
                        Description = orderItem.Description,
                        CreatedById = currentUserId
                    };
                    await _paymentService.InsertItemAsync(paymentItem, saveChangesToDb: true);
                    lineNbr++;
                }
            }
        }

        #endregion
    }
}
