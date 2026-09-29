using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RA.Core.DataCaching.CacheManagement;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Inventory.App_Data;
using RA.Inventory.Controllers;
using RA.Inventory.Domain;
using RA.Inventory.Factories;
using RA.Inventory.Mapping;
using RA.Inventory.Services.InventoryReservationServices;
using RA.Inventory.Services.InventoryServices;
using RA.Inventory.Services.InventoryStockServices;
using RA.Inventory.Services.InventoryTransactionServices;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // data context
            services.AddDbContextPool<RAInventoryContext>(c => c.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
            // controller
            services.AddMvc()
            .AddApplicationPart(typeof(InventoryController).Assembly);
            // caching
            services.AddTransient<ICacheManager<InventoryStock>, CacheManager<InventoryStock>>();
            services.AddTransient<ICacheManager<InventoryTransaction>, CacheManager<InventoryTransaction>>();
            services.AddTransient<ICacheManager<InventoryReservation>, CacheManager<InventoryReservation>>();
            // service
            services.AddTransient<IInventoryStockService, InventoryStockService>();
            services.AddTransient<IInventoryTransactionService, InventoryTransactionService>();
            services.AddTransient<IInventoryReservationService, InventoryReservationService>();
            services.AddTransient<IInventoryManager, InventoryManager>();
            // factory
            services.AddTransient<IInventoryModelFactory, InventoryModelFactory>();
            // automapper profile
            services.AddAutoMapper(cfg => { cfg.AddProfile<InventoryMappingProfile>(); });
            // validator
            //services.AddTransient<IValidator<BusinessEntityModel>, BusinessEntityValidator>();
        }
    }
}
