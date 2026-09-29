using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.App_Data;
using RAerp.Models.NavigationModel;
using RAerp.PluginServiceProvider;
using RAerp.Security.AccessRightsControl;

namespace RA.Catalogs.Infrastructure
{
    public class PluginRegistration : IPluginNavigation
    {
        #region Constants
        private readonly RAerpContext _erpContext;
        private readonly EntityTypeManager _entityTypeManager;
        private readonly IAccessControl _accessControl;
        #endregion

        #region Ctor
        public PluginRegistration(RAerpContext erpContext,
            IAccessControl accessControl)
        {
            _erpContext = erpContext;
            _entityTypeManager = Activator.CreateInstance(typeof(EntityTypeManager), _erpContext) as EntityTypeManager;
            _accessControl = accessControl;
        }
        #endregion

        public async Task<PluginNode> ManagePluginNode()
        {
            var catalogNode = new PluginNode();

            var parentEntity = await _entityTypeManager.GetTypeBySystemNameAsync(typeof(Catalog).FullName);

            if (parentEntity == null)
                return catalogNode;

            if (parentEntity.ParentEntityTypeId.IsNotNullOrEmpty() || !parentEntity.Installed)
                return catalogNode;

            bool hasViewAccess = await _accessControl.HasViewAccessAsync<Catalog>();

            catalogNode.MenuTitle = "Catalogs";
            catalogNode.SystemName = parentEntity.EntitySystemName;
            catalogNode.Url = "/Catalogs/List";
            catalogNode.IconClass = "far fa-list-alt";
            catalogNode.Visible = hasViewAccess;
            //catalogNode.Visible = true;
            catalogNode.DisplayOrder = 2;
            catalogNode.IsParentNode = true;

            var childEntities = _entityTypeManager.GetChildEntitiesAsync(parentEntity.Id).Result.Where(c => c.Installed).ToList();

            if (childEntities.Any() && hasViewAccess)
            {
                var childNodes = new List<PluginNode>();

                int displayOrderCount = 1;

                foreach (var childEntity in childEntities)
                {
                    var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(childEntity.Id);

                    if (settings == null)
                        continue;
                    if (!settings.Enabled)
                        continue;

                    var childNode = new PluginNode();

                    childNode.MenuTitle = childEntity.EntityName;
                    childNode.SystemName = childEntity.EntitySystemName;
                    childNode.Url = $"/Catalogs/List/?entityTypeId={childEntity.Id}";
                    childNode.IconClass = "fas fa-ellipsis-h";
                    childNode.Visible = childEntity.Installed;
                    childNode.DisplayOrder = displayOrderCount;
                    childNode.IsParentNode = false;

                    childNodes.Add(childNode);

                    displayOrderCount++;
                }

                catalogNode.RelatedNodes = childNodes;
            }

            return catalogNode;
        }
    }
}
