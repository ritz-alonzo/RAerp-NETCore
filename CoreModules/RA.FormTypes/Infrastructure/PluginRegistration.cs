using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.FormTypes.Infrastructure
{
    public class PluginRegistration : IPluginNavigation
    {
        #region Constants
        private readonly IAccessControl _accessControl;
        #endregion

        #region Ctor
        public PluginRegistration(IAccessControl accessControl)
        {
            _accessControl = accessControl;
        }
        #endregion

        public async Task<PluginNode> ManagePluginNode()
        {
            var formTypeNode = new PluginNode();
            formTypeNode.MenuTitle = "Form Types";
            formTypeNode.SystemName = "RA.FormTypes";
            formTypeNode.Url = "/FormTypes/List";
            formTypeNode.IconClass = "fab fa-wpforms";
            formTypeNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            formTypeNode.DisplayOrder = 3;
            formTypeNode.IsParentNode = true;

            return formTypeNode;
        }
    }
}
