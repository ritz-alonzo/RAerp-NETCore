using RA.Catalogs.App_Data;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.EntityTypes.Services;

namespace RA.Catalogs.Services
{
    public interface ICatalogService : IEntityTypeService<Catalog, CatalogSetting, RACatalogContext>
    {
        void Insert();

        void Test();
    }
}