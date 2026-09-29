using RA.Core.Models.PluginModels.WebServiceEndpoints;
using RA.OrdersManagement.Domain.Carts;
using RA.OrdersManagement.Domain.Orders;
using RA.OrdersManagement.Domain.Payments;
using RAerp.App_Data;
using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.OrdersManagement.Infrastructure
{
    public class PluginRegistration : IPluginNavigation
    {
        private readonly RAerpContext _erpContext;
        private readonly IAccessControl _accessControl;

        public PluginRegistration(RAerpContext context,
            IAccessControl accessControl)
        {
            _erpContext = context;
            _accessControl = accessControl;
        }

        public async Task<PluginNode> ManagePluginNode()
        {
            var orderManagementNode = new PluginNode();
            orderManagementNode.MenuTitle = "Order Management";
            orderManagementNode.SystemName = "RA.OrdersManagement";
            orderManagementNode.IconClass = "fas fa-shipping-fast";
            orderManagementNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            orderManagementNode.DisplayOrder = 4;
            orderManagementNode.IsParentNode = true;

            List<PluginNode> orderManagementChildNodes = new List<PluginNode>();

            #region Cart Node
            var cartNode = new PluginNode();
            cartNode.MenuTitle = "Carts";
            cartNode.SystemName = typeof(Cart).FullName;
            cartNode.Url = "/Carts/List";
            cartNode.IconClass = "fas fa-shopping-cart";
            cartNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            cartNode.DisplayOrder = 1;
            cartNode.IsParentNode = false;

            orderManagementChildNodes.Add(cartNode);
            #endregion

            #region Order Node
            var orderNode = new PluginNode();
            orderNode.MenuTitle = "Orders";
            orderNode.SystemName = typeof(Order).FullName;
            orderNode.Url = "/Orders/List";
            orderNode.IconClass = "fas fa-cubes";
            orderNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            orderNode.DisplayOrder = 2;
            orderNode.IsParentNode = false;

            orderManagementChildNodes.Add(orderNode);
            #endregion

            #region Payment Node
            var paymentNode = new PluginNode();
            paymentNode.MenuTitle = "Payments";
            paymentNode.SystemName = typeof(Payment).FullName;
            paymentNode.Url = "/Payments/List";
            paymentNode.IconClass = "fas fa-credit-card";
            paymentNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            paymentNode.DisplayOrder = 3;
            paymentNode.IsParentNode = false;

            orderManagementChildNodes.Add(paymentNode);
            #endregion

            orderManagementNode.RelatedNodes = orderManagementChildNodes;

            return orderManagementNode;
        }
    }
}
