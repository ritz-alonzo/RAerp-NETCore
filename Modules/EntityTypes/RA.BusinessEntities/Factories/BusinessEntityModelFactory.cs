using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using RA.BusinessEntities.Data;
using RA.BusinessEntities.Domain;
using RA.BusinessEntities.Services;
using RA.Categories.Services;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.PluginData.EntityTypes.BusinessEntities;
using RA.Data.Domain.Addresses;
using RA.Data.Domain.Application;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Helpers.AddressHelper;
using RAerp.Helpers.UserHelper;
using RAerp.Services.AddressServices;
using RAerp.Services.ApplicationSettingServices;
using RAerp.Services.UserServices;

namespace RA.BusinessEntities.Factories
{
    public class BusinessEntityModelFactory : IBusinessEntityModelFactory
    {
        #region Constants
        private readonly IBaseEntityModelFactory _baseEntityModelFactory;
        private readonly IMapper _mapper;
        private readonly IBusinessEntityService _businessEntityService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IUserIdentity _userIdentity;
        private readonly ICategoryService _categoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAddressService _addressService;
        private readonly IUserService _userService;
        private readonly IApplicationSettingService _applicationSettingService;
        private readonly ApplicationSetting _applicationSetting;
        #endregion

        #region Ctor
        public BusinessEntityModelFactory(IBaseEntityModelFactory entityModelFactory,
            IMapper mapper,
            IBusinessEntityService businessEntityService,
            IEntityTypeManager entityTypeManager,
            IUserIdentity userIdentity,
            ICategoryService categoryService,
            IHttpContextAccessor httpContextAccessor,
            IAddressService addressService,
            IUserService userService)
        {
            _baseEntityModelFactory = entityModelFactory;
            _mapper = mapper;
            _businessEntityService = businessEntityService;
            _entityTypeManager = entityTypeManager;
            _userIdentity = userIdentity;
            _categoryService = categoryService;
            _httpContextAccessor = httpContextAccessor;
            _addressService = addressService;
            _userService = userService;
            _applicationSettingService = httpContextAccessor.HttpContext?.RequestServices.GetService<IApplicationSettingService>();
            _applicationSetting = _applicationSettingService?.GetCurrentApplicationSettingAsync().Result;
        }
        #endregion

        public virtual async Task<BusinessEntitySearchModel> PrepareBusinessEntitySearchModelAsync(BusinessEntitySearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (searchModel.SearchEntityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(searchModel.SearchEntityTypeId));

            var businessEntityType = await _entityTypeManager.GetByIdAsync(searchModel.SearchEntityTypeId);

            if (businessEntityType == null)
                throw new ArgumentNullException(nameof(businessEntityType));

            _baseEntityModelFactory.PrepareBaseEntitySearchModel(searchModel, businessEntityType, pageSize, pageNumber);

            searchModel.BusinessEntities = await PrepareBusinessEntityListModelAsync(searchModel);

            searchModel.TotalItems = (int)searchModel.BusinessEntities.TotalItems;
            searchModel.PageSize = searchModel.BusinessEntities.PageSize;
            searchModel.CurrentItemsShown = searchModel.BusinessEntities.Items.Count;

            return searchModel;
        }

        public virtual async Task<BusinessEntityListModel> PrepareBusinessEntityListModelAsync(BusinessEntitySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var model = new BusinessEntityListModel();

            var businessEntityList = await _businessEntityService.GetListAsync(searchModel.SearchEntityTypeId);
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(searchModel.SearchEntityTypeId);

            List<SelectListItem> availableCategories = await _categoryService.GetCategoriesSelectListAsync(settings?.MappedCategoryIds);

            var businessEntityModelList = new List<BusinessEntityModel>();
            businessEntityModelList = businessEntityList.Select(businesEntity =>
            {
                var businessEntityModel = new BusinessEntityModel();
                businessEntityModel = _mapper.Map(businesEntity, businessEntityModel);
                // will need to add check user, to set user data
                //var createdByUser = _userIdentity.GetUserDetails(businesEntity.CreatedById);
                //if (createdByUser != null)
                //    businessEntityModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);
                businessEntityModel.AvailableCategories = availableCategories;
                businessEntityModel.CreatedOn = businessEntityModel.CreatedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone);
                businessEntityModel.ModifiedOn = businessEntityModel.ModifiedOn.HasValue ? businessEntityModel.ModifiedOn.ConvertUTCToAppSettingsDateTime(_applicationSetting?.DefaultTimeZone) : null;

                return businessEntityModel;

            }).ToList();

            _baseEntityModelFactory.PrepareBaseEntityListModel(model, businessEntityModelList, searchModel, businessEntityList.Count());

            // disable for now - will enable when full cycle testing 
            //_baseEntityModelFactory.PrepareBaseEntityListModelUIAccess<BusinessEntityListModel, BusinessEntityModel, BusinessEntity, BusinessEntitySetting>(model, searchModel.SearchEntityTypeId);

            return model;
        }

        public virtual async Task<BusinessEntityModel> PrepareBusinessEntityModelAsync(BusinessEntityModel businessEntityModel, BusinessEntity businessEntity, Guid entityTypeId)
        {
            if (businessEntityModel == null)
                throw new ArgumentNullException(nameof(businessEntityModel));

            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            businessEntityModel.EntityTypeId = entityTypeId;

            var businessEntityType = await _entityTypeManager.GetByIdAsync(entityTypeId);
            if (businessEntityType == null)
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            // means for creation
            if (businessEntity == null)
            {
                businessEntity = new BusinessEntity();

                businessEntityModel.Status = BusinessEntityStatus.Active;
                businessEntityModel.CreatedOn = DateTime.UtcNow;
                businessEntityModel.Code = "NEW";
            }
            else
            {
                businessEntityModel = _mapper.Map(businessEntity, businessEntityModel);
                if (businessEntity.CreatedById.IsNotNullOrEmpty())
                    businessEntityModel.CreatedByUser.Id = businessEntity.CreatedById;
            }

            businessEntity.EntitySystemName = businessEntityType.EntitySystemName;
            if (businessEntity.CategoryId.IsNotNullOrEmpty())
            {
                businessEntityModel.CategoryId = businessEntity.CategoryId;
            }
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(entityTypeId);
            if (settings != null)
            {
                if (settings.MappedCategoryIds.HasAny())
                {
                    // Model binding of Category, will create service for this (from Categories)
                    businessEntityModel.AvailableCategories = await _categoryService.GetCategoriesSelectListAsync(settings.MappedCategoryIds);
                }

                if (settings.AddressEnabled)
                {
                    if (businessEntity.AddressId.IsNotNullOrEmpty())
                    {
                        Address address = await _addressService.GetById(businessEntity.AddressId.Value);
                        businessEntityModel.Address = AddressOverviewModelHelper.PrepareOverviewModel(address);
                        businessEntityModel.Address.Regions = await _addressService.GetRegionsSelectList();
                        businessEntityModel.Address.Cities = await _addressService.GetCitiesSelectList(address.RegionCode);
                        businessEntityModel.Address.Barangays = await _addressService.GetBarangaysSelectList(address.CityCode);
                    }
                    else
                    {
                        businessEntityModel.Address.Regions = await _addressService.GetRegionsSelectList();
                        businessEntityModel.Address.Cities = await _addressService.GetCitiesSelectList();
                        businessEntityModel.Address.Barangays = await _addressService.GetBarangaysSelectList();
                    }
                }

                if (settings.IsUserMappingEnabled)
                {
                    if (businessEntity.UserId.IsNotNullOrEmpty())
                    {
                        businessEntityModel.UserId = businessEntity.UserId.Value;
                    }

                    businessEntityModel.IsUserMappingEnabled = settings.IsUserMappingEnabled;
                    businessEntityModel.AvailableUsers = await _userService.GetAvailableUsers();
                }
            }

            // base model mapping
            businessEntityModel = await _baseEntityModelFactory.PrepareBaseEntityModelAsync<BusinessEntityModel, BusinessEntity, BusinessEntitySetting>(businessEntityModel, businessEntity, settings);

            return businessEntityModel;
        }

        #region Configuration

        public virtual async Task<BusinessEntityConfigureModel> PrepareBusinessEntityConfigureModelAsync(Guid entityTypeId, string systemName)
        {
            var businessEntityConfigureModel = new BusinessEntityConfigureModel();
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<BusinessEntity, BusinessEntitySetting>(entityTypeId);
            if (settings != null)
            {
                if (settings.MappedCategoryIds.HasAny())
                {
                    businessEntityConfigureModel.MappedCategoryTypeIds = settings.MappedCategoryIds;
                }
            }
            
            businessEntityConfigureModel = await _baseEntityModelFactory.PrepareBaseEntityConfigureModelAsync<BusinessEntityConfigureModel, BusinessEntity, BusinessEntitySetting>(businessEntityConfigureModel, entityTypeId, systemName);
            businessEntityConfigureModel.AvailableCategoryTypes = await _categoryService.GetCategoryTypesSelectListAsync();

            return businessEntityConfigureModel;
        }

        #endregion
    }
}
