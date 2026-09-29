using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Services;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.Core.PluginData.FormTypes;
using RA.Inventory.Domain;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.DTO.Carts;
using RA.OrdersManagement.Services.Carts;
using RA.OrdersManagement.Services.Orders;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Domain.Application;
using RAerp.Domain.Users;
using RAerp.Helpers.UserHelper;
using RAerp.Models.ApiModel;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.UserServices;
using System.Net;

namespace RA.OrdersManagement.Controllers.Carts
{
    [ApiController]
    [Route("api/v{version:apiVersion}/carts")]
    [ApiVersion("1.0")]
    [Authorize]
    public class CartsAPIController : AdminApiController
    {
        #region Constants
        private readonly IAccessControl _accessControl;
        private readonly IUserIdentity _userIdentity;
        private readonly ICartService _cartService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        private readonly IMapper _mapper;
        private readonly IOrderService _orderService;
        private readonly IBusinessEntityService _businessEntityService;
        #endregion

        #region Ctor
        public CartsAPIController(IAccessControl accessControl,
            IUserIdentity userIdentity,
            ICartService cartService,
            IApplicationSettingService applicationSettingService,
            IMapper mapper,
            IOrderService orderService,
            IBusinessEntityService businessEntityService)
        {
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _cartService = cartService;
            _applicationSettingService = applicationSettingService;
            _applicationSetting = _applicationSettingService.GetCurrentApplicationSettingAsync()?.Result;
            _mapper = mapper;
            _orderService = orderService;
            _businessEntityService = businessEntityService;
        }
        #endregion

        #region Cart
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartList([FromQuery] CartQueryRequestDto cartSearchModel)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            List<CartResponseDto> cartResponseList = new List<CartResponseDto>();
            var cartList = await _cartService.GetCartPagedResultListAsync(
                    searchQuery: cartSearchModel?.SearchQuery,
                    searchCustomerName: cartSearchModel?.SearchCustomerName,
                    searchCustomerId: cartSearchModel?.SearchCustomerId,
                    searchServiceIds: cartSearchModel.SearchServiceIds,
                    searchCreatedOn: cartSearchModel.SearchCreatedOn,
                    formStatusIds: cartSearchModel.SearchStatusIds,
                    showDeleted: cartSearchModel.ShowDeleted,
                    pageSize: cartSearchModel.PageSize, pageNumber: cartSearchModel.PageNumber
                );
            if (cartList.Items.Any())
            {
                cartResponseList = cartList.Items.Select(cart =>
                {
                    CartResponseDto cartResponse = _mapper.Map<CartResponseDto>(cart);
                    return cartResponse;

                }).ToList();
            }

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successful", cartSearchModel.PageNumber, cartSearchModel.PageSize, cartResponseList));
        }

        [HttpGet("current"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartByCurrentUser()
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            User currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            BusinessEntity customerEntity = await _businessEntityService.GetBusinessEntityByUserId(currentUser.Id);
            if (customerEntity == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Customer entity doesn't exists"));

            Cart cart = await _cartService.GetCartByCustomerIdAsync(customerEntity.Id);
            if (cart == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart doesn't exists"));

            CartResponseDto cartResponse = _mapper.Map<CartResponseDto>(cart);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successful", cartResponse));
        }

        [HttpGet("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartByIdOrFormNbr(string idOrFormNbr)
        {
            if (string.IsNullOrEmpty(idOrFormNbr))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId or FormNbr doesn't have value"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            Cart cart = null;
            if (Guid.TryParse(idOrFormNbr, out Guid id))
                cart = await _cartService.GetFormByIdAsync(id);
            else
                cart = await _cartService.GetFormByFormNbrAsync(idOrFormNbr);

            if (cart == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart doesn't exists"));

            CartResponseDto cartResponse = _mapper.Map<CartResponseDto>(cart);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successful", cartResponse));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateCart([FromBody] CartRequestDto cartRequest)
        {
            if (cartRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            Cart cart = _mapper.Map<Cart>(cartRequest);
            cart.CreatedById = currentUser.Id;
            await _cartService.InsertFormAsync(cart);

            CartResponseDto cartResponse = _mapper.Map<CartResponseDto>(cart);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully Created Cart", cartResponse));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateCart([FromBody] CartRequestDto cartRequest)
        {
            if (cartRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart is empty"));

            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (cartRequest.Id.IsNullOrEmpty())
                return NotFound("Cart Id doesn't exists");

            Cart existingCart = await _cartService.GetFormByIdAsync(cartRequest.Id);
            if (existingCart == null)
                return NotFound("Cart doesn't exists");

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            Cart cart = _mapper.Map(cartRequest, existingCart);

            cart.ModifiedById = currentUser?.Id;
            await _cartService.UpdateFormAsync(cart);

            CartResponseDto cartResponse = _mapper.Map<CartResponseDto>(cart);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully Updated Cart", cartResponse));
        }

        [HttpDelete("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteCart(string idOrFormNbr)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            Cart cart = null;
            if (Guid.TryParse(idOrFormNbr, out Guid id))
                cart = await _cartService.GetFormByIdAsync(id);
            else
                cart = await _cartService.GetFormByFormNbrAsync(idOrFormNbr);

            if (cart == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart doesn't exists"));
            
            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            cart.ModifiedById = currentUser?.Id;
            await _cartService.DeleteFormAsync(cart);

            return Ok("Successfully Deleted Cart");
        }

        [HttpPost("checkout/{formId:guid}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> CheckoutCart(Guid formId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (formId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is not provided"));

            Cart cart = await _cartService.GetFormByIdAsync(formId);
            if (cart == null) 
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart doesn't exists"));

            List<CartItem> cartItems = (await _cartService.GetItemsByFormIdAsync(formId)).ToList();
            if (!cartItems.Any())
                return BadRequest(GenerateErrorResponseModel(HttpStatusCode.BadRequest, "Cart doesn't have any items"));

            Order order = await _orderService.GetPendingOrderByCartIdAsync(cart.Id);
            List<OrderItem> orderItems = new List<OrderItem>();
            if (order == null)
            {
                // Create a new pending order based on cart items
                Order newOrder = new Order
                {
                    CartId = cart.Id,
                    ServiceId = cart.ServiceId,
                    CustomerId = cart.CustomerId,
                    CustomerName = cart.CustomerName,
                    OrderDate = DateTime.UtcNow,
                    Status = FormStatus.Pending,
                    Description = cart.Description,
                    TotalQty = cart.TotalQty,
                    TotalDiscountAmount = 0m, // Calculate discount if applicable
                    TotalGrossAmount = cart.TotalAmount, // Assuming no discount or VAT for simplicity
                    TotalVatAmount = cart.TotalAmount * 0.12m, // Calculate VAT if applicable
                    TotalNetAmount = cart.TotalAmount // Assuming no discount or VAT for simplicity
                };
                await _orderService.InsertFormAsync(newOrder);
                order = newOrder;
            }
            else
            {
                // Update the existing pending order based on cart items
                order.ServiceId = cart.ServiceId;
                order.CustomerId = cart.CustomerId;
                order.CustomerName = cart.CustomerName;
                order.OrderDate = DateTime.UtcNow;
                order.Status = FormStatus.Pending;
                order.Description = cart.Description;
                order.TotalQty = cart.TotalQty;
                order.TotalDiscountAmount = 0m;
                order.TotalGrossAmount = cart.TotalAmount; // Assuming no discount or VAT for simplicity
                order.TotalVatAmount = cart.TotalAmount * 0.12m; // Calculate VAT if applicable
                order.TotalNetAmount = cart.TotalAmount; // Assuming no discount or VAT for simplicity
                await _orderService.UpdateFormAsync(order);

                orderItems = (await _orderService.GetItemsByFormIdAsync(order.Id)).ToList();
            }

            // Remove existing order items that are not in the cart anymore
            if (orderItems != null && orderItems.Any())
            {
                var cartItemCatalogIds = cartItems.Select(c => c.CatalogId).ToList();
                var orderItemsToRemove = orderItems.Where(oi => !cartItemCatalogIds.Contains(oi.CatalogId)).ToList();
                foreach (var orderItem in orderItemsToRemove)
                {
                    await _orderService.DeleteItemAsync(orderItem, true);
                }
            }

            foreach (CartItem cartItem in cartItems)
            {
                if (orderItems != null && orderItems.Any(c => c.CatalogId == cartItem.CatalogId))
                {
                    // Update existing order item
                    OrderItem existingOrderItem = orderItems.First(c => c.CatalogId == cartItem.CatalogId);
                    existingOrderItem.Qty = cartItem.Qty;
                    existingOrderItem.Price = cartItem.Price;
                    existingOrderItem.SubTotal = cartItem.SubTotal;
                    existingOrderItem.DiscountAmount = 0m;
                    await _orderService.UpdateItemAsync(existingOrderItem, true);
                }
                else
                {
                    // Add new order item
                    OrderItem newOrderItem = new OrderItem
                    {
                        FormId = order.Id,
                        CatalogId = cartItem.CatalogId,
                        Qty = cartItem.Qty,
                        Price = cartItem.Price,
                        SubTotal = cartItem.SubTotal,
                        DiscountAmount = 0m
                    };
                    await _orderService.InsertItemAsync(newOrderItem, true);
                }
            }

            return Created();
        }
        #endregion

        #region Cart Item
        [HttpGet("item/{formId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartItemList(string formId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            List<CartItemResponseDto> cartItemResponseList = new List<CartItemResponseDto>();
            if (Guid.TryParse(formId, out Guid result))
            {
                if (result.IsNotNullOrEmpty())
                {
                    var cartItemList = _cartService.GetItemsByFormIdAsync(result).Result.ToList();
                    if (cartItemList.Any())
                    {
                        cartItemResponseList = cartItemList.Select(cartItem =>
                        {
                            CartItemResponseDto cartItemResponse = _mapper.Map<CartItemResponseDto>(cartItem);
                            return cartItemResponse;
                        }).ToList();
                    }
                }
            }

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Succesful", dataList: cartItemResponseList));
        }

        [HttpGet("item/{formId}/{catalogId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartItemByFormIdAndCatalogId(string formId, string catalogId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (string.IsNullOrEmpty(formId) || string.IsNullOrEmpty(catalogId))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Both FormId and CatalogId should have values"));

            Guid? cartItemFormId = null;
            Guid? cartItemCatalogId = null;
            if (Guid.TryParse(formId, out Guid formIdResult))
                cartItemFormId = formIdResult;

            if (Guid.TryParse(catalogId, out Guid catalogIdResult))
                cartItemCatalogId = catalogIdResult;

            if (cartItemFormId.IsNullOrEmpty() || cartItemCatalogId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId or CatalogId is invalid"));

            var cartItem = await _cartService.GetItemByFormIdAndCatalogId(cartItemFormId.Value, cartItemCatalogId.Value);

            CartItemResponseDto cartItemResponse = _mapper.Map<CartItemResponseDto>(cartItem);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfull", cartItemResponse));
        }

        [HttpPost("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> InsertCartItems([FromBody] List<CartItemRequestDto> cartItemRequestList)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (!cartItemRequestList.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart item doesn't have value"));

            List<CartItem> cartItems = _mapper.Map<List<CartItem>>(cartItemRequestList);

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);
            
            var formId = Guid.Empty;
            decimal totalQty = 0m;
            decimal totalAmount = 0m;
            foreach (var item in cartItems)
            {
                if (item.FormId.IsNullOrEmpty())
                    return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));
                formId = item.FormId;
                // Additional validation for Line Nbr
                int? latestLineNbr = 0;
                var cartItemsList = await _cartService.GetItemsByFormIdAsync(item.FormId);
                if (cartItemsList.Any())
                    latestLineNbr = cartItemsList.Max(c => c.LineNbr);

                item.CreatedById = currentUser.Id;
                item.LineNbr = latestLineNbr.GetValueOrDefault() <= 0 ? 1 : latestLineNbr.Value + 1;
                totalQty += item.Qty;
                totalAmount += item.SubTotal;
                await _cartService.InsertItemAsync(item, saveChangesToDb: true);
            }

            // Update cart totals here
            if (formId.IsNotNullOrEmpty())
            {
                var cart = await _cartService.GetFormByIdAsync(formId);
                if (cart != null)
                {
                    cart.TotalQty = totalQty;
                    cart.TotalAmount = totalAmount;
                    await _cartService.UpdateFormAsync(cart);
                }
            }

            List<CartItemResponseDto> cartItemResponseList = _mapper.Map<List<CartItemResponseDto>>(cartItems);

            return Ok(GenerateListResponseModel(HttpStatusCode.OK, "Successfully added cart items", dataList: cartItemResponseList));
        }

        [HttpPut("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateCartItem([FromBody] CartItemRequestDto cartItemRequest)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            if (cartItemRequest == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart item doesn't have value"));
            
            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (cartItemRequest.FormId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

            CartItem cartItem = _mapper.Map<CartItem>(cartItemRequest);
            
            var existingCartItem = await _cartService.GetItemByIdAsync(cartItemRequest.Id);
            if (existingCartItem == null)
            {
                // Create cart item
                cartItem.CreatedById = currentUser.Id;
                await _cartService.InsertItemAsync(existingCartItem, saveChangesToDb: true);
            }
            else
            {
                // Update cart item
                existingCartItem.Qty = cartItem.Qty;
                existingCartItem.Price = cartItem.Price;
                existingCartItem.SubTotal = cartItem.SubTotal;
                existingCartItem.ModifiedById = currentUser?.Id;
                await _cartService.UpdateItemAsync(existingCartItem, saveChangesToDb: true);
            }

            // Update cart totals here
            var cart = await _cartService.GetFormByIdAsync(cartItem.FormId);
            if (cart != null)
            {
                if (cartItem.Qty > existingCartItem.Qty)
                {
                    cart.TotalQty += cartItem.Qty;
                    cart.TotalAmount += cartItem.Price;
                }
                else if (cartItem.Qty < existingCartItem.Qty)
                {
                    cart.TotalQty -= cartItem.Qty;
                    cart.TotalAmount -= cartItem.Price;
                }
                cart.ModifiedById = currentUser?.Id;
                await _cartService.UpdateFormAsync(cart);
            }

            CartItemResponseDto cartItemResponse = _mapper.Map<CartItemResponseDto>(cartItem);

            return Ok(GenerateResponseModel(HttpStatusCode.OK, "Successfully updated cart items", cartItemResponse));
        }

        [HttpDelete("item/{itemId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteCartItem(string itemId)
        {
            ApiValidationModel validationModel = await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity, _applicationSetting);
            if (validationModel != null && validationModel.IsPassed == false)
                return StatusCode((int)validationModel.StatusCode, GenerateErrorResponseModel(validationModel.StatusCode, validationModel.Message));

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            CartItem cartItem = null;
            if (Guid.TryParse(itemId, out Guid result))
                cartItem = await _cartService.GetItemByIdAsync(result);

            if (cartItem == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart item doesn't have value"));

            if (cartItem.FormId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));

            cartItem.ModifiedById = currentUser?.Id;
            await _cartService.DeleteItemAsync(cartItem, saveChangesToDb: true);

            return Ok("Successfully removed from cart");
        }
        #endregion
    }
}
