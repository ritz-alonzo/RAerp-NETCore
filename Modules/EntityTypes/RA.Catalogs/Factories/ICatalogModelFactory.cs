using RA.Catalogs.Domain;
using RA.Core.Models.PluginModels.Catalogs;

namespace RA.Catalogs.Factories
{
    public interface ICatalogModelFactory
    {
        Task<CatalogSearchModel> PrepareCatalogSearchModelAsync(CatalogSearchModel searchModel, int pageSize, int pageNumber);
        Task<CatalogListModel> PrepareCatalogListModelAsync(CatalogSearchModel searchModel);
        Task<CatalogModel> PrepareCatalogModelAsync(CatalogModel catalogModel, Catalog catalog, Guid entityTypeId);
        Task<CatalogConfigureModel> PrepareCatalogConfigureModelAsync(Guid entityTypeId, string systemName);
        Task<CatalogSearchModel> PrepareCatalogSelectorSearchModelAsync(CatalogSearchModel searchModel, int pageSize, int pageNumber);
        Task<CatalogListModel> PrepareCatalogSelectorListModelAsync(CatalogSearchModel searchModel);
    }
}