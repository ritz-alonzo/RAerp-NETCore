using RA.Inventory.Helper;
using RAerp.App_Data;
using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Infrastructure
{
    public class PluginRegistration : IPluginNavigation
    {
        private readonly RAerpContext _erpContext;
        private readonly IAccessControl _accessControl;

        public PluginRegistration(RAerpContext erpContext,
            IAccessControl accessControl)
        {
            _accessControl = accessControl;
            _erpContext = erpContext;
        }

        public async Task<PluginNode> ManagePluginNode()
        {
            var inventoryNode = new PluginNode();
            inventoryNode.MenuTitle = "Inventory";
            inventoryNode.SystemName = InventoryConstants.InventorySystemName;
            inventoryNode.Url = "/Inventory/Configuration";
            inventoryNode.IconClass = "fas fa-sitemap";
            inventoryNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            inventoryNode.DisplayOrder = 11;
            inventoryNode.IsParentNode = true;
            return inventoryNode;
        }
    }
}
