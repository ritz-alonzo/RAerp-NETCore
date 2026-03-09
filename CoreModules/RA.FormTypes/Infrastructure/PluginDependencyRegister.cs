using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RA.FormTypes.Controllers;
using RA.FormTypes.Factories;
using RA.FormTypes.Services;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Infrastructure
{
    public class PluginDependencyRegister : IPluginStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddMvc()
            .AddApplicationPart(typeof(FormTypesController).Assembly);
            services.AddTransient<IFormTypeManager, FormTypeManager>();
            services.AddTransient<IFormTypeModelFactory, FormTypeModelFactory>();
            services.AddTransient<IBaseFormModelFactory, BaseFormModelFactory>();
            //Validators
            //services.AddTransient<IValidator<EntityTypeModel>, EntityTypeValidator>();
        }
    }
}
