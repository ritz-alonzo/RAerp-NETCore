using RA.FormTypes.Services;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Payments;

namespace RA.OrdersManagement.Services.Payments
{
    public interface IPaymentService : IFormItemService<Payment, PaymentItem, PaymentSetting, RAOrderManagementContext>
    {
        Task<IEnumerable<Payment>> GetPaymentListAsync(string searchQuery = null, string searchPaymentRefNbr = null, string searchPaymentOrderNbr = null, DateTime? searchPaymentDate = null, DateTime? searchCreatedDate = null, List<int> paymentStatusIds = null, List<int> formStatusIds = null, bool showDeleted = false);
    }
}