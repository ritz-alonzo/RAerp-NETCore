using RA.Categories.Domain;
using RA.Core.Models.PluginModels.Categories;

namespace RA.Categories.Factories
{
    public interface ICategoryModelFactory
    {
        Task<CategoryConfigureModel> PrepareCategoryConfigureModel(Guid entityTypeId, string systemName);
        Task<CategoryListModel> PrepareCategoryListModel(CategorySearchModel searchModel);
        Task<CategoryModel> PrepareCategoryModel(CategoryModel categoryModel, Category category, Guid entityTypeId);
        Task<CategorySearchModel> PrepareCategorySearchModel(CategorySearchModel searchModel, int pageSize, int pageNumber);
    }
}