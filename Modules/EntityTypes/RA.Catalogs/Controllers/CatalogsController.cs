using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.Factories;
using RA.Catalogs.Helpers;
using RA.Catalogs.Services;
using RA.Core.Helpers;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Catalogs;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.Data.Domain.Addresses;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.AddressHelper;
using RAerp.Helpers.UserHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Controllers
{
    public class CatalogsController : AdminController
    {
        #region Constants
        private readonly ICatalogService _catalogService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        #endregion

        #region Ctor
        public CatalogsController(ICatalogService catalogService, 
            IEntityTypeManager entityTypeManager, 
            ICatalogModelFactory catalogModelFactory, 
            IMapper mapper, 
            IUserIdentity userIdentity)
        {
            _catalogService = catalogService;
            _entityTypeManager = entityTypeManager;
            _catalogModelFactory = catalogModelFactory;
            _mapper = mapper;
            _userIdentity = userIdentity;
        }
        #endregion

        #region Configuration

        [HttpGet]
        public async Task<IActionResult> Configuration(Guid entityTypeId, string systemName)
        {
            if (entityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeNotExists);

            var catalogConfigureModel = await _catalogModelFactory.PrepareCatalogConfigureModelAsync(entityTypeId, systemName);

            return View("~/Plugins/RA.Catalogs/Views/Configuration.cshtml", catalogConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(CatalogConfigureModel catalogConfigureModel)
        {
            if (catalogConfigureModel == null)
                return JsonError(CatalogMessages.ConfigurationSaveFailed);

            if (catalogConfigureModel.EntityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeIdNotExists);

            // will insert automatically when GetSettingDataOfEntity is used
            var settings = _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(catalogConfigureModel.EntityTypeId, catalogConfigureModel.SystemName).Result;

            // sanity check if settings is not created in GetSettingDataOfEntity
            if (settings == null)
            {
                // insert
                await _entityTypeManager.InsertEntitySettingAsync<Catalog, CatalogSetting>(catalogConfigureModel.EntityTypeId, catalogConfigureModel.SystemName);
            }

            settings = _mapper.Map(catalogConfigureModel, settings);
            settings.MappedCategoryIds = catalogConfigureModel.MappedCategoryTypeIds;

            // update
            await _entityTypeManager.UpdateSettingDataOfEntityAsync<Catalog, CatalogSetting>(settings, catalogConfigureModel.EntityTypeId);

            return NullJsonResult();
        }

        #endregion

        #region CRUD

        public async Task<IActionResult> List(Guid entityTypeId, int page = 1)
        {
            if (entityTypeId.IsNullOrEmpty())
                return NotFound();

            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(entityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (!entityTypeSetting.Enabled)
                return NotFound();

            var model = await _catalogModelFactory.PrepareCatalogSearchModelAsync(new CatalogSearchModel() { SearchEntityTypeId = entityTypeId }, 10, page);

            return View("~/Plugins/RA.Catalogs/Views/List.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> CatalogListSearch(CatalogSearchModel searchModel)
        {
            var model = await _catalogModelFactory.PrepareCatalogListModelAsync(searchModel);

            return PartialView("~/Plugins/RA.Catalogs/Views/_CatalogList.cshtml", model);
        }

        public async Task<IActionResult> Index(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _catalogService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            if (entity.EntityTypeId.IsNullOrEmpty())
                return NotFound();

            var model = await _catalogModelFactory.PrepareCatalogModelAsync(new CatalogModel(), entity, entity.EntityTypeId);

            return View("~/Plugins/RA.Catalogs/Views/Index.cshtml", model);
        }

        public async Task<IActionResult> Create(Guid entityTypeId)
        {
            if (entityTypeId.IsNullOrEmpty())
                return NotFound();

            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(entityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (!entityTypeSetting.Enabled)
                return NotFound();

            var model = await _catalogModelFactory.PrepareCatalogModelAsync(new CatalogModel(), null, entityTypeId);

            return View("~/Plugins/RA.Catalogs/Views/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CatalogModel model)
        {
            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(model.EntityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<Catalog>(model);

                entity.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                entity.TypeId = model.TypeId;
                await _catalogService.InsertAsync(entity);
                // sanity check
                model.Id = entity.Id;

                SuccessNotification(model, "Successfully created Catalog");
            }
            else
            {
                ErrorNotification(model, "Failed to create Catalog");
                return View("Create", new { entityTypeId = model.EntityTypeId });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CatalogModel model)
        {
            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(model.EntityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<Catalog>(model);
                // for enum with int value mapping
                entity.TypeId = model.TypeId;
                entity.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _catalogService.UpdateAsync(entity);

                SuccessNotification(model, "Successfully updated Catalog");
            }
            else
            {
                ErrorNotification(model, "Failed to update Catalog");
                return View("Index", new { id = model.Id });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _catalogService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            if (entity.EntityTypeId.IsNullOrEmpty())
                return NotFound();

            await _catalogService.DeleteAsync(entity);
            SuccessNotification(new CatalogModel() { Id = entity.Id }, "Successfully deleted Catalog");

            return RedirectToAction("List", new { entityTypeId = id });
        }

        #endregion

        #region Selector

        public async Task<IActionResult> CatalogSelectorList([FromQuery] List<Guid> typeIds, [FromQuery] List<Guid> existingCatalogIds)
        {
            if (!typeIds.HasAny())
                return NotFound();

            // search model preparation
            var searchModel = await _catalogModelFactory.PrepareCatalogSelectorSearchModelAsync(new CatalogSearchModel() { SearchCatalogTypeIds = typeIds, SearchExistingCatalogIds = existingCatalogIds }, 10, 1);

            return View("~/Plugins/RA.Catalogs/Views/CatalogSelectorList.cshtml", searchModel);
        }

        public async Task<IActionResult> CatalogSelectorListSearch(CatalogSearchModel searchModel)
        {
            if (searchModel == null)
                return NotFound();

            var model = await _catalogModelFactory.PrepareCatalogSelectorListModelAsync(searchModel);

            return View("~/Plugins/RA.Catalogs/Views/_CatalogSelectorItemList.cshtml", model);
        }

        #endregion
    }
}
