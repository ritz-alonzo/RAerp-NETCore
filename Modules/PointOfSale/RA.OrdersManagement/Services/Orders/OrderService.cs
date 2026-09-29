using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RA.Core.PluginData.FormTypes;
using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Orders;
using RA.WebFramework.Extensions;
using RA.WebFramework.Models.Pagination;
using RAerp.Services.DataChangeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Services.Orders
{
    public class OrderService : FormItemService<Order, OrderItem, OrderSetting, RAOrderManagementContext>, IOrderService
    {
        #region Constants
        private readonly RAOrderManagementContext _erpContext;
        private readonly DbSet<Order> _order;
        private readonly DbSet<OrderItem> _orderItem;
        #endregion

        #region Ctor
        public OrderService(RAOrderManagementContext erpContext,
            ICacheManager<Order> cacheFormManager,
            ICacheManager<OrderItem> cacheFormItemManager,
            IFormTypeManager formTypeManager,
            IDataChangeService dataChangeService)
            : base(erpContext, cacheFormManager, cacheFormItemManager, formTypeManager, dataChangeService)
        {
            _erpContext = erpContext;
            _order = _erpContext.Set<Order>();
            _orderItem = _erpContext.Set<OrderItem>();
        }
        #endregion

        #region CRUD
        public async Task<Order> GetPendingOrderByCartIdAsync(Guid cartId)
        {
            return await _order.FirstOrDefaultAsync(c => c.CartId == cartId && c.StatusId == (int)FormStatus.Pending);
        }

        public async Task<IEnumerable<Order>> GetOrderListAsync(
            string searchQuery = null,
            string searchCustomerName = null,
            List<Guid> searchServiceIds = null,
            DateTime? searchOrderDate = null,
            DateTime? searchCreatedDate = null,
            List<int> formStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            var query = _order.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.FormNbr.ToLower().Contains(searchQuery.ToLower()));

            if (!string.IsNullOrEmpty(searchCustomerName))
                query = query.Where(c =>
                c.CustomerName.ToLower().Contains(searchCustomerName.ToLower()));

            if (searchServiceIds.HasAny())
                query = query.Where(c =>
                searchServiceIds.Contains(c.ServiceId.Value));

            if (searchOrderDate.HasValue)
                query = query.Where(c =>
                c.OrderDate.Date.Equals(searchOrderDate.Value.Date));

            if (searchCreatedDate.HasValue)
                query = query.Where(c =>
                c.CreatedOn.Date.Equals(searchCreatedDate.Value));

            if (formStatusIds.HasAny())
                query = query.Where(c =>
                formStatusIds.Contains(c.StatusId));

            if (showDeleted)
                query = query.Where(c => !c.Deleted);

            query = query.OrderBy(c => c.OrderDate);

            return await ToPagedListAsync(query, pageNumber ?? 0, pageSize ?? 0);
        }

        public override Order CreateTempForm()
        {
            var tempOrderForm = base.CreateTempForm();
            tempOrderForm.TotalQty = 0m;
            tempOrderForm.TotalNetAmount = 0m;
            tempOrderForm.OrderDate = DateTime.UtcNow;
            return tempOrderForm;
        }
        #endregion

        #region PagedList
        public async Task<PagedResult<Order>> GetOrderPagedResultListAsync(
            string searchQuery = null,
            string searchCustomerName = null,
            List<Guid> searchServiceIds = null,
            DateTime? searchOrderDate = null,
            DateTime? searchCreatedDate = null,
            List<int> formStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            var query = _order.AsNoTracking().AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.FormNbr.ToLower().Contains(searchQuery.ToLower()));

            if (!string.IsNullOrEmpty(searchCustomerName))
                query = query.Where(c =>
                c.CustomerName.ToLower().Contains(searchCustomerName.ToLower()));

            if (searchServiceIds.HasAny())
                query = query.Where(c =>
                searchServiceIds.Contains(c.ServiceId.Value));

            if (searchOrderDate.HasValue)
                query = query.Where(c =>
                c.OrderDate.Date.Equals(searchOrderDate.Value.Date));

            if (searchCreatedDate.HasValue)
                query = query.Where(c =>
                c.CreatedOn.Date.Equals(searchCreatedDate.Value));

            if (formStatusIds.HasAny())
                query = query.Where(c =>
                formStatusIds.Contains(c.StatusId));

            if (showDeleted)
                query = query.Where(c => !c.Deleted);

            query = query.OrderBy(c => c.OrderDate);

            return query.ToPagedResult(pageNumber, pageSize);
        }
        #endregion
    }
}
