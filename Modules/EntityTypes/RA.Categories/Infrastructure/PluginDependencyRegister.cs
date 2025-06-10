using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RA.Categories.App_Data;
using RA.Categories.Controllers;
using RA.Categories.Domain;
using RA.Categories.Factories;
using RA.Categories.Mapping;
using RA.Categories.Services;
using RA.Core.DataCaching.CacheManagement;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // data context
            services.AddDbContextPool<RACategoryContext>(c => c.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
            // controller
            services.AddMvc()
            .AddApplicationPart(typeof(CategoriesController).Assembly);
            // caching
            services.AddTransient<ICacheManager<Category>, CacheManager<Category>>();
            // service
            services.AddTransient<ICategoryService, CategoryService>();
            // factory
            services.AddTransient<ICategoryModelFactory, CategoryModelFactory>();
            // automapper profile
            services.AddAutoMapper(typeof(CategoryMappingProfile));
        }
    }
}
