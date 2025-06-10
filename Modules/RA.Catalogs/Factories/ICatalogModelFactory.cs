using RA.Core.Models.PluginModels.Catalogs;

namespace RA.Catalogs.Factories
{
    public interface ICatalogModelFactory
    {
        CatalogConfigureModel PrepareCatalogConfigureModel(Guid entityTypeId, string systemNamee);
        CatalogModel PrepareCatalogModel(CatalogModel catalogModel);
        CatalogSearchModel PrepareCatalogSearchModel(CatalogSearchModel searchModel, int pageSize, int pageNumber);
    }
}