using FluentMigrator.Runner;
using Microsoft.EntityFrameworkCore;
using RA.Data.App_Data;
using RA.Data.Data;
using RA.Data.Domain.Users;
using RAerp.Helpers.Constants;
using RAerp.Helpers.PluginHelper;
using RAerp.Helpers.Security;
using RAerp.PluginServiceProvider;

namespace RAerp.Extensions
{
    public static class AppBuilderExtensions
    {
        public static void ApplyFluentMigrations(this IApplicationBuilder app)
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
        }

        public static void EnvironmentVariables(this IApplicationBuilder app, IWebHostEnvironment env)
        {
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
        }

        public static void ApplyApplicationMiddlewares(this IApplicationBuilder app)
        {
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            // needed for Angular
            app.UseCors(options => options.WithOrigins("http://localhost:4200", "http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader());
            // end

            app.UseRouting();
            app.UseAuthorization();

            // for WebServiceEndpoint plugin
            app.UseSwagger();
            app.UseSwaggerUI();

            // rate limiting for requests
            app.UseRateLimiter();

            app.UseEndpoints(endpoints =>
            {
                // will change this to User/Login as start up page
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Users}/{action=Login}/{id?}");
            });
        }

        #region Methods
        private static void PluginTypeInstall(IApplicationBuilder app)
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

        private static void SuperAdminCreation(RAerpContext erpContext)
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
                    IsVerified = true,
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
        #endregion
    }
}
