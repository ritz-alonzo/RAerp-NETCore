using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.OrdersManagement.Domain.Orders;

namespace RA.OrdersManagement.Factories.Orders
{
    public interface IOrderModelFactory
    {
        Task<OrderConfigureModel> PrepareOrderConfigureModelAsync(string systemName);
        Task<OrderItemListModel> PrepareOrderItemListModelAsync(OrderModel orderModel, int pageNumber);
        Task<OrderListModel> PrepareOrderListModelAsync(OrderSearchModel searchModel);
        Task<OrderModel> PrepareOrderModelAsync(OrderModel orderModel, Order orderForm);
        Task<OrderSearchModel> PrepareOrderSearchModelAsync(OrderSearchModel searchModel, int pageSize, int pageNumber);
    }
}