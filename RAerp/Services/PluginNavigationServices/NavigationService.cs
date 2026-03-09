using RA.Data.App_Data;
using RA.Data.Domain.Users;
using RAerp.Helpers.PluginHelper;
using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RA.Data.Domain.AccessRightControl;
using RAerp.Security.AccessRightsControl;

namespace RAerp.Services.PluginNavigationServices
{
    /// <summary>
    /// This is for user defined navigation panel
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly RAerpContext _erpContext;
        private readonly IAccessControl _accessControl;

        public NavigationService(RAerpContext erpContext, IAccessControl accessControl)
        {
            _erpContext = erpContext;
            _accessControl = accessControl;
        }

        public async Task<List<PluginNode>> GetAdminNodesMenu(HttpContext httpContext)
        {
            var adminNodes = new List<PluginNode>();

            #region Access Rights
            var accessRightsNode = new PluginNode();
            accessRightsNode.MenuTitle = "Access Rights";
            accessRightsNode.SystemName = typeof(AccessRights).FullName;
            accessRightsNode.Url = "/AccessRights/Index";
            accessRightsNode.IconClass = "fa fa-lock";
            accessRightsNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            accessRightsNode.DisplayOrder = 1;
            accessRightsNode.IsParentNode = true;
            
            adminNodes.Add(accessRightsNode);
            #endregion

            #region Users node
            var userNode = new PluginNode();
            userNode.MenuTitle = "Users";
            userNode.SystemName = typeof(User).FullName;
            userNode.Url = "/Users/List";
            userNode.IconClass = "fa fa-users";
            userNode.Visible = await _accessControl.HasSuperAdminAccessAsync();
            userNode.DisplayOrder = 2;
            userNode.IsParentNode = true;

            if (userNode.Visible)
            {
                // add user role
                userNode.RelatedNodes.Add(new PluginNode
                {
                    MenuTitle = "User Roles",
                    SystemName = typeof(UserRole).FullName,
                    Url = "/Users/UserRoleList",
                    DisplayOrder = 1,
                    Visible = true,
                    IconClass = "fa fa-users"
                });
            }

            adminNodes.Add(userNode);
            #endregion

            #region Core Modules
            var pluginAssemblies = PluginAssemblyHelper.GetAllCorePluginAssemblies();

            if (pluginAssemblies.Any())
            {
                foreach (var assembly in pluginAssemblies)
                {
                    var pluginRegistration = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IPluginNavigation)))
                        .FirstOrDefault();

                    if (pluginRegistration != null)
                    {
                        var pluginInstance = (IPluginNavigation)Activator.CreateInstance(pluginRegistration, _accessControl);

                        if (pluginInstance != null)
                        {
                            var pluginNode = await pluginInstance.ManagePluginNode();
                            if (pluginNode != null)
                            {
                                adminNodes.Add(pluginNode);
                            }
                        }
                    }
                }
            }
            #endregion

            #region Main Configuration

            #endregion

            return adminNodes;
        }

        public async Task<List<PluginNode>> GetPluginNodesMenu()
        {
            var pluginNodes = new List<PluginNode>();

            var pluginAssemblies = PluginAssemblyHelper.GetAllModulesPluginAssemblies();

            if (pluginAssemblies.Any())
            {
                foreach (var assembly in pluginAssemblies)
                {
                    var pluginRegistration = assembly.GetTypes()
                        .Where(t => t.GetInterfaces().Contains(typeof(IPluginNavigation)))
                        .FirstOrDefault();

                    if (pluginRegistration != null)
                    {
                        // added _erpContext to fix error in when creating instance for that class
                        // when creating instance if there's parameter in ctor error will occur
                        var pluginInstance = (IPluginNavigation)Activator.CreateInstance(pluginRegistration, _erpContext, _accessControl);

                        if (pluginInstance != null)
                        {
                            var pluginNode = await pluginInstance.ManagePluginNode();
                            pluginNodes.Add(pluginNode);
                        }
                    }
                }
            }
            #region Category node
            //var categoryNode = new PluginNode();
            //categoryNode.MenuTitle = "Categories";
            //categoryNode.SystemName = nameof(Category);
            //categoryNode.Url = "/Categories/List";
            //categoryNode.IconClass = "fa fa-hashtag";
            //categoryNode.Visible = _entityTypeService.GetTypeBySystemName(typeof(Category).FullName) != null;
            //categoryNode.DisplayOrder = 1;
            //categoryNode.IsParentNode = true;

            //pluginNodes.Add(categoryNode);
            #endregion

            #region Product node
            //var productNode = new PluginNode();
            //productNode.MenuTitle = "Products";
            //productNode.SystemName = nameof(Product);
            //productNode.Url = "/Products/List";
            //productNode.IconClass = "fa fa-shopping-bag";
            //productNode.Visible = _entityTypeService.GetTypeBySystemName(typeof(Product).FullName) != null;
            //productNode.DisplayOrder = 1;
            //productNode.IsParentNode = true;

            //pluginNodes.Add(productNode);
            #endregion

            return pluginNodes;
        }
    }
}
