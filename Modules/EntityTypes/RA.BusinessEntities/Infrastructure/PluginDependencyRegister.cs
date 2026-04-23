using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RA.BusinessEntities.App_Data;
using RA.BusinessEntities.Controllers;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Factories;
using RA.BusinessEntities.Mapping;
using RA.BusinessEntities.Services;
using RA.Core.DataCaching.CacheManagement;
using RA.EntityTypes.Services;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup 
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // data context
            services.AddDbContextPool<RABusinessEntityContext>(c => c.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
            // controller
            services.AddMvc()
            .AddApplicationPart(typeof(BusinessEntitiesController).Assembly);
            // caching
            services.AddTransient<ICacheManager<BusinessEntity>, CacheManager<BusinessEntity>>();
            // service
            services.AddTransient<IBusinessEntityService, BusinessEntityService>();
            // factory
            services.AddTransient<IBusinessEntityModelFactory, BusinessEntityModelFactory>();
            // automapper profile
            services.AddAutoMapper(cfg => { cfg.AddProfile<BusinessEntityMappingProfile>(); });
        }
    }
}
