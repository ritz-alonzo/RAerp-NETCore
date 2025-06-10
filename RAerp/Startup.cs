#region namespaces
using Microsoft.EntityFrameworkCore;
using FluentMigrator.Runner;
using System.Reflection;
using FluentValidation.AspNetCore;
using RAerp.PluginServiceProvider;
using RA.Data.App_Data;
using RAerp.Services.Configurations;
using RAerp.Helpers.HtmlHelper;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.UserHelper;
using RAerp.Services.UserServices;
using RAerp.Services.PluginNavigationServices;
using RAerp.Factories.CoreFactories;
using RAerp.Factories.UserFactory;
using RAerp.Helpers.Constants;
using RA.Data.Domain.Users;
using RAerp.Helpers.Security;
using RA.Data.Data;
using RAerp.Mapping;
using RAerp.Services.AccessRightsServices;
using RAerp.Security.AccessRightsControl;
using RAerp.Factories.AccessRightsFactory;
using RAerp.Services.ApplicationServices;
using RAerp.Services.AddressServices;
#endregion

namespace RAerp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

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

            // will enable this when user activity is created in database
            //services.AddControllersWithViews(options => {
            //    options.Filters.Add(typeof(UserActivityTracker));
            //});

            services.AddDbContext<RAerpContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("CurrentConnection")));
            // http context accessor
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            //// caching manager for entities
            //services.AddTransient<ICacheManager<Category>, CacheManager<Category>>();
            //services.AddTransient<ICacheManager<Product>, CacheManager<Product>>();

            //// admin services
            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserIdentity, UserIdentity>();
            services.AddTransient<IAccessRightsService, AccessRightsService>();
            services.AddTransient<IAccessControl, AccessControl>();
            services.AddTransient<IApplicationService, ApplicationService>();
            services.AddTransient<IAddressService, AddressService>();
            //services.AddTransient<ICategoryService, CategoryService>();
            //services.AddTransient<IProductService, ProductService>();
            //// needed to add categorySettings cause it was used in ctor
            //services.AddTransient<CategorySettings>();
            //services.AddTransient<ProductSettings>();

            //// base services
            //services.AddTransient<IBaseEntitySettingService, BaseEntitySettingService>();
            //// navigation service
            services.AddTransient<INavigationService, NavigationService>();

            // settings
            services.AddTransient<ISettingService, SettingService>();

            //// admin factories
            services.AddTransient<IBaseModelFactory, BaseModelFactory>();
            services.AddTransient<IBaseAdminModelFactory, BaseAdminModelFactory>();
            services.AddTransient<IUserModelFactory, UserModelFactory>();
            services.AddTransient<IAccessRightsModelFactory, AccessRightsModelFactory>();

            // validators
            services.AddFluentValidationAutoValidation();
            //services.AddTransient<IValidator<ProductModel>, ProductValidator>();

            services.AddMemoryCache();
            services.AddSession();
            services.AddMvc();
            services.AddRazorPages();

            // webservice endpoint api
            services.AddSwaggerGen();

            // auto mapper
            services.AddAutoMapper(typeof(AdminMappingProfile));

            // core helpers
            services.AddTransient<IModelAttributeHelper, ModelAttributeHelper>();
            //services.AddTransient<IPluginNavigation, PluginNavigation>();

            PluginDependencyRegistry(services);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // to migrate tables with created migrations
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RAerpContext>();
                // to create Database based the plugin.json connection
                context.Database.Migrate();

                var migrator = scope.ServiceProvider.GetService<IMigrationRunner>();

                var haveMigrationsToApply = migrator.HasMigrationsToApplyUp();
                if (haveMigrationsToApply)
                    migrator.MigrateUp();

                migrator.MigrateUp();
                // run code on start up
                //appLifetime.ApplicationStarted.Register(OnStarted);

                PluginTypeInstall(app);

                // add creation of super admin user
                SuperAdminCreation(context);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error/ErrorPage?statusCode={0}");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            // will activate when Error pages are done
            //app.UseStatusCodePagesWithRedirects("/Error/ErrorPage?statusCode={0}");

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            // needed for Angular
            app.UseCors(options => options.WithOrigins("http://localhost:4200")
            .AllowAnyMethod()
            .AllowAnyHeader());
            // end

            app.UseRouting();
            app.UseAuthorization();

            // for WebServiceEndpoint plugin
            app.UseSwagger();
            app.UseSwaggerUI();


            app.UseEndpoints(endpoints =>
            {
                // will change this to User/Login as start up page
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Users}/{action=Login}/{id?}");
            });
        }

        #region Methods

        private void SuperAdminCreation(RAerpContext erpContext)
        {
            var users = erpContext.User.ToList();

            if (users.Any())
                return;

            if (users.Any(c => c.Username.Contains(AdminMessages.SuperAdminUsername, StringComparison.InvariantCultureIgnoreCase)))
                return;

            var userRoles = erpContext.UserRole.ToList();

            if (!userRoles.Any())
            {
                var superAdminKey = EncryptionHelper.GenerateSalt();
                var superAdmin = new User()
                {
                    FirstName = AdminMessages.SuperAdminName,
                    LastName = AdminMessages.SuperAdminName,
                    Username = AdminMessages.SuperAdminUsername,
                    Password = EncryptionHelper.EncryptData("@dm!n123", superAdminKey).Result,
                    CreatedOn = DateTime.Now,
                    AccountStatus = UserAccountStatus.Active,
                    Salt = superAdminKey
                };
                erpContext.User.Add(superAdmin);

                var superAdminRole = new UserRole()
                {
                    Rolename = AdminMessages.SuperAdminRole,
                    CreatedOn = DateTime.Now
                };
                erpContext.UserRole.Add(superAdminRole);

                var superAdminMapping = new UserUserRoleMapping()
                {
                    UserId = superAdmin.Id,
                    UserRoleId = superAdminRole.Id
                };
                erpContext.UserUserRoleMapping.Add(superAdminMapping);

                erpContext.SaveChanges();
            }
        }

        private void PluginTypeInstall(IApplicationBuilder app)
        {
            // working
            var pluginAssemblies = PluginAssemblyHelper.GetAllCorePluginAssemblies();

            if (pluginAssemblies.Any())
            {
                foreach (var assembly in pluginAssemblies)
                {
                    var pluginInstallerClass = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IPluginInstallation)))
                        .FirstOrDefault();

                    if (pluginInstallerClass != null)
                    {
                        var pluginInstance = Activator.CreateInstance(pluginInstallerClass) as IPluginInstallation;

                        if (pluginInstance != null)
                        {
                            pluginInstance.PluginTypeInstall(app);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get all plugin dependency from class libraries
        /// </summary>
        /// <param name="services"></param>
        private void PluginDependencyRegistry(IServiceCollection services)
        {
            // working
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
                            pluginInstance.ConfigureServices(services, Configuration);
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

        #endregion
    }
}
