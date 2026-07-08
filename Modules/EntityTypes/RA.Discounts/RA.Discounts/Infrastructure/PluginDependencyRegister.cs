using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RA.Core.DataCaching.CacheManagement;
using RA.Discounts.App_Data;
using RA.Discounts.Controllers;
using RA.Discounts.Domain;
using RA.Discounts.Factories;
using RA.Discounts.Mapping;
using RA.Discounts.Services;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // data context
            services.AddDbContextPool<RADiscountContext>(c => c.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
            // controller
            services.AddMvc()
            .AddApplicationPart(typeof(DiscountsController).Assembly);
            // caching
            services.AddTransient<ICacheManager<Discount>, CacheManager<Discount>>();
            services.AddTransient<ICacheManager<DiscountRedemption>, CacheManager<DiscountRedemption>>();
            // service
            services.AddTransient<IDiscountService, DiscountService>();
            // factory
            services.AddTransient<IDiscountModelFactory, DiscountModelFactory>();
            // automapper profile
            services.AddAutoMapper(cfg => { cfg.AddProfile<DiscountMappingProfile>(); });
        }
    }
}
