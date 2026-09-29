using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.Discounts.Domain;
using RA.Discounts.Services;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.DTO.Orders;
using RA.OrdersManagement.Services.Carts;
using RA.OrdersManagement.Services.Orders;
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

namespace RA.OrdersManagement.Controllers.Orders
{
    [ApiController]
    [Route("api/v{version:apiVersion}/orders")]
    [Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    // Discount is it by items or overall 
    // Delivery (Shipment)
    public class OrdersAPIController : AdminApiController
    {
        #region Constants
        private readonly IAccessControl _accessControl;
        private readonly IUserIdentity _userIdentity;
        private readonly IOrderService _orderService;
        private readonly ICartService _cartService;
        private readonly IMapper _mapper;
        private readonly IFormTypeManager _formTypeManager;
        private readonly OrderSetting _orderSettings;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly IDiscountService _discountService;
        #endregion

        #region Ctor
        public OrdersAPIController(IAccessControl accessControl,
            IUserIdentity userIdentity,
            IOrderService orderService,
            ICartService cartService,
            IMapper mapper,
            IFormTypeManager formTypeManager,
            IApplicationSettingService applicationSettingService,
            IDiscountService discountService)
        {
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _orderService = orderService;
            _cartService = cartService;
            _mapper = mapper;
            _formTypeManager = formTypeManager;
            _orderSettings = _formTypeManager.GetSettingDataOfFormAsync<Order, OrderSetting>().Result;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _discountService = discountService;
        }
        #endregion

        #region Version 1.0

        #region Order
        [HttpPost("test"), MapToApiVersion("1.0")]
        [Idempotent] // Protects this endpoint
        public async Task<IActionResult> TestIdempotent([FromBody] object testdata)
        {
            return Ok("Success");
        }


        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetOrderList([FromQuery] OrderSearchModel orderSearchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var orderList = await _orderService.GetOrderListAsync(
                searchQuery: orderSearchModel?.SearchQuery,
                searchCustomerName: orderSearchModel?.SearchCustomerName,
                searchServiceIds: orderSearchModel.SearchServiceId.HasValue ? new List<Guid> { orderSearchModel.SearchServiceId.Value } : null,
                searchCreatedDate: orderSearchModel.SearchCreatedOn,
                formStatusIds: orderSearchModel.SearchStatusId > 0 ? new List<int> { orderSearchModel.SearchStatusId } : null,
                searchOrderDate: orderSearchModel.SearchOrderDate,
                showDeleted: orderSearchModel.ShowDeleted,
                pageNumber: orderSearchModel.PageNumber,
                pageSize: orderSearchModel.PageSize
            );

            return Ok(GenerateListResponseModel<Order>(HttpStatusCode.OK, "Successful", orderSearchModel.PageNumber, orderSearchModel.PageSize, orderList));
        }

        [HttpGet("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetOrderByIdOrFormNbr(string idOrFormNbr)
        {
            if (string.IsNullOrEmpty(idOrFormNbr))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId or FormNbr doesn't have value"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            Order order = null;
            if (Guid.TryParse(idOrFormNbr, out Guid result))
                order = await _orderService.GetFormByIdAsync(result);
            else
                order = await _orderService.GetFormByFormNbrAsync(idOrFormNbr);

            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            return Ok(GenerateResponseModel<Order>(HttpStatusCode.OK, "Successful", order));
        }

        [HttpGet("cart/{cartId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetPendingOrderByCartId(Guid cartId)
        {
            if (cartId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "CartId doesn't have value"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            Order order = await _orderService.GetPendingOrderByCartIdAsync(cartId);
            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            OrderResponseDto orderResponse = _mapper.Map<OrderResponseDto>(order);

            return Ok(GenerateResponseModel<OrderResponseDto>(HttpStatusCode.OK, "Successful", orderResponse));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderRequestDto orderRequest)
        {
            if (orderRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            Order order = _mapper.Map<Order>(orderRequest);

            order.CreatedById = currentUser.Id;
            order.OrderDate = DateTime.UtcNow;
            order.Status = Core.PluginData.FormTypes.FormStatus.Onhold;
            await _orderService.InsertFormAsync(order);

            // Check if has CartId
            if (order.CartId.IsNotNullOrEmpty())
            {
                // Get Cart Items and insert items to Order Items
                IEnumerable<CartItem> cartItemList = await _cartService.GetItemsByFormIdAsync(order.CartId.Value);
                int lineNbr = 1;
                decimal totalQty = 0m;
                decimal totalSubTotal = 0m;
                foreach (var cartItem in cartItemList)
                {
                    var orderItem = _mapper.Map<OrderItem>(cartItem);
                    if (orderItem != null)
                    {
                        orderItem.FormId = order.Id;
                        orderItem.LineNbr = lineNbr;
                        orderItem.CreatedById = currentUser.Id;
                        orderItem.ModifiedById = null;
                        orderItem.CreatedOn = DateTime.UtcNow;
                        orderItem.ModifiedOn = null;
                        orderItem.DeletedOn = null;
                        orderItem.DiscountAmount = 0m;
                        totalQty += orderItem.Qty;
                        totalSubTotal += orderItem.SubTotal;
                        await _orderService.InsertItemAsync(orderItem, true);
                    }

                    lineNbr += 1;
                }

                #region Total Computations
                order.TotalQty = totalQty;
                order.TotalDiscountAmount = 0m;
                order.TotalGrossAmount = totalSubTotal;
                order.TotalVatAmount = order.TotalGrossAmount * 0.12m;
                order.TotalNetAmount = order.TotalGrossAmount;
                #endregion

                order.OrderDate = order.OrderDate == DateTime.MinValue ? DateTime.UtcNow : order.OrderDate;

                await _orderService.UpdateFormAsync(order);
            }

            OrderResponseDto orderResponse = _mapper.Map<OrderResponseDto>(order);

            return Ok(GenerateResponseModel<OrderResponseDto>(HttpStatusCode.OK, "Successfully Created Order", orderResponse));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateOrder([FromBody] OrderRequestDto orderRequest)
        {
            if (orderRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (_orderSettings == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order configuration not yet configured"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User doesn't exists please login"));

            if (orderRequest.Id.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order Id is invalid"));

            Order existingOrder = await _orderService.GetFormByIdAsync(orderRequest.Id);
            if (existingOrder == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            Order order = _mapper.Map(orderRequest, existingOrder);

            order.ModifiedById = currentUser?.Id;
            // Check if has CartId
            if (order.CartId.IsNotNullOrEmpty())
            {
                // Get Order Items
                IEnumerable<OrderItem> orderItemList = await _orderService.GetItemsByFormIdAsync(order.Id);
                // Get Cart Items and update Order Items
                IEnumerable<CartItem> cartItemList = await _cartService.GetItemsByFormIdAsync(order.CartId.Value);
                
                decimal totalQty = 0m;
                decimal totalSubTotal = 0m;
                List<Guid> updatedOrderItemCatalogIds = new List<Guid>();
                // Need to check if CartItems deleted an item 
                foreach (OrderItem orderItem in orderItemList)
                {
                    CartItem cartItem = cartItemList?.FirstOrDefault(c => c.CatalogId == orderItem.CatalogId);
                    if (cartItem != null)
                    {
                        updatedOrderItemCatalogIds.Add(orderItem.CatalogId);
                        if (orderItem.Qty == cartItem.Qty && orderItem.Qty == cartItem.SubTotal)
                            continue;

                        orderItem.Qty = cartItem.Qty;
                        orderItem.SubTotal = cartItem.SubTotal;
                        orderItem.ModifiedById = currentUser.Id;
                        orderItem.ModifiedOn = DateTime.UtcNow;
                        totalQty += orderItem.Qty;
                        totalSubTotal += orderItem.SubTotal;
                        await _orderService.UpdateItemAsync(orderItem, true);
                    }
                    // Means it's deleted remove it
                    else
                    {
                        await _orderService.DeleteItemAsync(orderItem, true);
                    }
                }

                int? maxLineNbr = orderItemList.Any() ? orderItemList.Max(c => c.LineNbr) : 0;
                // Added Cart Items 
                foreach (CartItem cartItem in cartItemList.Where(c => !updatedOrderItemCatalogIds.Contains(c.CatalogId)).ToList())
                {
                    if (cartItem.FormId.IsNullOrEmpty())
                        return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                    var orderItem = _mapper.Map<OrderItem>(cartItem);
                    if (orderItem != null)
                    {
                        orderItem.FormId = order.Id;
                        orderItem.LineNbr = maxLineNbr.Value;
                        orderItem.CreatedById = currentUser.Id;
                        orderItem.ModifiedById = null;
                        orderItem.CreatedOn = DateTime.UtcNow;
                        orderItem.ModifiedOn = null;
                        orderItem.DeletedOn = null;
                        orderItem.DiscountAmount = 0m;
                        totalQty += orderItem.Qty;
                        totalSubTotal += orderItem.SubTotal;
                        await _orderService.InsertItemAsync(orderItem, true);
                    }
                    maxLineNbr += 1;
                }

                #region Total Computations
                order.TotalQty = totalQty;
                order.TotalGrossAmount = totalSubTotal;
                order.TotalVatAmount = (order.TotalGrossAmount - order.TotalDiscountAmount) * 0.12m;
                order.TotalNetAmount = order.TotalGrossAmount - order.TotalDiscountAmount;
                #endregion
            }
            await _orderService.UpdateFormAsync(order);

            OrderResponseDto orderResponse = _mapper.Map<OrderResponseDto>(order);

            return Ok(GenerateResponseModel<OrderResponseDto>(HttpStatusCode.OK, "Successfully Updated Order", orderResponse));
        }

        [HttpDelete("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteOrder(string idOrFormNbr)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            Order order = null;
            if (Guid.TryParse(idOrFormNbr, out Guid id))
                order = await _orderService.GetFormByIdAsync(id);
            else
                order = await _orderService.GetFormByFormNbrAsync(idOrFormNbr);

            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            order.ModifiedById = currentUser?.Id;
            await _orderService.DeleteFormAsync(order);

            return NoContent();
        }

        #region Order Discount
        [HttpGet("discount/{orderId:guid}")]
        public async Task<IActionResult> GetPendingOrderDiscount(Guid orderId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (orderId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Request body doesn't exists"));

            Order order = await _orderService.GetFormByIdAsync(orderId);
            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            List<DiscountRedemption> discountRedemptionList = new List<DiscountRedemption>();
            discountRedemptionList = (List<DiscountRedemption>)await _discountService.GetPendingDiscountRedemptionListByOrderIdAsync(order.Id);

            return Ok(GenerateListResponseModel<DiscountRedemption>(HttpStatusCode.OK, "Successful", dataList: discountRedemptionList));
        }

        [HttpPost("discount"), MapToApiVersion("1.0")]
        public async Task<IActionResult> ApplyPendingOrderDiscount([FromBody] OrderDiscountRequestDto orderRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (orderRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Request body doesn't exists"));

            Order order = await _orderService.GetFormByIdAsync(orderRequest.Id);
            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));
            // Create Discount Redemption
            await CreateOrderDiscountRedemption(order, orderRequest.DiscountId, orderRequest.DiscountAmount);
            // Recompute Total
            await RecomputeOrderTotals(order, orderRequest.DiscountAmount);

            OrderResponseDto orderResponse = _mapper.Map<OrderResponseDto>(order);

            return Ok(GenerateResponseModel<OrderResponseDto>(HttpStatusCode.OK, "Successful", orderResponse));
        }
        #endregion

        #endregion

        #region Order Item
        [HttpGet("item/{formId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetOrderItemList(string formId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            List<OrderItem> orderItemList = new List<OrderItem>();
            if (Guid.TryParse(formId, out Guid result))
            {
                if (result.IsNotNullOrEmpty())
                    orderItemList = _orderService.GetItemsByFormIdAsync(result).Result.ToList();
            }

            return Ok(GenerateListResponseModel<OrderItem>(HttpStatusCode.OK, "Succesful", dataList: orderItemList));
        }

        [HttpPost("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> InsertOrderItems([FromBody] List<OrderItem> orderItems)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (!orderItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            // Insert from Cart
            foreach (var item in orderItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                item.CreatedById = currentUser.Id;
                await _orderService.InsertItemAsync(item, saveChangesToDb: true);
            }

            return Ok(GenerateListResponseModel<OrderItem>(HttpStatusCode.OK, "Successfully added order items", dataList: orderItems));
        }

        [HttpPut("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateOrderItem([FromBody] OrderItemRequestDto orderItemRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (orderItemRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order item doesn't have value"));

            Order order = await _orderService.GetFormByIdAsync(orderItemRequest.FormId);
            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            OrderItem existingOrderItem = await _orderService.GetItemByIdAsync(orderItemRequest.Id);
            if (existingOrderItem == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order item doesn't exists"));

            OrderItem orderItem = _mapper.Map(orderItemRequest, existingOrderItem);
            await _orderService.UpdateItemAsync(orderItem, saveChangesToDb: true);

            // Recompute Totals
            await RecomputeOrderTotals(order, order.TotalDiscountAmount);

            return NoContent();
        }

        [HttpDelete("item/{orderItemId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteOrderItem([FromRoute] Guid orderItemId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            OrderItem existingOrderItem = await _orderService.GetItemByIdAsync(orderItemId);
            if (existingOrderItem == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order item doesn't exists"));

            Order order = await _orderService.GetFormByIdAsync(existingOrderItem.FormId);
            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            await _orderService.DeleteItemAsync(existingOrderItem, saveChangesToDb: true);

            // Recompute Totals
            await RecomputeOrderTotals(order, order.TotalDiscountAmount);

            return NoContent();
        }
        #endregion

        #endregion

        #region Version 2.0
        [HttpGet, MapToApiVersion("2.0")]
        public async Task<IActionResult> GetOrderListv2([FromQuery] OrderSearchModel orderSearchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));


            var orderList = await _orderService.GetOrderListAsync(
                searchQuery: orderSearchModel?.SearchQuery,
                searchCustomerName: orderSearchModel?.SearchCustomerName,
                searchServiceIds: orderSearchModel.SearchServiceId.HasValue ? new List<Guid> { orderSearchModel.SearchServiceId.Value } : null,
                searchCreatedDate: orderSearchModel.SearchCreatedOn,
                formStatusIds: orderSearchModel.SearchStatusId > 0 ? new List<int> { orderSearchModel.SearchStatusId } : null,
                searchOrderDate: orderSearchModel.SearchOrderDate,
                showDeleted: orderSearchModel.ShowDeleted,
                pageNumber: orderSearchModel.PageNumber,
                pageSize: orderSearchModel.PageSize
            );

            return Ok(GenerateListResponseModel<Order>(HttpStatusCode.OK, "Successful", orderSearchModel.PageNumber, orderSearchModel.PageSize, orderList));
        }
        #endregion

        #region Methods (will move this to order service)
        private async Task RecomputeOrderTotals(Order order, decimal? totalDiscountAmount = null)
        {
            // Get Sum of Items Subtotal
            decimal itemsSubTotal = (await _orderService.GetItemsByFormIdAsync(order.Id)).ToList().Sum(c => c.SubTotal);
            order.TotalGrossAmount = itemsSubTotal;
            order.TotalNetAmount = order.TotalGrossAmount - order.TotalDiscountAmount;
            order.TotalVatAmount = order.TotalNetAmount * 0.12m;
            if (totalDiscountAmount != null)
            {
                decimal updatedNetAmount = order.TotalGrossAmount - totalDiscountAmount.GetValueOrDefault();
                order.TotalDiscountAmount = totalDiscountAmount.GetValueOrDefault();
                order.TotalVatAmount = updatedNetAmount * 0.12m;
                order.TotalNetAmount = updatedNetAmount;
            }
            await _orderService.UpdateFormAsync(order);
        }

        private async Task CreateOrderDiscountRedemption(Order order, Guid discountId, decimal discountAmount)
        {
            if (order == null || discountAmount <= 0)
                return;
            if (order.CustomerId.IsNullOrEmpty())
                return;
            if (discountId.IsNullOrEmpty())
                return;

            Discount discount = await _discountService.GetByIdAsync(discountId);
            if (discount == null) return;

            List<DiscountRedemption> discountRedemptionList = (List<DiscountRedemption>)await _discountService.GetPendingDiscountRedemptionListByOrderIdAsync(order.Id);
            if (discountRedemptionList != null && discountRedemptionList.Any())
            {
                foreach (var discRedemption in discountRedemptionList)
                {
                    if (discRedemption.DiscountId != discount.Id)
                    {
                        await _discountService.DeleteDiscountRedemptionAsync(discRedemption);

                        var discountRedemption = new DiscountRedemption
                        {
                            DiscountId = discountId,
                            DiscountCode = discount.Code,
                            OrderId = order.Id,
                            OrderNbr = order.FormNbr,
                            CustomerId = order.CustomerId.Value,
                            OriginalAmount = order.TotalGrossAmount,
                            DiscountAmount = discountAmount,
                            DiscountedAmount = order.TotalGrossAmount - discountAmount,
                            RedeemedAt = DateTime.UtcNow,
                            IsPending = true
                        };
                        await _discountService.CreateDiscountRedemptionAsync(discountRedemption);
                    }
                }
            }
            else
            {
                var discountRedemption = new DiscountRedemption
                {
                    DiscountId = discountId,
                    DiscountCode = discount.Code,
                    OrderId = order.Id,
                    OrderNbr = order.FormNbr,
                    CustomerId = order.CustomerId.Value,
                    OriginalAmount = order.TotalGrossAmount,
                    DiscountAmount = discountAmount,
                    DiscountedAmount = order.TotalGrossAmount - discountAmount,
                    RedeemedAt = DateTime.UtcNow,
                    IsPending = true
                };
                await _discountService.CreateDiscountRedemptionAsync(discountRedemption);
            }
        }
        #endregion
    }
}
