using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Orders;
using RA.WebFramework.Models.Pagination;

namespace RA.OrdersManagement.Services.Orders
{
    public interface IOrderService : IFormItemService<Order, OrderItem, OrderSetting, RAOrderManagementContext>
    {
        Task<IEnumerable<Order>> GetOrderListAsync(string searchQuery = null, string searchCustomerName = null, List<Guid> searchServiceIds = null, DateTime? searchOrderDate = null, DateTime? searchCreatedDate = null, List<int> formStatusIds = null, bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);

        Task<PagedResult<Order>> GetOrderPagedResultListAsync(
            string searchQuery = null,
            string searchCustomerName = null,
            List<Guid> searchServiceIds = null,
            DateTime? searchOrderDate = null,
            DateTime? searchCreatedDate = null,
            List<int> formStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);

        Task<Order> GetPendingOrderByCartIdAsync(Guid cartId);
    }
}