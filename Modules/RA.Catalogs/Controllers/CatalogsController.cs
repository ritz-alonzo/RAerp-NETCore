using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.Factories;
using RA.Catalogs.Helpers;
using RA.Catalogs.Services;
using RA.Core.Models.PluginModels.Catalogs;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Controllers
{
    public class CatalogsController : AdminController
    {
        private readonly ICatalogService _catalogService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly ICatalogModelFactory _catalogModelFactory;
        private readonly IMapper _mapper;

        public CatalogsController(ICatalogService catalogService, IEntityTypeManager entityTypeManager, ICatalogModelFactory catalogModelFactory, IMapper mapper)
        {
            _catalogService = catalogService;
            _entityTypeManager = entityTypeManager;
            _catalogModelFactory = catalogModelFactory;
            _mapper = mapper;
        }

        public IActionResult testing()
        {
            _catalogService.Test();
            return View("~/Plugins/RA.Catalogs/Views/testing.cshtml");
        }

        [HttpGet]
        public IActionResult Configuration(Guid entityTypeId, string systemName)
        {
            if (entityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeNotExists);

            var catalogConfigureModel = _catalogModelFactory.PrepareCatalogConfigureModel(entityTypeId, systemName);

            return View("~/Plugins/RA.Catalogs/Views/Configuration.cshtml", catalogConfigureModel);
        }

        [HttpPost]
        public IActionResult Configuration(CatalogConfigureModel catalogConfigureModel)
        {
            if (catalogConfigureModel == null)
                return JsonError(CatalogMessages.ConfigurationSaveFailed);

            if (catalogConfigureModel.EntityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeIdNotExists);

            // will insert automatically when GetSettingDataOfEntity is used
            var settings = _entityTypeManager.GetSettingDataOfEntity<Catalog, CatalogSetting>(catalogConfigureModel.EntityTypeId, catalogConfigureModel.SystemName).Result;

            // sanity check if settings is not created in GetSettingDataOfEntity
            if (settings == null)
            {
                // insert
                _entityTypeManager.InsertEntitySetting<Catalog, CatalogSetting>(catalogConfigureModel.EntityTypeId, catalogConfigureModel.SystemName);
            }

            settings = _mapper.Map(catalogConfigureModel, settings);

            // update
            _entityTypeManager.UpdateSettingDataOfEntity<Catalog, CatalogSetting>(settings, catalogConfigureModel.EntityTypeId);

            return NullJsonResult();
        }

    }
}
