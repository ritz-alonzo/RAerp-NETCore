using RA.Catalogs.Domain;
using RA.Catalogs.Services;
using RA.Core.Models.PluginModels.Inventory;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Services;
using RA.Inventory.Helper;
using RA.Inventory.Services.InventoryServices;
using RAerp.Factories.CoreFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Factories
{
    public class InventoryModelFactory : IInventoryModelFactory
    {
        #region Constants
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IInventoryManager _inventoryManager;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Ctor
        public InventoryModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IInventoryManager inventoryManager,
            ICatalogService catalogService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _inventoryManager = inventoryManager;
            _catalogService = catalogService;
        }
        #endregion

        #region Configuration

        public virtual async Task<InventoryConfigureModel> PrepareInventoryConfigureModelAsync(string systemName)
        {
            var InventoryConfigureModel = new InventoryConfigureModel();
            // settings
            var settings = await _inventoryManager.GetSettingDataAsync();
            if (settings != null)
            {
                if (settings.MappedCatalogTypeIds.Any())
                {
                    InventoryConfigureModel.MappedCatalogTypeIds = settings.MappedCatalogTypeIds;
                }

                InventoryConfigureModel.SystemName = InventoryConstants.InventorySystemName;
                InventoryConfigureModel.IsEnabled = settings.IsEnabled;
            }

            InventoryConfigureModel.AvailableCatalogTypes = await _catalogService.GetCatalogTypesSelectListAsync();

            return InventoryConfigureModel;
        }

        #endregion
    }
}
