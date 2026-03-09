using RA.Core.Models.PluginModels.OrdersManagement.Payments;
using RA.OrdersManagement.Domain.Payments;

namespace RA.OrdersManagement.Factories.Payments
{
    public interface IPaymentModelFactory
    {
        Task<PaymentConfigureModel> PreparePaymentConfigureModelAsync(string systemName);
        Task<PaymentItemListModel> PreparePaymentItemListModelAsync(PaymentModel paymentModel, int pageNumber);
        Task<PaymentListModel> PreparePaymentListModelAsync(PaymentSearchModel searchModel);
        Task<PaymentModel> PreparePaymentModelAsync(PaymentModel paymentModel, Payment paymentForm);
        Task<PaymentSearchModel> PreparePaymentSearchModelAsync(PaymentSearchModel searchModel, int pageSize, int pageNumber);
    }
}