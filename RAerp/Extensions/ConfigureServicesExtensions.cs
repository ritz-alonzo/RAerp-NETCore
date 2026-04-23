using FluentMigrator.Runner;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using RA.Data.App_Data;
using RAerp.Factories.AccessRightsFactory;
using RAerp.Factories.CoreFactories;
using RAerp.Factories.UserFactory;
using RAerp.Helpers.HtmlHelper;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.UserHelper;
using RAerp.Mapping;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.AccessRightsServices;
using RAerp.Services.AddressServices;
using RAerp.Services.ApplicationServices;
using RAerp.Services.Configurations;
using RAerp.Services.DataChangeServices;
using RAerp.Services.PluginNavigationServices;
using RAerp.Services.UserServices;
using System.Reflection;

namespace RAerp.Extensions
{
    public static class ConfigureServicesExtensions
    {
        public static void RegisterFluentMigration(this IServiceCollection services)
        {
            var pluginAssemblies = new Assembly[0];
            var pluginAssembliesList = PluginAssemblyHelper.GetAllPluginAssemblies().ToList();
            // include raerp assembly in all plugin assemblies for Migrations
            pluginAssembliesList.Add(Assembly.GetExecutingAssembly());
            pluginAssemblies = pluginAssembliesList.ToArray();

            // fluent migrator for creating table in database
            services.AddFluentMigratorCore()
                .ConfigureRunner(config => config
                .AddSqlServer()
                .WithGlobalConnectionString("CurrentConnection")
                // using plugin assemblies
                .ScanIn(pluginAssemblies).For.Migrations())
                .AddLogging(log => log.AddFluentMigratorConsole());

            services.AddTransient<IMigrationRunner, MigrationRunner>();
        }

        public static void RegisterDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<RAerpContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("CurrentConnection")));
        }

        public static void RegisterDependencyLifetime(this IServiceCollection services)
        {
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            //// admin services
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserIdentity, UserIdentity>();
            services.AddTransient<IAccessRightsService, AccessRightsService>();
            services.AddTransient<IAccessControl, AccessControl>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IAddressService, AddressService>();
            services.AddTransient<IDataChangeService, DataChangeService>();
            // navigation service
            services.AddTransient<INavigationService, NavigationService>();
            // settings
            services.AddTransient<ISettingService, SettingService>();
            // admin factories
            services.AddTransient<IBaseModelFactory, BaseModelFactory>();
            services.AddTransient<IBaseAdminModelFactory, BaseAdminModelFactory>();
            services.AddTransient<IUserModelFactory, UserModelFactory>();
            services.AddTransient<IAccessRightsModelFactory, AccessRightsModelFactory>();
            // core helpers
            services.AddTransient<IModelAttributeHelper, ModelAttributeHelper>();
        }

        public static void RegisterFluentValidators(this IServiceCollection services)
        {
            // validators
            services.AddFluentValidationAutoValidation();
            //services.AddTransient<IValidator<ProductModel>, ProductValidator>();
        }

        public static void RegisterAutoMapper(this IServiceCollection services)
        {
            // auto mapper
            //services.AddAutoMapper(typeof(AdminMappingProfile));
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AdminMappingProfile>();
            });
        }

        public static void RegisterMiddlewares(this IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddMemoryCache();
            services.AddSession();
            services.AddMvc();
            services.AddRazorPages();
            // webservice endpoint api
            services.AddSwaggerGen();
        }

        public static void RegisterPluginDependency(this IServiceCollection services, IConfiguration configuration)
        {
            var pluginAssemblies = PluginAssemblyHelper.GetAllPluginAssemblies();

            if (pluginAssemblies.Any())
            {
                foreach (var assembly in pluginAssemblies)
                {
                    var pluginDependencyRegisterClass = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IPluginStartup)))
                        .FirstOrDefault();

                    if (pluginDependencyRegisterClass != null)
                    {
                        var pluginInstance = Activator.CreateInstance(pluginDependencyRegisterClass) as IPluginStartup;

                        if (pluginInstance != null)
                        {
                            pluginInstance.ConfigureServices(services, configuration);
                        }
                    }
                }
            }

            //// Get the path to the folder you're interested in (e.g. "MyFolder")
            //var path = Path.GetFullPath("D:\\RitzAlonzoSystem\\RAerp\\Plugins");

            //// Get all the folders in Plugins
            //var pluginFolders = Directory.GetDirectories(path);

            //foreach (var pluginFolder in pluginFolders)
            //{
            //    var pluginFolderName = pluginFolder.Substring(pluginFolder.LastIndexOf("RA"));

            //    // Get all the DLL files in the folder
            //    var pluginDll = Directory.GetFiles(pluginFolder, $"{pluginFolderName}.dll").FirstOrDefault();

            //    if (pluginDll != null)
            //    {
            //        // Load the assembly
            //        var assembly = Assembly.LoadFrom(pluginDll);

            //        var pluginDependencyRegisterClass = assembly.GetTypes()
            //            .Where(t => t.GetInterfaces().Contains(typeof(IPluginStartup)))
            //            .FirstOrDefault();

            //        if (pluginDependencyRegisterClass != null)
            //        {
            //            var pluginInstance = Activator.CreateInstance(pluginDependencyRegisterClass) as IPluginStartup;

            //            if (pluginInstance != null)
            //            {
            //                pluginInstance.ConfigureServices(services);
            //            }
            //        }
            //    }
            //}
        }
    }
}
