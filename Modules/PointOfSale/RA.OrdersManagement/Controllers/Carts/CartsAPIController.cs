using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.SqlServer.Server;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Services.Carts;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
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
        #endregion

        #region Ctor
        public CartsAPIController(IAccessControl accessControl,
            IUserIdentity userIdentity,
            ICartService cartService)
        {
            _accessControl = accessControl;
            _userIdentity = userIdentity;
            _cartService = cartService;
        }
        #endregion

        #region Cart
        [HttpGet, MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartList([FromQuery] CartSearchModel cartSearchModel)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

            var cartList = (await _cartService.GetCartListAsync(
                    searchQuery: cartSearchModel?.SearchQuery,
                    searchCustomerName: cartSearchModel?.SearchCustomerName,
                    searchServiceIds: cartSearchModel.SearchServiceId.HasValue ? new List<Guid> { cartSearchModel.SearchServiceId.Value } : null,
                    searchCreatedDate: cartSearchModel.SearchCreatedOn,
                    formStatusIds: cartSearchModel.SearchStatusId > 0 ? new List<int> { cartSearchModel.SearchStatusId } : null,
                    showDeleted: cartSearchModel.ShowDeleted,
                    pageNumber: cartSearchModel.PageNumber,
                    pageSize: cartSearchModel.PageSize
                )).ToList();

            return Ok(GenerateListResponseModel<Cart>(HttpStatusCode.OK, "Successful", cartSearchModel.PageNumber, cartSearchModel.PageSize, cartList));
        }

        [HttpGet("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartByIdOrFormNbr(string idOrFormNbr)
        {
            if (!string.IsNullOrEmpty(idOrFormNbr))
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId or FormNbr doesn't have value"));

            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

            Cart cart = null;
            if (Guid.TryParse(idOrFormNbr, out Guid id))
                cart = await _cartService.GetFormByIdAsync(id);
            else
                cart = await _cartService.GetFormByFormNbrAsync(idOrFormNbr);

            if (cart == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart doesn't exists"));

            return Ok(GenerateResponseModel<Cart>(HttpStatusCode.OK, "Successful", cart));
        }

        [HttpPost, MapToApiVersion("1.0")]
        public async Task<IActionResult> CreateCart([FromBody] Cart cart)
        {
            if (cart == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart is empty"));

            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            cart.CreatedById = currentUser.Id;
            await _cartService.InsertFormAsync(cart);

            return Ok(GenerateResponseModel<Cart>(HttpStatusCode.OK, "Successfully Created Cart", cart));
        }

        [HttpPut, MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateCart([FromBody] Cart cart)
        {
            if (cart == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart is empty"));

            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            cart.ModifiedById = currentUser?.Id;
            await _cartService.UpdateFormAsync(cart);

            return Ok(GenerateResponseModel<Cart>(HttpStatusCode.OK, "Successfully Updated Cart", cart));
        }

        [HttpDelete("{idOrFormNbr}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteCart(string idOrFormNbr)
        {
            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

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

            return Ok(GenerateResponseModel<Cart>(HttpStatusCode.OK, "Successfully Deleted Cart"));
        }
        #endregion

        #region Cart Item
        [HttpGet("item/{formId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartItemList(string formId)
        {
            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

            List<CartItem> cartItemList = new List<CartItem>();
            if (Guid.TryParse(formId, out Guid result))
            {
                if (result.IsNotNullOrEmpty())
                    cartItemList = _cartService.GetItemsByFormIdAsync(result).Result.ToList();
            }

            return Ok(GenerateListResponseModel<CartItem>(HttpStatusCode.OK, "Succesful", dataList: cartItemList));
        }

        [HttpGet("item/{formId}/{catalogId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> GetCartItemByFormIdAndCatalogId(string formId, string catalogId)
        {
            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

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
            
            return Ok(GenerateResponseModel<CartItem>(HttpStatusCode.OK, "Successfull", cartItem));
        }

        [HttpPost("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> InsertCartItems([FromBody] List<CartItem> cartItems)
        {
            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

            if (!cartItems.Any())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart item doesn't have value"));
            
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

            return Ok(GenerateListResponseModel<CartItem>(HttpStatusCode.OK, "Successfully added cart items", dataList: cartItems));
        }

        [HttpPut("item"), MapToApiVersion("1.0")]
        public async Task<IActionResult> UpdateCartItems([FromBody] CartItem cartItem)
        {
            await ValidateUserAccessAndCredentials<Order>(_accessControl, _userIdentity);

            if (cartItem == null)
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "Cart item doesn't have value"));
            
            var currentUser = await _userIdentity.GetCurrentApiUserAsync(HttpContext.User);

            if (cartItem.FormId.IsNullOrEmpty())
                return NotFound(GenerateErrorResponseModel(HttpStatusCode.NotFound, "FormId is invalid"));
            
            var existingCartItem = await _cartService.GetItemByIdAsync(cartItem.Id);
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

            return Ok(GenerateResponseModel<CartItem>(HttpStatusCode.OK, "Successfully added cart items", cartItem));
        }

        [HttpDelete("item/{itemId}"), MapToApiVersion("1.0")]
        public async Task<IActionResult> DeleteCartItem(string itemId)
        {
            await ValidateUserAccessAndCredentials<Cart>(_accessControl, _userIdentity);

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

            return Ok(GenerateResponseModel<CartItem>(HttpStatusCode.OK, "Successfully added cart items", cartItem));
        }
        #endregion
    }
}
