using RA.BusinessEntities.Domain;
using RA.Categories.Domain;
using RA.Core.Models.PortableViewModels;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Infrastructure
{
    public class PluginViewComponentRegister : IPluginViewComponent
    {
        public List<PluginViewComponentModel> ManagePluginViewComponent()
        {
            var componentList = new List<PluginViewComponentModel>();

            var testComponent = new PluginViewComponentModel()
            {
                SourceEntity = nameof(BusinessEntity),
                TargetEntity = nameof(Category),
                ComponentName = "BusinessEntityTest",
                TabName = "BE Test"
            };
            componentList.Add(testComponent);

            return componentList;
        }
    }
}
