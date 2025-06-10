using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MMS.Factories.Factories.EntityTypeFactory;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.EntityTypes.Controllers;
using RA.EntityTypes.Data;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Mapping;
using RA.EntityTypes.Services;
using RA.EntityTypes.Validators;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.EntityTypes.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc()
            .AddApplicationPart(typeof(EntityTypesController).Assembly);
            services.AddTransient<IEntityTypeManager, EntityTypeManager>();
            services.AddTransient<IBaseEntityModelFactory, BaseEntityModelFactory>();
            services.AddTransient<IEntityTypeModelFactory, EntityTypeModelFactory>();
            services.AddAutoMapper(typeof(MappingProfile));
            //services.AddDbContextPool<TestContext>(c => c.UseSqlServer("CurrentConnection"));
            //services.AddAutoMapper(typeof(EntityTypeMappingProfile));
            // Validators
            services.AddTransient<IValidator<EntityTypeModel>, EntityTypeValidator>();
        }
    }
}
