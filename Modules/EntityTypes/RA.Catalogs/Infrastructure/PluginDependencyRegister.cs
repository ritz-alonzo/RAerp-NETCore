using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RA.Catalogs.App_Data;
using RA.Catalogs.Controllers;
using RA.Catalogs.Domain;
using RA.Catalogs.Factories;
using RA.Catalogs.Mapping;
using RA.Catalogs.Services;
using RA.Core.DataCaching.CacheManagement;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // data context
            services.AddDbContextPool<RACatalogContext>(c => c.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
            // controller
            services.AddMvc()
            .AddApplicationPart(typeof(CatalogsController).Assembly);
            // caching
            services.AddTransient<ICacheManager<Catalog>, CacheManager<Catalog>>();
            // service
            services.AddTransient<ICatalogService, CatalogService>();
            // factory
            services.AddTransient<ICatalogModelFactory, CatalogModelFactory>();
            // automapper profile
            services.AddAutoMapper(cfg => { cfg.AddProfile<CatalogMappingProfile>(); });
        }
    }
}
