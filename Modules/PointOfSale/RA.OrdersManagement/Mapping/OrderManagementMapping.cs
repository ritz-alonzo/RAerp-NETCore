using AutoMapper;
using RA.Core.Models.PluginModels.FormTypes;
using RA.Core.Models.PluginModels.OrdersManagement.Carts;
using RA.Core.Models.PluginModels.OrdersManagement.Orders;
using RA.Core.Models.PluginModels.OrdersManagement.Payments;
using RA.OrdersManagement.Data;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Mapping
{
    public class OrderManagementMapping : Profile
    {
        public OrderManagementMapping()
        {
            #region Order
            CreateMap<Order, OrderModel>();
            CreateMap<OrderModel, Order>();
            CreateMap<OrderItem, OrderItemModel>();
            CreateMap<OrderItemModel, OrderItem>();
            CreateMap<OrderSetting, OrderConfigureModel>();
            CreateMap<OrderConfigureModel, OrderSetting>();

            CreateMap<OrderSetting, FormSettingsModel>();
            CreateMap<FormSettingsModel, OrderSetting>();
            #endregion

            #region Cart
            CreateMap<Cart, CartModel>();
            CreateMap<CartModel, Cart>();
            CreateMap<CartItem, CartItemModel>();
            CreateMap<CartItemModel, CartItem>();
            CreateMap<CartSetting, CartConfigureModel>();
            CreateMap<CartConfigureModel, CartSetting>();

            CreateMap<CartSetting, FormSettingsModel>();
            CreateMap<FormSettingsModel, CartSetting>();
            #endregion

            #region Payment
            CreateMap<Payment, PaymentModel>();
            CreateMap<PaymentModel, Payment>();
            CreateMap<PaymentItem, PaymentItemModel>();
            CreateMap<PaymentItemModel, PaymentItem>();
            CreateMap<PaymentSetting, PaymentConfigureModel>();
            CreateMap<PaymentConfigureModel, PaymentSetting>();

            CreateMap<PaymentSetting, FormSettingsModel>();
            CreateMap<FormSettingsModel, PaymentSetting>();
            #endregion
        }
    }
}
