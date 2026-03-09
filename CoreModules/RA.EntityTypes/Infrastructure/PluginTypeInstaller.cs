using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RA.Data.App_Data;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Domain;
using RA.EntityTypes.Services;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.EntityTypes.Infrastructure
{
    public class PluginTypeInstaller : IPluginInstallation
    {
        public async Task PluginTypeInstall(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RAerpContext>();

                var entityTypeManager = Activator.CreateInstance(typeof(EntityTypeManager), context) as EntityTypeManager;

                #region Install EntityTypes
                var types = GetClassesOfBaseEntityType();

                if (types.Any())
                {
                    var typesToInstall = new List<Type>();

                    var entityTypes = entityTypeManager.GetListAsync().Result.Where(c => c.EntityClassificationName == null).Select(c => c.EntitySystemName).ToList();
                    if (entityTypes.Any())
                    {
                        var typeList = types.ToList();
                        typesToInstall = typeList.Where(c => !entityTypes.Contains(c.FullName)).ToList();
                    }
                    else
                    {
                        typesToInstall = types;
                    }
                    // there's entities to install
                    if (typesToInstall.Any())
                    {
                        // foreach all types to insert in database
                        foreach (var type in typesToInstall)
                        {
                            // sanity check to avoid duplicates
                            var entity = await entityTypeManager.GetTypeBySystemNameAsync(type.FullName);
                            if (entity != null)
                                continue;
                            // just to get the name of the assembly full name
                            var indexToGet = type.FullName.LastIndexOf(".");
                            var typeName = type.FullName.Substring(indexToGet + 1);

                            if (!string.IsNullOrEmpty(typeName))
                            {
                                var entityType = new EntityType()
                                {
                                    EntityName = typeName,
                                    EntitySystemName = type.FullName,
                                    Installed = true
                                };
                                await entityTypeManager.InsertAsync(entityType);
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }
                }

                #endregion
            }
        }

        private List<Type> GetClassesOfBaseEntityType()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
           .SelectMany(assembly => assembly.GetTypes())
           .Where(type => type.IsSubclassOf(typeof(BaseEntityType))).ToList();

            return assemblies;
        }
    }
}
