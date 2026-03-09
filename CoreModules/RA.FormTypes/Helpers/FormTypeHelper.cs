using RA.FormTypes.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Helpers
{
    public static class FormTypeHelper
    {
        public static List<Type> GetClassesOfBaseForm()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
           .SelectMany(assembly => assembly.GetTypes())
           .Where(type => type.IsSubclassOf(typeof(BaseForm))).ToList();

            return assemblies;
        }
    }
}
