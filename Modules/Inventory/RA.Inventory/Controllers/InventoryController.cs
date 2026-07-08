using Microsoft.AspNetCore.Mvc;
using RA.Catalogs.Services;
using RA.Core.Models.PluginModels.Inventory;
using RA.EntityTypes.Helpers;
using RA.Inventory.Data;
using RA.Inventory.Factories;
using RA.Inventory.Helper;
using RA.Inventory.Services.InventoryServices;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Factories.CoreFactories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Controllers
{
    public class InventoryController : AdminController
    {
        #region Constants
        private readonly IInventoryManager _inventoryManager;
        private readonly IInventoryModelFactory _inventoryModelFactory;
        #endregion

        #region Ctor
        public InventoryController(IInventoryManager inventoryManager, 
            IInventoryModelFactory inventoryModelFactory)
        {
            _inventoryManager = inventoryManager;
            _inventoryModelFactory = inventoryModelFactory;
        }
        #endregion

        #region Configuration

        [HttpGet]
        public async Task<IActionResult> Configuration()
        {
            var inventoryConfigureModel = await _inventoryModelFactory.PrepareInventoryConfigureModelAsync(InventoryConstants.InventorySystemName);

            return View(inventoryConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(InventoryConfigureModel inventoryConfigureModel)
        {
            if (inventoryConfigureModel == null)
            {
                ErrorNotification(inventoryConfigureModel, "Form doesn't have values");
                return RedirectToAction(nameof(Configuration));
            }

            if (string.IsNullOrEmpty(inventoryConfigureModel.SystemName))
            {
                ErrorNotification(inventoryConfigureModel, "Inventory system name not found");
                return RedirectToAction(nameof(Configuration));
            }

            // will insert automatically when GetSettingDataOfEntity is used
            var settings = await _inventoryManager.GetSettingDataAsync();

            // sanity check if settings is not created in GetSettingDataOfEntity
            if (settings == null)
            {
                // insert
                await _inventoryManager.InsertSettingAsync();
            }

            settings = new InventorySetting();
            settings.IsEnabled = inventoryConfigureModel.IsEnabled;
            settings.MappedCatalogTypeIds = inventoryConfigureModel.MappedCatalogTypeIds;

            // update
            await _inventoryManager.UpdateSettingDataAsync(settings);

            SuccessNotification(inventoryConfigureModel, "Successfully configure Inventory Settings");
            return RedirectToAction(nameof(Configuration));
        }

        #endregion
    }
}
