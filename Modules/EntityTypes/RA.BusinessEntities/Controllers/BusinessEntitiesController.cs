using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Factories;
using RA.BusinessEntities.Helpers;
using RA.BusinessEntities.Services;
using RA.BusinessEntities.Validators;
using RA.Core.Models.OverviewModels;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Categories;
using RA.Core.Models.PluginModels.EntityTypes;
using RA.Data.Domain.Addresses;
using RA.Data.Domain.EntityTypes;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.AddressHelper;
using RAerp.Helpers.UserHelper;
using RAerp.Services.AddressServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.BusinessEntities.Controllers
{
    public class BusinessEntitiesController : AdminController
    {
        #region Constants
        private readonly IBusinessEntityService _businessEntityService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IBusinessEntityModelFactory _businessEntityModelFactory;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        private readonly IAddressService _addressService;
        #endregion

        #region Ctor
        public BusinessEntitiesController(IBusinessEntityService businessEntityService,
            IEntityTypeManager entityTypeManager,
            IBusinessEntityModelFactory businessEntityModelFactory,
            IMapper mapper,
            IUserIdentity userIdentity,
            IAddressService addressService)
        {
            _businessEntityService = businessEntityService;
            _entityTypeManager = entityTypeManager;
            _businessEntityModelFactory = businessEntityModelFactory;
            _mapper = mapper;
            _userIdentity = userIdentity;
            _addressService = addressService;
        }
        #endregion

        #region Configuration

        [HttpGet]
        public async Task<IActionResult> Configuration(Guid entityTypeId, string systemName)
        {
            if (entityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeNotExists);

            var businessEntityConfigureModel = await _businessEntityModelFactory.PrepareBusinessEntityConfigureModelAsync(entityTypeId, systemName);

            return View(businessEntityConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(BusinessEntityConfigureModel businessEntityConfigureModel)
        {
            if (businessEntityConfigureModel == null)
                return JsonError(BusinessEntityMessages.ConfigurationSaveFailed);

            if (businessEntityConfigureModel.EntityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeIdNotExists);

            // will insert automatically when GetSettingDataOfEntity is used
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(businessEntityConfigureModel.EntityTypeId, businessEntityConfigureModel.SystemName);

            // sanity check if settings is not created in GetSettingDataOfEntity
            if (settings == null)
            {
                // insert
                await _entityTypeManager.InsertEntitySettingAsync<BusinessEntity, BusinessEntitySetting>(businessEntityConfigureModel.EntityTypeId, businessEntityConfigureModel.SystemName);
            }

            settings = _mapper.Map(businessEntityConfigureModel, settings);
            // needed to manually set mapper cannot map ids
            settings.MappedCategoryIds = businessEntityConfigureModel.MappedCategoryTypeIds;

            // update
            await _entityTypeManager.UpdateSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(settings, businessEntityConfigureModel.EntityTypeId);

            return NullJsonResult();
        }

        #endregion

        #region CRUD

        public async Task<IActionResult> List(Guid entityTypeId, int page = 1)
        {
            if (entityTypeId.IsNullOrEmpty())
                return NotFound();

            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(entityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (!entityTypeSetting.Enabled)
                return NotFound();

            var model = await _businessEntityModelFactory.PrepareBusinessEntitySearchModelAsync(new BusinessEntitySearchModel() { SearchEntityTypeId = entityTypeId }, 10, page);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> BusinessEntityListSearch(BusinessEntitySearchModel searchModel)
        {
            var model = await _businessEntityModelFactory.PrepareBusinessEntityListModelAsync(searchModel);

            return PartialView(model);
        }

        public async Task<IActionResult> Index(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _businessEntityService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            if (entity.EntityTypeId.IsNullOrEmpty())
                return NotFound();

            var model = await _businessEntityModelFactory.PrepareBusinessEntityModelAsync(new BusinessEntityModel(), entity, entity.EntityTypeId);

            return View(model);
        }

        public async Task<IActionResult> Create(Guid entityTypeId)
        {
            if (entityTypeId.IsNullOrEmpty())
                return NotFound();

            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(entityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (!entityTypeSetting.Enabled)
                return NotFound();

            var model = await _businessEntityModelFactory.PrepareBusinessEntityModelAsync(new BusinessEntityModel(), null, entityTypeId);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BusinessEntityModel model)
        {
            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(model.EntityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<BusinessEntity>(model);

                #region Address Create

                if (entityTypeSetting.AddressEnabled && model.Address != null && model.Address.RegionCode != null)
                {
                    Address address = AddressOverviewModelHelper.PrepareAddressEntity(model.Address);
                    if (address != null && address.Id.IsNullOrEmpty())
                    {
                        address.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                        address = await _addressService.Insert(address);
                        entity.AddressId = address.Id;
                    }
                }

                #endregion

                entity.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _businessEntityService.InsertAsync(entity);
                // sanity check
                model.Id = entity.Id;

                SuccessNotification(model, "Successfully created Business Entity");
            }
            else
            {
                ErrorNotification(model, "Failed to create Business Entity");
                return RedirectToAction("Create", new { entityTypeId = model.EntityTypeId });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(BusinessEntityModel model)
        {
            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(model.EntityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<BusinessEntity>(model);

                #region Address Update

                if (entityTypeSetting.AddressEnabled && model.Address != null && model.Address.RegionCode != null)
                {
                    Address address = AddressOverviewModelHelper.PrepareAddressEntity(model.Address);
                    if (address != null && address.Id.IsNotNullOrEmpty())
                    {
                        var originalAddress = await _addressService.GetById(address.Id);
                        originalAddress = AddressOverviewModelHelper.PrepareAddressRemapping(originalAddress, address);
                        originalAddress.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                        await _addressService.Update(originalAddress);
                    }
                    else
                    {
                        address = await _addressService.Insert(address);
                        entity.AddressId = address.Id;
                    }
                }

                #endregion

                entity.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _businessEntityService.UpdateAsync(entity);

                SuccessNotification(model, "Successfully updated Business Entity");
            }
            else
            {
                //ErrorNotification(model, $"Failed to update Business Entity: {ModelState}");
                ErrorNotification<BusinessEntityModel, BusinessEntityValidator>(model, "Failed to update Business Entity");
                return RedirectToAction("Index", model);
            }

            return RedirectToAction("Index", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _businessEntityService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            if (entity.EntityTypeId.IsNullOrEmpty())
                return NotFound();

            await _businessEntityService.DeleteAsync(entity);
            SuccessNotification(new BusinessEntityModel() { Id = entity.Id }, "Successfully deleted Business Entity");

            return RedirectToAction("List", new { entityTypeId =  id });
        }

        #endregion
    }
}
