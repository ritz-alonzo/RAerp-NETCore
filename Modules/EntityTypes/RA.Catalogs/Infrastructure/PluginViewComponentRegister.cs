using RA.Catalogs.Domain;
using RA.Core.Models.PortableViewModels;
using RAerp.PluginServiceProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Infrastructure
{
    public class PluginViewComponentRegister : IPluginViewComponent
    {
        public List<PluginViewComponentModel> ManagePluginViewComponent()
        {
            var componentList = new List<PluginViewComponentModel>();

            var catalogApiComponent = new PluginViewComponentModel()
            {
                SourceEntity = nameof(Catalog),
                TargetEntity = "WebServiceEndpoint",
                ComponentName = "CatalogApi",
                TabName = "Catalog Mapping"
            };
            componentList.Add(catalogApiComponent);

            return componentList;
        }
    }
}
