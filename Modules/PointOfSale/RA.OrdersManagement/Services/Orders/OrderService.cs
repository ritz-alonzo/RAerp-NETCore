using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Orders;
using RA.WebFramework.Extensions;
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
        public async Task<IEnumerable<Order>> GetOrderListAsync(
            string searchQuery = null,
            string searchCustomerName = null,
            List<Guid> searchServiceIds = null,
            DateTime? searchOrderDate = null,
            DateTime? searchCreatedDate = null,
            List<int> formStatusIds = null,
            bool showDeleted = false)
        {
            var query = GetFormListAsync().Result.AsQueryable();
            
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

            return await query.ToListAsync();
        }

        public override Order CreateTempForm()
        {
            var tempOrderForm = base.CreateTempForm();
            tempOrderForm.TotalQty = 0m;
            tempOrderForm.TotalNetAmount = 0m;
            tempOrderForm.OrderDate = DateTime.Now;
            return tempOrderForm;
        }
        #endregion
    }
}
