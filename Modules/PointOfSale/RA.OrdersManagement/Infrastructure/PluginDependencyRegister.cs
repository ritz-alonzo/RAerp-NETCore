using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RA.Core.DataCaching.CacheManagement;
using RA.OrdersManagement.App_Data;
using RA.OrdersManagement.Controllers.Carts;
using RA.OrdersManagement.Controllers.Orders;
using RA.OrdersManagement.Controllers.Payments;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using RA.OrdersManagement.Factories.Carts;
using RA.OrdersManagement.Factories.Orders;
using RA.OrdersManagement.Factories.Payments;
using RA.OrdersManagement.Mapping;
using RA.OrdersManagement.Services.Carts;
using RA.OrdersManagement.Services.Orders;
using RA.OrdersManagement.Services.Payments;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            //data context
            services.AddDbContextPool<RAOrderManagementContext>(c => c.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
            // controller
            services.AddMvc()
                .AddApplicationPart(typeof(OrdersController).Assembly)
                .AddApplicationPart(typeof(CartsController).Assembly)
                .AddApplicationPart(typeof(PaymentsController).Assembly);
            // caching
            services.AddTransient<ICacheManager<Order>, CacheManager<Order>>();
            services.AddTransient<ICacheManager<OrderItem>, CacheManager<OrderItem>>();
            services.AddTransient<ICacheManager<Cart>, CacheManager<Cart>>();
            services.AddTransient<ICacheManager<CartItem>, CacheManager<CartItem>>();
            services.AddTransient<ICacheManager<Payment>, CacheManager<Payment>>();
            services.AddTransient<ICacheManager<PaymentItem>, CacheManager<PaymentItem>>();
            // services
            services.AddTransient<IOrderService, OrderService>();
            services.AddTransient<ICartService, CartService>();
            services.AddTransient<IPaymentService, PaymentService>();
            // factories
            services.AddTransient<IOrderModelFactory, OrderModelFactory>();
            services.AddTransient<ICartModelFactory, CartModelFactory>();
            services.AddTransient<IPaymentModelFactory, PaymentModelFactory>();
            // auto mapper profile
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<OrderManagementMapping>();
            });
        }
    }
}
