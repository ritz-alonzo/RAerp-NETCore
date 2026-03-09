using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Categories.App_Data;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.EntityTypes.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Categories.Services
{
    public interface ICategoryService : IEntityTypeService<Category, CategorySetting, RACategoryContext>
    {
        #region Category
        Task<IEnumerable<Category>> GetCategoryListAsync(List<Guid> categoryTypeIds);
        Task<List<SelectListItem>> GetCategoryTypesSelectListAsync(List<Guid> categoryTypeIds = null);
        Task<List<SelectListItem>> GetCategoriesSelectListAsync(List<Guid> categoryTypeIds);
        #endregion

        #region Category - Entity Mapping
        Task<List<Guid>> GetEntityIdsByCategoryIdAsync(Guid categoryId);
        Task<Category> GetCategoryByEntityIdAsync(Guid entityId);
        #endregion
    }
}
