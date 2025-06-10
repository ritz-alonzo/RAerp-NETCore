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
        private readonly IBaseEntityModelFactory _baseEntityModelFactory;
        private readonly IMapper _mapper;
        private readonly ICategoryService _categoryService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly IUserIdentity _userIdentity;
        private readonly IHttpContextAccessor _httpContextAccessor;

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

        public virtual async Task<CategorySearchModel> PrepareCategorySearchModel(CategorySearchModel searchModel, int pageSize, int pageNumber)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            if (searchModel.SearchEntityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(nameof(searchModel.SearchEntityTypeId));

            var categoryType = await _entityTypeManager.GetById(searchModel.SearchEntityTypeId);

            if (categoryType == null)
                throw new ArgumentNullException(nameof(categoryType));

            searchModel.EntityTypeName = categoryType.EntityName;

            _baseEntityModelFactory.PrepareBaseEntitySearchModel(searchModel, categoryType, pageSize, pageNumber);

            searchModel.Categories = await PrepareCategoryListModel(searchModel);

            searchModel.TotalItems = (int)searchModel.Categories.TotalItems;
            searchModel.PageSize = searchModel.Categories.PageSize;
            searchModel.CurrentItemsShown = searchModel.Categories.Items.Count;

            return searchModel;
        }

        public virtual async Task<CategoryListModel> PrepareCategoryListModel(CategorySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var model = new CategoryListModel();

            var categoryList = await _categoryService.GetList(searchModel.SearchEntityTypeId);

            var categoryModelList = new List<CategoryModel>();

            categoryModelList = categoryList.Select(businesEntity =>
            {
                var categoryModel = new CategoryModel();
                categoryModel = _mapper.Map(businesEntity, categoryModel);
                // will need to add check user, to set user data
                //var createdByUser = _userIdentity.GetUserDetails(businesEntity.CreatedById);
                //if (createdByUser != null)
                //    categoryModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(createdByUser);

                return categoryModel;

            }).ToList();

            _baseEntityModelFactory.PrepareBaseEntityListModel(model, categoryModelList, searchModel, categoryList.Count());
            // disable for now - will enable when full cycle testing 
            //_baseEntityModelFactory.PrepareBaseEntityListModelUIAccess<CategoryListModel, CategoryModel, Category, CategorySetting>(model, searchModel.SearchEntityTypeId);

            return model;
        }

        public virtual async Task<CategoryModel> PrepareCategoryModel(CategoryModel categoryModel, Category category, Guid entityTypeId)
        {
            if (categoryModel == null)
                throw new ArgumentNullException(nameof(categoryModel));

            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            categoryModel.EntityTypeId = entityTypeId;
            var categoryType = await _entityTypeManager.GetById(entityTypeId);
            if (categoryType == null)
                throw new ArgumentNullException(EntityTypeMessages.EntityTypeIdNotExists);

            // means for creation
            if (category == null)
            {
                category = new Category();

                categoryModel.Status = CategoryStatus.Active;
                categoryModel.CreatedOn = DateTime.Now;
                categoryModel.Code = "NEW";
                categoryModel.CreatedByUser = UserOverviewHelper.PrepareUserOverviewModel(_userIdentity.GetCurrentUser(_httpContextAccessor.HttpContext));
            }
            else
            {
                categoryModel = _mapper.Map(category, categoryModel);
                if (category.CreatedById.IsNotNullOrEmpty())
                    categoryModel.CreatedByUser.Id = category.CreatedById;
            }
            category.EntitySystemName = categoryType.EntitySystemName;
            // settings
            var settings = await _entityTypeManager.GetSettingDataOfEntity<Category, CategorySetting>(entityTypeId);

            // base model mapping
            categoryModel = _baseEntityModelFactory.PrepareBaseEntityModel<CategoryModel, Category, CategorySetting>(categoryModel, category, settings);

            // Model binding of Category, will create service for this (from Categories)
            //model.AvailableCategories =
            //        Enum.GetValues(typeof(CatalogCategory)).Cast<CatalogCategory>().Select(category => new SelectListItem
            //        {
            //            Text = category.ToString(),
            //            Value = ((int)category).ToString()
            //        }).ToList();

            return categoryModel;
        }

        public virtual async Task<CategoryConfigureModel> PrepareCategoryConfigureModel(Guid entityTypeId, string systemName)
        {
            var categoryConfigureModel = new CategoryConfigureModel();

            categoryConfigureModel = _baseEntityModelFactory.PrepareBaseEntityConfigureModel<CategoryConfigureModel, Category, CategorySetting>(categoryConfigureModel, entityTypeId, systemName);

            // for testing
            //categoryConfigureModel.AvailableCategoryTypes = new List<SelectListItem>()
            //{
            //    new SelectListItem()
            //    {
            //        Value = Guid.NewGuid().ToString(),
            //        Text = "Testing"
            //    },
            //    new SelectListItem()
            //    {
            //        Value = Guid.NewGuid().ToString(),
            //        Text = "World"
            //    }
            //};

            return categoryConfigureModel;
        }
    }
}
