using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;
using RA.WebFramework.Models.Pagination;

namespace RA.OrdersManagement.Services.Carts
{
    public interface ICartService : IFormItemService<Cart, CartItem, CartSetting, RAOrderManagementContext>
    {
        Task<IEnumerable<Cart>> GetCartListAsync(string searchQuery = null, 
            string searchCustomerName = null, Guid? searchCustomerId = null, List<Guid> searchServiceIds = null, DateTime? searchCreatedOn = null, List<int> formStatusIds = null, bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);
        Task<PagedResult<Cart>> GetCartPagedResultListAsync(
            string searchQuery = null,
            string searchCustomerName = null,
            Guid? searchCustomerId = null,
            List<Guid> searchServiceIds = null,
            DateTime? searchCreatedOn = null,
            List<int> formStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);
        Task<Cart> GetCartByCustomerUserNameAsync(string customerUserName);
        Task<Cart> GetCartByCustomerIdAsync(Guid customerId);
    }
}