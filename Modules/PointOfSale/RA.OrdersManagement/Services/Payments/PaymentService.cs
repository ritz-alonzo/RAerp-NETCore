using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using RA.Core.DataCaching.CacheManagement;
using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using RA.WebFramework.Extensions;
using RAerp.Services.DataChangeServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Services.Payments
{
    public class PaymentService : FormItemService<Payment, PaymentItem, PaymentSetting, RAOrderManagementContext>, IPaymentService
    {
        #region Constants
        private readonly RAOrderManagementContext _erpContext;
        private readonly DbSet<Payment> _payment;
        private readonly DbSet<PaymentItem> _paymentItem;
        #endregion

        #region Ctor
        public PaymentService(RAOrderManagementContext erpContext,
            ICacheManager<Payment> cacheFormManager,
            ICacheManager<PaymentItem> cacheFormItemManager,
            IFormTypeManager formTypeManager,
            IDataChangeService dataChangeService)
            : base(erpContext, cacheFormManager, cacheFormItemManager, formTypeManager, dataChangeService)
        {
            _erpContext = erpContext;
            _payment = _erpContext.Set<Payment>();
            _paymentItem = _erpContext.Set<PaymentItem>();
        }
        #endregion

        #region CRUD
        public async Task<IEnumerable<Payment>> GetPaymentListAsync(
            string searchQuery = null,
            string searchPaymentRefNbr = null,
            string searchPaymentOrderNbr = null,
            DateTime? searchPaymentDate = null,
            DateTime? searchCreatedDate = null,
            List<int> paymentStatusIds = null,
            List<int> formStatusIds = null,
            bool showDeleted = false)
        {
            var query = GetFormListAsync().Result.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.FormNbr.ToLower().Contains(searchQuery.ToLower()));

            if (!string.IsNullOrEmpty(searchPaymentRefNbr))
                query = query.Where(c =>
                c.PaymentRefNbr.ToLower().Contains(searchPaymentRefNbr.ToLower()));

            if (!string.IsNullOrEmpty(searchPaymentOrderNbr))
                query = from payment in query
                        join order in _erpContext.Order
                        on payment.OrderId equals order.Id
                        where order.FormNbr.ToLower() == searchPaymentRefNbr.ToLower()
                        select payment;

            if (searchPaymentDate.HasValue)
                query = query.Where(c =>
                c.PaymentDate.Date.Equals(searchPaymentDate.Value.Date));

            if (searchCreatedDate.HasValue)
                query = query.Where(c =>
                c.CreatedOn.Date.Equals(searchCreatedDate.Value));

            if (paymentStatusIds.HasAny())
                query = query.Where(c =>
                paymentStatusIds.Contains(c.PaymentStatusId));

            if (formStatusIds.HasAny())
                query = query.Where(c =>
                formStatusIds.Contains(c.StatusId));

            if (showDeleted)
                query = query.Where(c => !c.Deleted);

            return await query.ToListAsync();
        }

        public override Payment CreateTempForm()
        {
            var tempOrderForm = base.CreateTempForm();
            tempOrderForm.AmountPaid = 0m;
            tempOrderForm.ChangeAmount = 0m;
            tempOrderForm.PaymentDate = DateTime.Now;
            return tempOrderForm;
        }
        #endregion

    }
}
