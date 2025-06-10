using RA.Core.Models.PluginModels.EntityTypes;
using RA.WebServiceEndpoints.Domain;
using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.WebServiceEndpoints.Infrastructure
{
    public class PluginRegistration : IPluginNavigation
    {
        private readonly IAccessControl _accessControl;

        public PluginRegistration(IAccessControl accessControl)
        {
            _accessControl = accessControl;
        }

        public async Task<PluginNode> ManagePluginNode()
        {
            var webServiceEndpointNode = new PluginNode();
            webServiceEndpointNode.MenuTitle = "Web Service Endpoints";
            webServiceEndpointNode.SystemName = typeof(WebServiceEndpoint).FullName;
            webServiceEndpointNode.Url = "/WebServiceEndpoints/List";
            webServiceEndpointNode.IconClass = "fa fa-th-list";
            webServiceEndpointNode.Visible = _accessControl.HasSuperAdminAccess();
            webServiceEndpointNode.DisplayOrder = 4;
            webServiceEndpointNode.IsParentNode = true;
            return webServiceEndpointNode;
        }
    }
}
