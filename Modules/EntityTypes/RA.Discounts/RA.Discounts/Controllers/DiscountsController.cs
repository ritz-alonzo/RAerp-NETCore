using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RA.Core.Models.PluginModels.Discounts;
using RA.Discounts.Data;
using RA.Discounts.Domain;
using RA.Discounts.Factories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Controllers
{
    public class DiscountsController : AdminController
    {
        #region Constants
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IDiscountModelFactory _discountModelFactory;
        private readonly IMapper _mapper;

        public DiscountsController(IEntityTypeManager entityTypeManager,
            IDiscountModelFactory discountModelFactory,
            IMapper mapper)
        {
            _entityTypeManager = entityTypeManager;
            _discountModelFactory = discountModelFactory;
            _mapper = mapper;
        }
        #endregion

        #region Configuration

        [HttpGet]
        public async Task<IActionResult> Configuration(Guid entityTypeId, string systemName)
        {
            if (entityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeNotExists);

            var discountConfigureModel = await _discountModelFactory.PrepareDiscountConfigureModelAsync(entityTypeId, systemName);

            return View(discountConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(DiscountConfigureModel discountConfigureModel)
        {
            if (discountConfigureModel == null)
                return JsonError("Failed to Save Configuration");

            if (discountConfigureModel.EntityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeIdNotExists);

            // will insert automatically when GetSettingDataOfEntity is used
            var settings = _entityTypeManager.GetSettingDataOfEntityAsync<Discount, DiscountSetting>(discountConfigureModel.EntityTypeId, discountConfigureModel.SystemName).Result;

            // sanity check if settings is not created in GetSettingDataOfEntity
            if (settings == null)
            {
                // insert
                await _entityTypeManager.InsertEntitySettingAsync<Discount, DiscountSetting>(discountConfigureModel.EntityTypeId, discountConfigureModel.SystemName);
            }

            settings = _mapper.Map(discountConfigureModel, settings);
            settings.MappedCatalogIds = discountConfigureModel.MappedCatalogTypeIds;

            // update
            await _entityTypeManager.UpdateSettingDataOfEntityAsync<Discount, DiscountSetting>(settings, discountConfigureModel.EntityTypeId);

            return NullJsonResult();
        }

        #endregion
    }
}
