using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Payments;
using RA.WebFramework.Models.Pagination;

namespace RA.OrdersManagement.Services.Payments
{
    public interface IPaymentService : IFormItemService<Payment, PaymentItem, PaymentSetting, RAOrderManagementContext>
    {
        Task<Payment> GetForConfirmationPaymentByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Payment>> GetPaymentListAsync(string searchQuery = null, string searchPaymentRefNbr = null, string searchPaymentOrderNbr = null,
            string searchCustomerName = null,
            DateTime? searchPaymentDate = null, DateTime? searchCreatedDate = null, List<int> paymentStatusIds = null, List<int> formStatusIds = null,
            List<Guid> paymentMethodIds = null, bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);

        Task<PagedResult<Payment>> GetPaymentPagedResultListAsync(
            string searchQuery = null,
            string searchPaymentRefNbr = null,
            string searchPaymentOrderNbr = null,
            string searchCustomerName = null,
            DateTime? searchPaymentDate = null,
            DateTime? searchCreatedDate = null,
            List<int> paymentStatusIds = null,
            List<int> formStatusIds = null,
            List<Guid> paymentMethodIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);
    }
}