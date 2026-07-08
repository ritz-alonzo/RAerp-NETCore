using AutoMapper;
using Microsoft.AspNetCore.Http;
using RA.Catalogs.Services;
using RA.Core.Models.PluginModels.Discounts;
using RA.Discounts.Data;
using RA.Discounts.Domain;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Services;
using RAerp.Helpers.UserHelper;
using RAerp.Security.AccessRightsControl;
using RAerp.Services.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Discounts.Factories
{
    public class DiscountModelFactory : IDiscountModelFactory
    {
        #region Constants
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IBaseEntityModelFactory _baseEntityModelFactory;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Ctor
        public DiscountModelFactory(IEntityTypeManager entityTypeManager,
            IBaseEntityModelFactory baseEntityModelFactory,
            ICatalogService catalogService)
        {
            _entityTypeManager = entityTypeManager;
            _baseEntityModelFactory = baseEntityModelFactory;
            _catalogService = catalogService;
        }
        #endregion

        #region Configuration

        public virtual async Task<DiscountConfigureModel> PrepareDiscountConfigureModelAsync(Guid entityTypeId, string systemName)
        {
            var DiscountConfigureModel = new DiscountConfigureModel();
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Discount, DiscountSetting>(entityTypeId);
            if (settings != null)
            {
                if (settings.MappedCatalogIds.Any())
                {
                    DiscountConfigureModel.MappedCatalogTypeIds = settings.MappedCatalogIds;
                }
            }
            DiscountConfigureModel = await _baseEntityModelFactory.PrepareBaseEntityConfigureModelAsync<DiscountConfigureModel, Discount, DiscountSetting>(DiscountConfigureModel, entityTypeId, systemName);
            DiscountConfigureModel.AvailableCatalogTypes = await _catalogService.GetCatalogTypesSelectListAsync();

            return DiscountConfigureModel;
        }

        #endregion
    }
}
