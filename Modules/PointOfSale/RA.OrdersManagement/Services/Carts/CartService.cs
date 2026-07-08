using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.WebFramework.Extensions;
using RAerp.Services.DataChangeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Services.Carts
{
    public class CartService : FormItemService<Cart, CartItem, CartSetting, RAOrderManagementContext>, ICartService
    {
        #region Constants
        private readonly RAOrderManagementContext _erpContext;
        private readonly DbSet<Cart> _cart;
        private readonly DbSet<CartItem> _cartItem;
        #endregion

        #region Ctor
        public CartService(RAOrderManagementContext erpContext,
            ICacheManager<Cart> cacheFormManager,
            ICacheManager<CartItem> cacheFormItemManager,
            IFormTypeManager formTypeManager,
            IDataChangeService dataChangeService)
            : base(erpContext, cacheFormManager, cacheFormItemManager, formTypeManager, dataChangeService)
        {
            _erpContext = erpContext;
            _cart = _erpContext.Set<Cart>();
            _cartItem = _erpContext.Set<CartItem>();
        }
        #endregion

        #region CRUD
        public async Task<IEnumerable<Cart>> GetCartListAsync(
            string searchQuery = null,
            string searchCustomerName = null,
            List<Guid> searchServiceIds = null,
            DateTime? searchCreatedDate = null,
            List<int> formStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            var query = await GetFormListAsync();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.FormNbr.ToLower().Contains(searchQuery.ToLower()));

            if (!string.IsNullOrEmpty(searchCustomerName))
                query = query.Where(c =>
                c.CustomerName.ToLower().Contains(searchCustomerName.ToLower()));

            if (searchServiceIds.HasAny())
                query = query.Where(c =>
                searchServiceIds.Contains(c.ServiceId.Value));

            if (formStatusIds.HasAny())
                query = query.Where(c =>
                formStatusIds.Contains(c.StatusId));

            if (searchCreatedDate.HasValue)
                query = query.Where(c =>
                c.CreatedOn.Date.Equals(searchCreatedDate.Value));

            if (showDeleted)
                query = query.Where(c => !c.Deleted);

            return await ToPagedListAsync(query, pageNumber ?? 0, pageSize ?? 0);
        }

        public override Cart CreateTempForm()
        {
            var tempOrderForm = base.CreateTempForm();
            tempOrderForm.TotalQty = 0m;
            tempOrderForm.TotalAmount = 0m;
            return tempOrderForm;
        }
        #endregion
    }
}
