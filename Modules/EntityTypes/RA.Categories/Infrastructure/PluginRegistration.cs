using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Data.App_Data;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Infrastructure
{
    public class PluginRegistration : IPluginNavigation
    {
        private readonly RAerpContext _erpContext;
        private readonly EntityTypeManager _entityTypeManager;
        private readonly IAccessControl _accessControl;

        public PluginRegistration(RAerpContext erpContext, 
            IAccessControl accessControl)
        {
            _erpContext = erpContext;
            _entityTypeManager = Activator.CreateInstance(typeof(EntityTypeManager), _erpContext) as EntityTypeManager;
            _accessControl = accessControl;
        }

        public async Task<PluginNode> ManagePluginNode()
        {
            var categoryNode = new PluginNode();

            var parentEntity = await _entityTypeManager.GetTypeBySystemNameAsync(typeof(Category).FullName);

            if (parentEntity == null)
                return categoryNode;

            if (parentEntity.ParentEntityTypeId.IsNotNullOrEmpty() || !parentEntity.Installed)
                return categoryNode;

            bool hasAccess = await _accessControl.HasViewAccessAsync<Category>();

            categoryNode.MenuTitle = "Categories";
            categoryNode.SystemName = parentEntity.EntitySystemName;
            categoryNode.Url = "/Categories/List";
            categoryNode.IconClass = "fas fa-tags";
            categoryNode.Visible = hasAccess;
            //categoryNode.Visible = true;
            categoryNode.DisplayOrder = 2;
            categoryNode.IsParentNode = true;

            var childEntities = _entityTypeManager.GetChildEntitiesAsync(parentEntity.Id).Result.Where(c => c.Installed).ToList();

            if (childEntities.Any() && hasAccess)
            {
                var childNodes = new List<PluginNode>();

                int displayOrderCount = 1;

                foreach (var childEntity in childEntities)
                {
                    var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(childEntity.Id);

                    if (settings == null)
                        continue;
                    if (!settings.Enabled)
                        continue;

                    var childNode = new PluginNode();

                    childNode.MenuTitle = childEntity.EntityName;
                    childNode.SystemName = childEntity.EntitySystemName;
                    childNode.Url = $"/Categories/List/?entityTypeId={childEntity.Id}";
                    childNode.IconClass = "fas fa-tag";
                    childNode.Visible = childEntity.Installed;
                    childNode.DisplayOrder = displayOrderCount;
                    childNode.IsParentNode = false;

                    childNodes.Add(childNode);

                    displayOrderCount++;
                }

                categoryNode.RelatedNodes = childNodes;
            }

            return categoryNode;
        }
    }
}
