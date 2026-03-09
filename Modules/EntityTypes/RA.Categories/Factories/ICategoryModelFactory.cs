using RA.Categories.Domain;
using RA.Core.Models.PluginModels.Categories;

namespace RA.Categories.Factories
{
    public interface ICategoryModelFactory
    {
        Task<CategoryConfigureModel> PrepareCategoryConfigureModelAsync(Guid entityTypeId, string systemName);
        Task<CategoryListModel> PrepareCategoryListModelAsync(CategorySearchModel searchModel);
        Task<CategoryModel> PrepareCategoryModelAsync(CategoryModel categoryModel, Category category, Guid entityTypeId);
        Task<CategorySearchModel> PrepareCategorySearchModelAsync(CategorySearchModel searchModel, int pageSize, int pageNumber);
    }
}