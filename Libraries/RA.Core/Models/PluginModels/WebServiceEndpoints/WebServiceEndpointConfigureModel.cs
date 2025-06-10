using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.WebServiceEndpoints
{
    public class WebServiceEndpointConfigureModel
    {
        public WebServiceEndpointConfigureModel()
        {
            AvailableEntityTypes = new List<SelectListItem>();
        }

        public bool Enabled { get; set; }

        public List<Guid> MappedEntityTypeIds { get; set; }

        public List<SelectListItem> AvailableEntityTypes { get; set; }
    }
}
