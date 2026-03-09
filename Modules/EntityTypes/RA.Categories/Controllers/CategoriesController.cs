using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.Factories;
using RA.Categories.Helpers;
using RA.Categories.Services;
using RA.Core.Models.PluginModels.Categories;
using RA.EntityTypes.Helpers;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RAerp.Controllers.Admin;
using RAerp.Helpers.UserHelper;

namespace RA.Categories.Controllers
{
    public class CategoriesController : AdminController
    {
        #region Constants
        private readonly ICategoryService _categoryService;
        private readonly IEntityTypeManager _entityTypeManager;
        private readonly ICategoryModelFactory _categoryModelFactory;
        private readonly IMapper _mapper;
        private readonly IUserIdentity _userIdentity;
        #endregion

        #region Ctor
        public CategoriesController(ICategoryService categoryService,
            IEntityTypeManager entityTypeManager,
            ICategoryModelFactory categoryModelFactory,
            IMapper mapper,
            IUserIdentity userIdentity)
        {
            _categoryService = categoryService;
            _entityTypeManager = entityTypeManager;
            _categoryModelFactory = categoryModelFactory;
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

            var categoryConfigureModel = await _categoryModelFactory.PrepareCategoryConfigureModelAsync(entityTypeId, systemName);

            return View("~/Plugins/RA.Categories/Views/Configuration.cshtml", categoryConfigureModel);
        }

        [HttpPost]
        public async Task<IActionResult> Configuration(CategoryConfigureModel categoryConfigureModel)
        {
            if (categoryConfigureModel == null)
                return JsonError(CategoryMessages.ConfigurationSaveFailed);

            if (categoryConfigureModel.EntityTypeId.IsNullOrEmpty())
                return JsonError(EntityTypeMessages.EntityTypeIdNotExists);

            // will insert automatically when GetSettingDataOfEntity is used
            var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(categoryConfigureModel.EntityTypeId, categoryConfigureModel.SystemName);

            // sanity check if settings is not created in GetSettingDataOfEntity
            if (settings == null)
            {
                // insert
                await _entityTypeManager.InsertEntitySettingAsync<Category, CategorySetting>(categoryConfigureModel.EntityTypeId, categoryConfigureModel.SystemName);
            }

            settings = _mapper.Map(categoryConfigureModel, settings);

            // update
            await _entityTypeManager.UpdateSettingDataOfEntityAsync<Category, CategorySetting>(settings, categoryConfigureModel.EntityTypeId);

            return NullJsonResult();
        }

        #endregion

        #region CRUD

        public async Task<IActionResult> List(Guid entityTypeId, int page = 1)
        {
            if (entityTypeId.IsNullOrEmpty())
                return NotFound();

            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(entityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (!entityTypeSetting.Enabled)
                return NotFound();

            var model = await _categoryModelFactory.PrepareCategorySearchModelAsync(new CategorySearchModel() { SearchEntityTypeId = entityTypeId }, 10, page);

            return View("~/Plugins/RA.Categories/Views/List.cshtml", model);
        }

        [HttpGet]
        public async Task<IActionResult> CategoryListSearch(CategorySearchModel searchModel)
        {
            var model = await _categoryModelFactory.PrepareCategoryListModelAsync(searchModel);

            return PartialView("~/Plugins/RA.Categories/Views/_CategoryList.cshtml", model);
        }

        public async Task<IActionResult> Index(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _categoryService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            if (entity.EntityTypeId.IsNullOrEmpty())
                return NotFound();

            var model = await _categoryModelFactory.PrepareCategoryModelAsync(new CategoryModel(), entity, entity.EntityTypeId);

            return View("~/Plugins/RA.Categories/Views/Index.cshtml", model);
        }

        public async Task<IActionResult> Create(Guid entityTypeId)
        {
            if (entityTypeId.IsNullOrEmpty())
                return NotFound();

            var entityTypeSetting = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(entityTypeId);
            if (entityTypeSetting == null)
                return NotFound();

            if (!entityTypeSetting.Enabled)
                return NotFound();

            var model = await _categoryModelFactory.PrepareCategoryModelAsync(new CategoryModel(), null, entityTypeId);

            return View("~/Plugins/RA.Categories/Views/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<Category>(model);
                entity.CreatedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _categoryService.InsertAsync(entity);
                // sanity check
                model.Id = entity.Id;

                SuccessNotification(model, "Successfully created Category");
            }
            else
            {
                ErrorNotification(model, "Failed to create Category");
                return View("Create", new { entityTypeId = model.EntityTypeId });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CategoryModel model)
        {
            if (ModelState.IsValid)
            {
                var entity = _mapper.Map<Category>(model);
                entity.ModifiedById = (await _userIdentity.GetCurrentUserAsync(HttpContext)).Id;
                await _categoryService.UpdateAsync(entity);
                // sanity check
                model.Id = entity.Id;
                SuccessNotification(model, "Successfully updated Category");
            }
            else
            {
                ErrorNotification(model, "Failed to update Category");
                return View("Index", new { id = model.Id });
            }

            return RedirectToAction("Index", new { id = model.Id });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id.IsNullOrEmpty())
                return NotFound();

            var entity = await _categoryService.GetByIdAsync(id);
            if (entity == null)
                return NotFound();

            if (entity.EntityTypeId.IsNullOrEmpty())
                return NotFound();

            await _categoryService.DeleteAsync(entity);
            SuccessNotification(new CategoryModel() { Id = entity.Id }, "Successfully deleted Category");

            return RedirectToAction("List", new { entityTypeId = id });
        }

        #endregion
    }
}
