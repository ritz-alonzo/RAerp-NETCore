using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RA.Categories.App_Data;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.Helpers;
using RA.Core.DataCaching.CacheManagement;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;

namespace RA.Categories.Services
{
    public class CategoryService : EntityTypeService<Category, CategorySetting, RACategoryContext>, ICategoryService
    {
        #region Constants
        private readonly RACategoryContext _context;
        private readonly DbSet<Category> _category;
        private readonly DbSet<CategoryEntityMapping> _categoryMapping;
        private readonly ICacheManager<Category> _cacheManager;
        private readonly IEntityTypeManager _entityTypeManager;
        #endregion

        #region Ctor
        public CategoryService(RACategoryContext context,
            ICacheManager<Category> cacheManager,
            IEntityTypeManager entityTypeManager) 
            : base(context, cacheManager, entityTypeManager)
        {
            _context = context;
            _category = _context.Set<Category>();
            _categoryMapping = _context.Set<CategoryEntityMapping>();
            _cacheManager = cacheManager;
            _entityTypeManager = entityTypeManager;
        }
        #endregion

        #region Category

        public async Task<IEnumerable<Category>> GetCategoryListAsync(List<Guid> categoryTypeIds)
        {
            if (!categoryTypeIds.HasAny())
                throw new ArgumentNullException(CategoryMessages.EmptyCategoryIds);

            var query = await GetListAsync(categoryTypeIds);

            // additional filters
            return query.ToList();
        }

        #region Select List Items
        public async Task<List<SelectListItem>> GetCategoryTypesSelectListAsync(List<Guid> categoryTypeIds = null)
        {
            var categoryTypeList = new List<SelectListItem>();

            if (!categoryTypeIds.HasAny())
            {
                var categoryTypes = await _entityTypeManager.GetTypesBySystemNameAsync(typeof(Category).FullName);
                if (categoryTypes.Any())
                {
                    foreach (var categoryType in categoryTypes)
                    {
                        var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(categoryType.Id);
                        if (settings == null)
                            continue;

                        if (!settings.Enabled)
                            continue;

                        categoryTypeList.Add(new SelectListItem()
                        {
                            Value = categoryType.Id.ToString(),
                            Text = categoryType.EntityName
                        });
                    }
                }
            }
            else
            {
                foreach (var categoryTypeId in categoryTypeIds)
                {
                    var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Category, CategorySetting>(categoryTypeId);
                    if (settings == null)
                        continue;

                    if (!settings.Enabled)
                        continue;

                    categoryTypeList.Add(new SelectListItem()
                    {
                        Value = categoryTypeId.ToString(),
                        Text = _entityTypeManager.GetByIdAsync(categoryTypeId).Result.EntityName
                    });
                }
            }

            return categoryTypeList;
        }

        public async Task<List<SelectListItem>> GetCategoriesSelectListAsync(List<Guid> categoryTypeIds)
        {
            var categoryList = new List<SelectListItem>
            {
                // show default
                new SelectListItem()
                {
                    Value = Guid.Empty.ToString(),
                    Text = "None"
                }
            };

            if (!categoryTypeIds.HasAny())
            {
                return categoryList.ToList();
            }
                
            var categories = await GetCategoryListAsync(categoryTypeIds);
            foreach (var category in categories)
            {
                categoryList.Add(new SelectListItem()
                {
                    Value = category.Id.ToString(),
                    Text = category.Name.ToString()
                });
            }

            return categoryList.ToList();
        }
        #endregion

        #endregion

        #region Category - Entity Mapping

        public async Task<List<Guid>> GetEntityIdsByCategoryIdAsync(Guid categoryId)
        {
            return await _categoryMapping.Where(c => c.CategoryId == categoryId).Select(c => c.EntityId).ToListAsync();
        }

        public async Task<Category> GetCategoryByEntityIdAsync(Guid entityId)
        {
            var query = await (from cat in _category
                                join map in _categoryMapping on cat.Id equals map.CategoryId
                                where map.EntityId == entityId
                                select cat).FirstOrDefaultAsync();

            return query;
        }

        #endregion
    }
}
