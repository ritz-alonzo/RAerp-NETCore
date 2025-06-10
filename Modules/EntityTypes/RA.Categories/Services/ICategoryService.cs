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
        Task<IEnumerable<Category>> GetCategoryList(List<Guid> categoryTypeIds = null);
        Task<List<SelectListItem>> GetCategoryTypesSelectList(List<Guid> categoryIds = null);
        Task<List<SelectListItem>> GetCategoriesSelectList(List<Guid> categoryTypeIds = null);
        #endregion

        #region Category - Entity Mapping
        Task<List<Guid>> GetEntityIdsByCategoryId(Guid categoryId);
        Task<Category> GetCategoryByEntityId(Guid entityId);
        #endregion
    }
}
