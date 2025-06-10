using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Logging;
using RAerp.Helpers.PluginHelper;
using RAerp.Models;
using System.Diagnostics;
using System.Reflection;
// test - 2

namespace RAerp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            //GetTable();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // 04-17-25 Working as dynamic db set
        // will be used for API integration of all plugin modules (CRUD)
        private void GetTable()
        {
            var modulePluginAssemblies = PluginAssemblyHelper.GetAllModulesPluginAssemblies();
            var dbContextTypeList = new List<object>();

            var corePluginAssemblies = PluginAssemblyHelper.GetAllCorePluginAssemblies();
            Type baseEntityType = null;
            if (corePluginAssemblies.Any())
            {
                foreach (var corePluginAssembly in corePluginAssemblies)
                {
                    if (baseEntityType != null) continue;

                    var baseEntityTypeClass = corePluginAssembly.GetType("RA.EntityTypes.Domain.BaseEntityType");
                    if (baseEntityTypeClass != null)
                    {
                        baseEntityType = baseEntityTypeClass;
                    }
                }
            }
            if (modulePluginAssemblies.Any())
            {
                foreach (var pluginAssembly in modulePluginAssemblies)
                {
                    var dbContextType = pluginAssembly.GetTypes().Where(c => c.IsSubclassOf(typeof(DbContext))).FirstOrDefault();
                    if (dbContextType == null) continue;

                    if (dbContextType != null)
                    {
                        // Entity DbContext
                        // Create DbContextOptionsBuilder<TContext>
                        var optionsBuilderType = typeof(DbContextOptionsBuilder<>).MakeGenericType(dbContextType);
                        var optionsBuilder = Activator.CreateInstance(optionsBuilderType);

                        // Cast to base type
                        var baseOptionsBuilder = (DbContextOptionsBuilder)optionsBuilder;

                        // Find any overload of UseSqlServer(DbContextOptionsBuilder, string)
                        var useSqlServerMethod = typeof(SqlServerDbContextOptionsExtensions)
                            .GetMethods(BindingFlags.Public | BindingFlags.Static)
                            .FirstOrDefault(m =>
                                m.Name == "UseSqlServer" &&
                                m.GetParameters().Length >= 2 &&
                                m.GetParameters()[0].ParameterType == typeof(DbContextOptionsBuilder) &&
                                m.GetParameters()[1].ParameterType == typeof(string));

                        if (useSqlServerMethod == null)
                            throw new Exception("UseSqlServer method not found.");

                        string connectionString = "CurrentConnection";
                        // Call UseSqlServer
                        useSqlServerMethod.Invoke(null, new object[] { baseOptionsBuilder, connectionString, null });

                        // Get the options property
                        // Get the correct "Options" property declared only on the generic builder type
                        var optionsProperty = optionsBuilderType.GetProperty(
                            "Options",
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly
                        );

                        if (optionsProperty == null)
                            throw new InvalidOperationException("Could not find the Options property.");

                        var dbContextOptions = optionsProperty.GetValue(optionsBuilder);

                        var dbContext = (DbContext)Activator.CreateInstance(dbContextType, dbContextOptions);

                        // EntityType Domain
                        if (baseEntityType != null)
                        {
                            var domain = pluginAssembly.GetTypes().Where(c => c.IsSubclassOf(baseEntityType)).FirstOrDefault();
                            
                            // Entity dbSet
                            var setMethod = typeof(DbContext).GetMethod("Set", Type.EmptyTypes).MakeGenericMethod(domain);

                            var dbSet = setMethod.Invoke(dbContext, null);

                            var addMethod = dbSet.GetType().GetMethod("Add");
                            //addMethod.Invoke(dbSet, new[] { domain });
                            //dbContext.SaveChanges();
                        }
                        
                    }


                }
            }
        }
    }
}