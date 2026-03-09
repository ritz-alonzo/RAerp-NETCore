using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.Services;
using RA.Core.Models.PluginModels.Categories;
using RA.Core.PluginData.EntityTypes.Categories;
using RA.EntityTypes.Factories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Helpers.UserHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Factories
{
    public class CategoryModelFactory : ICategoryModelFactory
    {
        #region Constants
        private readonly IBaseEntityModelFactory _baseEntityModelFactory;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IUserIdentity _userIdentity;
        private readonly IHttpContextAccessor _httpContextAccessor;
        #endregion

        #region Ctor
        public CategoryModelFactory(IBaseEntityModelFactory baseEntityModelFactory,
            IMapper mapper,
            ICategoryService categoryService,
            IEntityTypeManager entityTypeManager,
            IUserIdentity userIdentity,
            IHttpContextAccessor httpContextAccessor)
        {
            _baseEntityModelFactory = baseEntityModelFactory;
            _mapper = mapper;
            _categoryService = categoryService;
            _entityTypeManager = entityTypeManager;
            _userIdentity = userIdentity;
            _httpContextAccessor = httpContextAccessor;
        }
        #endregion

        public virtual async Task<CategorySearchModel> PrepareCategorySearchModelAsync(CategorySearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (searchModel.SearchEntityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(searchModel.SearchEntityTypeId));

            var categoryType = await _entityTypeManager.GetByIdAsync(searchModel.SearchEntityTypeId);

            if (categoryType == null)
                throw new ArgumentNullException(nameof(categoryType));

            searchModel.EntityTypeName = categoryType.EntityName;

            _baseEntityModelFactory.PrepareBaseEntitySearchModel(searchModel, categoryType, pageSize, pageNumber);

            searchModel.Categories = await PrepareCategoryListModelAsync(searchModel);

            searchModel.TotalItems = (int)searchModel.Categories.TotalItems;
            searchModel.PageSize = searchModel.Categories.PageSize;
            searchModel.CurrentItemsShown = searchModel.Categories.Items.Count;

            return searchModel;
        }

        public virtual async Task<CategoryListModel> PrepareCategoryListModelAsync(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var model = new CategoryListModel();

            var categoryList = await _categoryService.GetListAsync(searchModel.SearchEntityTypeId);

            var categoryModelList = new List<CategoryModel>();

            categoryModelList = categoryList.Select(businesEntity =>
            {
                var categoryModel = new CategoryModel();
                categoryModel = _mapper.Map(businesEntity, categoryModel);
                var createdByUser = _userIdentity.GetUserDetailsAsync(businesEntity.CreatedById).Result;
                if (createdByUser != null)
                    categoryModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);

                return categoryModel;

            }).ToList();

            _baseEntityModelFactory.PrepareBaseEntityListModel(model, categoryModelList, searchModel, categoryList.Count());

            return model;
        }

        public virtual async Task<CategoryModel> PrepareCategoryModelAsync(CategoryModel categoryModel, Category category, Guid entityTypeId)
        {
            if (categoryModel == null)
                throw new ArgumentNullException(nameof(categoryModel));

            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            categoryModel.EntityTypeId = entityTypeId;
            var categoryType = await _entityTypeManager.GetByIdAsync(entityTypeId);
            if (categoryType == null)
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            // means for creation
            if (category == null)
            {
                category = new Category();

                categoryModel.Status = CategoryStatus.Active;
                categoryModel.CreatedOn = DateTime.Now;
                categoryModel.Code = "NEW";
            }
            else
            {
                categoryModel = _mapper.Map(category, categoryModel);
                if (category.CreatedById.IsNotNullOrEmpty())
                    categoryModel.CreatedByUser.Id = category.CreatedById;
            }
            category.EntitySystemName = categoryType.EntitySystemName;
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(entityTypeId);

            // base model mapping
            categoryModel = await _baseEntityModelFactory.PrepareBaseEntityModelAsync<CategoryModel, Category, CategorySetting>(categoryModel, category, settings);

            return categoryModel;
        }

        public virtual async Task<CategoryConfigureModel> PrepareCategoryConfigureModelAsync(Guid entityTypeId, string systemName)
        {
            var categoryConfigureModel = new CategoryConfigureModel();

            categoryConfigureModel = await _baseEntityModelFactory.PrepareBaseEntityConfigureModelAsync<CategoryConfigureModel, Category, CategorySetting>(categoryConfigureModel, entityTypeId, systemName);

            return categoryConfigureModel;
        }
    }
}
