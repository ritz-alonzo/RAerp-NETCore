using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
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

namespace RA.BusinessEntities.Infrastructure
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
            var businessEntityNode = new PluginNode();

            var parentEntity = await _entityTypeManager.GetTypeBySystemNameAsync(typeof(BusinessEntity).FullName);

            if (parentEntity == null)
                return businessEntityNode;

            if (parentEntity.ParentEntityTypeId.IsNotNullOrEmpty() || !parentEntity.Installed)
                return businessEntityNode;

            bool hasAccess = await _accessControl.HasViewAccessAsync<BusinessEntity>();

            businessEntityNode.MenuTitle = "Business Entities";
            businessEntityNode.SystemName = parentEntity.EntitySystemName;
            businessEntityNode.Url = "/BusinessEntities/List";
            businessEntityNode.IconClass = "fas fa-industry";
            businessEntityNode.Visible = hasAccess;
            //businessEntityNode.Visible = true;
            businessEntityNode.DisplayOrder = 2;
            businessEntityNode.IsParentNode = true;

            var childEntities = _entityTypeManager.GetChildEntitiesAsync(parentEntity.Id).Result.Where(c => c.Installed).ToList();

            if (childEntities.Any() && hasAccess)
            {
                var childNodes = new List<PluginNode>();

                int displayOrderCount = 1;

                foreach(var childEntity in childEntities)
                {
                    var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(childEntity.Id);

                    if (settings == null)
                        continue;
                    if (!settings.Enabled)
                        continue;

                    var childNode = new PluginNode();

                    childNode.MenuTitle = childEntity.EntityName;
                    childNode.SystemName = childEntity.EntitySystemName;
                    childNode.Url = $"/BusinessEntities/List/?entityTypeId={childEntity.Id}";
                    childNode.IconClass = "fas fa-building";
                    childNode.Visible = childEntity.Installed;
                    childNode.DisplayOrder = displayOrderCount;
                    childNode.IsParentNode = false;

                    childNodes.Add(childNode);

                    displayOrderCount++;
                }

                businessEntityNode.RelatedNodes = childNodes;
            }

            return businessEntityNode;
        }
    }
}
