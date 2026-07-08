using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;

namespace RA.OrdersManagement.Services.Carts
{
    public interface ICartService : IFormItemService<Cart, CartItem, CartSetting, RAOrderManagementContext>
    {
        Task<IEnumerable<Cart>> GetCartListAsync(string searchQuery = null, string searchCustomerName = null, List<Guid> searchServiceIds = null, DateTime? searchCreatedDate = null, List<int> formStatusIds = null, bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);
    }
}