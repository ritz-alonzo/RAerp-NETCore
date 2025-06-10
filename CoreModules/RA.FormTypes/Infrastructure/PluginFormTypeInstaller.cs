using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using RA.Data.App_Data;
using RA.Data.Domain.FormTypes;
using RA.FormTypes.Domain;
using RA.FormTypes.Services;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Infrastructure
{
    public class PluginFormTypeInstaller : IPluginInstallation
    {
        public async Task PluginTypeInstall(IApplicationBuilder app)
        {
            using (var scope = app.ApplicationServices.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<RAerpContext>();

                var formTypeManager = Activator.CreateInstance(typeof(FormTypeManager), context) as FormTypeManager;

                #region Install EntityTypes
                var types = GetClassesOfBaseForm();

                if (types.Any())
                {
                    var typesToInstall = new List<Type>();

                    var formTypes = formTypeManager.GetList().Result.Where(c => c.FormTypeClassificationName == null).Select(c => c.FormTypeSystemName).ToList();
                    if (formTypes.Any())
                    {
                        var typeList = types.ToList();
                        typesToInstall = typeList.Where(c => !formTypes.Contains(c.FullName)).ToList();
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
                            var form = await formTypeManager.GetTypeBySystemName(type.FullName);
                            if (form != null)
                                continue;
                            // just to get the name of the assembly full name
                            var indexToGet = type.FullName.LastIndexOf(".");
                            var typeName = type.FullName.Substring(indexToGet + 1);

                            if (!string.IsNullOrEmpty(typeName))
                            {
                                var formType = new FormType()
                                {
                                    FormTypeName = typeName,
                                    FormTypeSystemName = type.FullName,
                                    Installed = true
                                };
                                await formTypeManager.Insert(formType);
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

        private List<Type> GetClassesOfBaseForm()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
           .SelectMany(assembly => assembly.GetTypes())
           .Where(type => type.IsSubclassOf(typeof(BaseForm))).ToList();

            return assemblies;
        }
    }
}
