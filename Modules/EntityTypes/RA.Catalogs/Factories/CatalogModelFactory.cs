using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.Services;
using RA.Categories.Services;
using RA.Core.Helpers;
using RA.Core.Models.PluginModels.BusinessEntities;
using RA.Core.Models.PluginModels.Catalogs;
using RA.Core.PluginData.EntityTypes.BusinessEntities;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.Data.Domain.Addresses;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Helpers.AddressHelper;
using RAerp.Helpers.UserHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Catalogs.Factories
{
    public class CatalogModelFactory : ICatalogModelFactory
    {
        #region Constants
        private readonly IBaseEntityModelFactory _baseEntityModelFactory;
        private readonly IMapper _mapper;
        private readonly ICatalogService _catalogService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IUserIdentity _userIdentity;
        private readonly ICategoryService _categoryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public CatalogModelFactory(IBaseEntityModelFactory baseEntityModelFactory,
            IMapper mapper,
            ICatalogService catalogService,
            IEntityTypeManager entityTypeManager,
            IUserIdentity userIdentity,
            ICategoryService categoryService,
            IHttpContextAccessor httpContextAccessor)
        {
            _baseEntityModelFactory = baseEntityModelFactory;
            _mapper = mapper;
            _catalogService = catalogService;
            _entityTypeManager = entityTypeManager;
            _userIdentity = userIdentity;
            _categoryService = categoryService;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public virtual async Task<CatalogSearchModel> PrepareCatalogSearchModelAsync(CatalogSearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (searchModel.SearchEntityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(searchModel.SearchEntityTypeId));

            var catalogEntityType = await _entityTypeManager.GetByIdAsync(searchModel.SearchEntityTypeId);

            if (catalogEntityType == null)
                throw new ArgumentNullException(nameof(catalogEntityType));

            _baseEntityModelFactory.PrepareBaseEntitySearchModel(searchModel, catalogEntityType, pageSize, pageNumber);
            searchModel.AvailableCatalogStatus = PluginDataHelper.EnumToSelectListItems<CatalogStatus>(showDefaultNoneValue: true);

            searchModel.Catalogs = await PrepareCatalogListModelAsync(searchModel);
            searchModel.TotalItems = (int)searchModel.Catalogs.TotalItems;
            searchModel.PageSize = searchModel.Catalogs.PageSize;
            searchModel.CurrentItemsShown = searchModel.Catalogs.Items.Count;

            return searchModel;
        }

        public virtual async Task<CatalogListModel> PrepareCatalogListModelAsync(CatalogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var model = new CatalogListModel();

            var catalogList = await _catalogService.GetCatalogPagedResultListAsync(
                searchModel.SearchEntityTypeId,
                searchQuery: searchModel.SearchQuery,
                catalogStatusIds: searchModel.SearchStatusId > 0 ? new List<int> { searchModel.SearchStatusId } : null,
                pageNumber: searchModel.PageNumber,
                pageSize: searchModel.PageSize
                );
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(searchModel.SearchEntityTypeId);

            var catalogModelList = new List<CatalogModel>();

            catalogModelList = catalogList.Items.Select(catalog =>
            {
                var catalogModel = new CatalogModel();
                catalogModel = _mapper.Map(catalog, catalogModel);

                var createdByUser = _userIdentity.GetUserDetailsAsync(catalog.CreatedById).Result;
                if (createdByUser != null)
                    catalogModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);

                if (settings != null)
                {
                    if (settings.MappedCategoryIds.Any())
                        catalogModel.AvailableCategories = _categoryService.GetCategoriesSelectListAsync(settings.MappedCategoryIds).Result;
                }

                return catalogModel;

            }).ToList();

            _baseEntityModelFactory.PrepareBaseEntityListModel(model, catalogModelList, searchModel, catalogList.TotalCount);

            // disable for now - will enable when full cycle testing 
            //_baseEntityModelFactory.PrepareBaseEntityListModelUIAccess<CatalogListModel, CatalogModel, Catalog, CatalogSetting>(model, searchModel.SearchEntityTypeId);

            return model;
        }

        public virtual async Task<CatalogModel> PrepareCatalogModelAsync(CatalogModel catalogModel, Catalog catalog, Guid entityTypeId)
        {
            if (catalogModel == null)
                throw new ArgumentNullException(nameof(catalogModel));

            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            catalogModel.EntityTypeId = entityTypeId;

            var catalogEntityType = await _entityTypeManager.GetByIdAsync(entityTypeId);
            if (catalogEntityType == null)
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            // means for creation
            if (catalog == null)
            {
                catalog = new Catalog();

                catalogModel.Status = CatalogStatus.Active;
                catalogModel.CreatedOn = DateTime.UtcNow;
                catalogModel.Code = "NEW";
            }
            else
            {
                catalogModel = _mapper.Map(catalog, catalogModel);
                if (catalog.CreatedById.IsNotNullOrEmpty())
                    catalogModel.CreatedByUser.Id = catalog.CreatedById;
            }

            catalog.EntitySystemName = catalogEntityType.EntitySystemName;
            if (catalog.UOMId.IsNotNullOrEmpty())
            {
                catalogModel.UOMId = catalog.UOMId;
            }
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(entityTypeId);
            if (settings != null)
            {
                if (settings.MappedCategoryIds.HasAny())
                {
                    // Model binding of Category, will create service for this (from Categories)
                    catalogModel.AvailableCategories = await _categoryService.GetCategoriesSelectListAsync(settings.MappedCategoryIds);
                }

                if (settings.InventoryEnabled)
                {
                    catalogModel.AvailableCatalogTypes = PluginDataHelper.EnumToSelectListItems<CatalogType>(new List<int> { (int)CatalogType.Product });
                }
                else
                {
                    catalogModel.AvailableCatalogTypes = PluginDataHelper.EnumToSelectListItems<CatalogType>(new List<int> { (int)CatalogType.Service });
                }

                if (settings.IsImageEnabled)
                {
                    catalogModel.ImagePath = catalog.ImagePath;
                    catalogModel.ImageUrl = catalog.ImagePath;
                }
            }

            if (catalogModel.CreatedOn != DateTime.MinValue)
                catalogModel.CreatedOn = catalogModel.CreatedOn.ConvertUTCToLocalDateTime();

            if (catalogModel.ModifiedOn.HasValue)
                catalogModel.ModifiedOn = catalogModel.ModifiedOn.ConvertUTCToLocalDateTime();
            // base model mapping
            catalogModel = await _baseEntityModelFactory.PrepareBaseEntityModelAsync<CatalogModel, Catalog, CatalogSetting>(catalogModel, catalog, settings);

            return catalogModel;
        }

        #region Configuration

        public virtual async Task<CatalogConfigureModel> PrepareCatalogConfigureModelAsync(Guid entityTypeId, string systemName)
        {
            var catalogConfigureModel = new CatalogConfigureModel();
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(entityTypeId);
            if (settings != null)
            {
                if (settings.MappedCategoryIds.Any())
                {
                    catalogConfigureModel.MappedCategoryTypeIds = settings.MappedCategoryIds;
                }
            }
            catalogConfigureModel = await _baseEntityModelFactory.PrepareBaseEntityConfigureModelAsync<CatalogConfigureModel, Catalog, CatalogSetting>(catalogConfigureModel, entityTypeId, systemName);
            catalogConfigureModel.AvailableCategoryTypes = await _categoryService.GetCategoryTypesSelectListAsync();

            return catalogConfigureModel;
        }

        #endregion

        #region Selector

        public virtual async Task<CatalogSearchModel> PrepareCatalogSelectorSearchModelAsync(CatalogSearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            searchModel.PageSize = pageSize;
            searchModel.PageNumber = pageNumber;
            searchModel.AvailableCatalogStatus = PluginDataHelper.EnumToSelectListItems<CatalogStatus>(showDefaultNoneValue: true);

            searchModel.Catalogs = await PrepareCatalogSelectorListModelAsync(searchModel);
            searchModel.TotalItems = (int)searchModel.Catalogs.TotalItems;
            searchModel.PageSize = searchModel.Catalogs.PageSize;
            searchModel.CurrentItemsShown = searchModel.Catalogs.Items.Count;

            return searchModel;
        }

        public virtual async Task<CatalogListModel> PrepareCatalogSelectorListModelAsync(CatalogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var model = new CatalogListModel();

            var catalogList = await _catalogService.GetCatalogPagedResultListAsync(
                searchModel.SearchCatalogTypeIds,
                searchQuery: searchModel.SearchQuery,
                catalogStatusIds: searchModel.SearchStatusId > 0 ? new List<int> { searchModel.SearchStatusId } : null
                );

            if (searchModel.SearchExistingCatalogIds.HasAny())
            {
                catalogList.Items = catalogList.Items.Where(c => !searchModel.SearchExistingCatalogIds.Contains(c.Id)).ToList();
            }

            var catalogModelList = new List<CatalogModel>();

            catalogModelList = catalogList.Items.Select(catalog =>
            {
                var catalogModel = new CatalogModel();
                catalogModel = _mapper.Map(catalog, catalogModel);

                var createdByUser = _userIdentity.GetUserDetailsAsync(catalog.CreatedById).Result;
                if (createdByUser != null)
                    catalogModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);

                // settings
                var settings = _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(catalog.EntityTypeId).Result;
                if (settings != null)
                {
                    if (settings.MappedCategoryIds.Any())
                        catalogModel.AvailableCategories = _categoryService.GetCategoriesSelectListAsync(settings.MappedCategoryIds).Result;
                }

                catalogModel.CreatedOn = catalogModel.CreatedOn.ConvertUTCToLocalDateTime();
                catalogModel.ModifiedOn = catalogModel.ModifiedOn.HasValue ? catalogModel.ModifiedOn.ConvertUTCToLocalDateTime() : null;

                return catalogModel;

            }).ToList();

            _baseEntityModelFactory.PrepareBaseEntityListModel(model, catalogModelList, searchModel, catalogList.TotalCount);

            return model;

            #endregion
        }
    }
}
