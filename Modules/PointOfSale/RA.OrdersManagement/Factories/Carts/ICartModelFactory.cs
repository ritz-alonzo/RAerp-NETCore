using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.OrdersManagement.Domain.Carts;

namespace RA.OrdersManagement.Factories.Carts
{
    public interface ICartModelFactory
    {
        Task<CartConfigureModel> PrepareCartConfigureModelAsync(string systemName);
        Task<CartItemListModel> PrepareCartItemListModelAsync(CartModel cartModel, int pageNumber);
        Task<CartListModel> PrepareCartListModelAsync(CartSearchModel searchModel);
        Task<CartModel> PrepareCartModelAsync(CartModel cartModel, Cart cartForm);
        Task<CartSearchModel> PrepareCartSearchModelAsync(CartSearchModel searchModel, int pageSize, int pageNumber);
    }
}