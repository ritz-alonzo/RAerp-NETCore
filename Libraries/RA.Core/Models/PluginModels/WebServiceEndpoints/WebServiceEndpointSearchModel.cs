using RA.Core.Models.BaseModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Core.Models.PluginModels.WebServiceEndpoints
{
    public class WebServiceEndpointSearchModel : BaseSearchModel
    {
        public WebServiceEndpointSearchModel()
        {
            WebServiceEndpoints = new WebServiceEndpointListModel();
        }

        public string WebServiceEndpointName { get; set; }
        public string WebServiceEndpointSystemName { get; set; }
        public string SearchQuery { get; set; }
        public DateTime? SearchCreatedOn { get; set; }
        public WebServiceEndpointListModel WebServiceEndpoints { get; set; }
    }
}
