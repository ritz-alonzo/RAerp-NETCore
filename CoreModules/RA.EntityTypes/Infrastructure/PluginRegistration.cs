using RA.Data.Domain.EntityTypes;
using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.EntityTypes.Infrastructure
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
            var entityTypeNode = new PluginNode();
            entityTypeNode.MenuTitle = "Entity Types";
            entityTypeNode.SystemName = typeof(EntityType).FullName;
            entityTypeNode.Url = "/EntityTypes/List";
            entityTypeNode.IconClass = "fa fa-th-list";
            entityTypeNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            entityTypeNode.DisplayOrder = 4;
            entityTypeNode.IsParentNode = true;

            return entityTypeNode;
        }
    }
}
