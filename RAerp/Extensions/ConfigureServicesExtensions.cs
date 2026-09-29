using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using FluentMigrator.Runner;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using RA.Core.DataCaching.CacheManagement;
using RAerp.App_Data;
using RAerp.Domain.EntityAttributes;
using RAerp.Domain.Users;
using RAerp.Factories.AccessRightsFactory;
using RAerp.Factories.ApplicationSettingFactory;
using RAerp.Factories.CoreFactories;
using RAerp.Factories.UserFactory;
using RAerp.Helpers.HtmlHelper;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.UserHelper;
using RAerp.Mapping;
using RAerp.Models.ApplicationSettingsModel;
using RAerp.Models.EmailModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using RAerp.Security.Idempontency.Services.Idempotency;
using RAerp.Security.Idempontency.Services.RedisCacheService;
using RAerp.Security.Idempontency.Services.SqlLock;
using RAerp.Services.AccessRightsServices;
using RAerp.Services.AddressServices;
using RAerp.Services.ApplicationServices;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.Configurations;
using RAerp.Services.DataChangeServices;
using RAerp.Services.EmailServices;
using RAerp.Services.EntityAttributeServices;
using RAerp.Services.ExternalLoginServices;
using RAerp.Services.FileServices;
using RAerp.Services.PluginNavigationServices;
using RAerp.Services.UserServices;
using RAerp.Validators;
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

            EnsureDatabaseExists(configuration.GetConnectionString("CurrentConnection"));
        }

        static void EnsureDatabaseExists(string connectionString)
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";

            using var connection = new Microsoft.Data.SqlClient.SqlConnection(builder.ConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"""
                IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{databaseName}')
                BEGIN
                    CREATE DATABASE [{databaseName}];
                END
                """;
            command.ExecuteNonQuery();
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
            services.AddTransient<IEntityAttributeService, EntityAttributeService>();
            services.AddTransient<ICacheManager<EntityAttribute>, CacheManager<EntityAttribute>>();
            services.AddTransient<ICacheManager<EntityAttributeOption>, CacheManager<EntityAttributeOption>>();
            services.AddTransient<ICacheManager<EntityAttributeValue>, CacheManager<EntityAttributeValue>>();

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
            // Application Settings
            services.AddScoped<IApplicationSettingService, ApplicationSettingService>();
            services.AddScoped<IApplicationSettingModelFactory, ApplicationSettingModelFactory>();
            services.AddScoped<IValidator<ApplicationSettingModel>, ApplicationSettingValidator>();
            // Main Services
            services.AddTransient<IFileService, FileService>();
            // Idempontecy Services
            services.AddDistributedMemoryCache();
            services.AddTransient<ISqlLockService, SqlLockService>();
            services.AddTransient<IRedisCacheService, RedisCacheService>();
            services.AddTransient<IIdempotencyService, IdempotencyService>();
            // External Login Services
            services.AddTransient<IExternalLoginService, ExternalLoginService>();
            services.AddTransient<ICacheManager<ExternalLogin>, CacheManager<ExternalLogin>>();
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
            // API Versioning configuration
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                // Choose versioning strategy
                options.ApiVersionReader = ApiVersionReader.Combine(
                    new UrlSegmentApiVersionReader(),
                    new QueryStringApiVersionReader("version"),
                    new HeaderApiVersionReader("x-api-version")
                );
            })
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV"; // v1, v2
                options.SubstituteApiVersionInUrl = true;
            });

            // webservice endpoint api
            services.AddSwaggerGen(options =>
            {
                // Resolves duplicate actions by choosing the first one (common in versioning)
                options.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

                // Automatically hooks into the API Explorer version groups (v1, v2, etc.)
                using (var serviceProvider = services.BuildServiceProvider())
                {
                    var provider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();
                    foreach (var description in provider.ApiVersionDescriptions)
                    {
                        options.SwaggerDoc(description.GroupName, new Microsoft.OpenApi.Models.OpenApiInfo
                        {
                            Title = $"My Web Service API {description.ApiVersion}",
                            Version = description.ApiVersion.ToString()
                        });
                    }
                }
            });
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

        public static void RegisterEmailConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            // For SMTP
            services.Configure<SmtpSettings>(configuration.GetSection("SmtpSettings"));
            // For Brevo
            services.Configure<BrevoEmailSettings>(configuration.GetSection("BrevoEmailSettings"));
            services.AddTransient<IEmailService, EmailService>();
        }

        public static void RegisterRedisCacheConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration =
                    configuration.GetConnectionString("Redis");
            });
        }
    }
}
