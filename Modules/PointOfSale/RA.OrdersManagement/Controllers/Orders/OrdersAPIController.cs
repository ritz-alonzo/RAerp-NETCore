using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.FormTypes.Services;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Services.Carts;
using RA.OrdersManagement.Services.Orders;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
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
        #endregion

        #region Ctor
        public OrdersAPIController(IAccessControl accessControl,
            IUserIdentity userIdentity,
            IOrderService orderService,
            ICartService cartService,
            IMapper mapper,
            IFormTypeManager formTypeManager)
        {
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _orderService = orderService;
            _cartService = cartService;
            _mapper = mapper;
            _formTypeManager = formTypeManager;
            _orderSettings = _formTypeManager.GetSettingDataOfFormAsync<Order, OrderSetting>().Result;
        }
        #endregion

        #region Version 1.0

        #region Order
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetOrderList([FromQuery] OrderSearchModel orderSearchModel)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

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
            if (!string.IsNullOrEmpty(idOrFormNbr))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId or FormNbr doesn't have value"));

            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

            Order order = null;
            if (Guid.TryParse(idOrFormNbr, out Guid result))
                order = await _orderService.GetFormByIdAsync(result);
            else
                order = await _orderService.GetFormByFormNbrAsync(idOrFormNbr);

            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order doesn't exists"));

            return Ok(GenerateResponseModel<Order>(HttpStatusCode.OK, "Successful", order));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order is empty"));

            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            order.CreatedById = currentUser.Id;
            order.OrderDate = DateTime.UtcNow;
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
                order.TotalNetAmount = order.TotalGrossAmount * 1.12m;
                #endregion

                order.OrderDate = order.OrderDate == DateTime.MinValue ? DateTime.UtcNow : order.OrderDate;

                await _orderService.UpdateFormAsync(order);
            }

            return Ok(GenerateResponseModel<Order>(HttpStatusCode.OK, "Successfully Created Order", order));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateOrder([FromBody] Order order)
        {
            if (order == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order is empty"));

            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

            if (_orderSettings == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order configuration not yet configured"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            if (currentUser == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "User doesn't exists please login"));

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
                order.TotalNetAmount = (order.TotalGrossAmount - order.TotalDiscountAmount) * 1.12m;
                #endregion
                order.OrderDate = DateTime.UtcNow;
            }
            await _orderService.UpdateFormAsync(order);

            return Ok(GenerateResponseModel<Order>(HttpStatusCode.OK, "Successfully Updated Order", order));
        }

        [HttpDelete("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteOrder(string idOrFormNbr)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

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

            return Ok(GenerateResponseModel<Order>(HttpStatusCode.OK, "Successfully Deleted Order"));
        }
        #endregion

        #region Order Item
        [HttpGet("item/{formId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetOrderItemList(string formId)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

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
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

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
        public async Task<IActionResult> UpdateOrderItems([FromBody] List<OrderItem> orderItems)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

            if (!orderItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            foreach (var item in orderItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                item.ModifiedById = currentUser?.Id;
                await _orderService.UpdateItemAsync(item, saveChangesToDb: true);
            }

            return Ok(GenerateListResponseModel<OrderItem>(HttpStatusCode.OK, "Successfully added order items", dataList: orderItems));
        }

        [HttpDelete("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteOrderItem([FromBody] List<OrderItem> orderItems)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

            if (!orderItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Order item doesn't have value"));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            foreach (var item in orderItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

                item.ModifiedById = currentUser?.Id;
                await _orderService.DeleteItemAsync(item, saveChangesToDb: true);
            }

            return Ok(GenerateListResponseModel<OrderItem>(HttpStatusCode.OK, "Successfully added order items", dataList: orderItems));
        }
        #endregion

        #endregion

        #region Version 2.0
        [HttpGet, MapToApiVersion("2.0")]
        public async Task<IActionResult> GetOrderListv2([FromQuery] OrderSearchModel orderSearchModel)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

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
    }
}
