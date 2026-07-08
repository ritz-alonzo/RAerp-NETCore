using Microsoft.AspNetCore.Mvc.Rendering;
using RA.Catalogs.App_Data;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.EntityTypes.Services;
using RA.WebFramework.Models.Pagination;

namespace RA.Catalogs.Services
{
    public interface ICatalogService : IEntityTypeService<Catalog, CatalogSetting, RACatalogContext>
    {
        Task<IEnumerable<Catalog>> GetCatalogListAsync(
            Guid entityTypeId,
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false);
        Task<IEnumerable<Catalog>> GetCatalogListAsync(
            List<Guid> entityTypeIds,
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false);

        Task<PagedResult<Catalog>> GetCatalogPagedResultListAsync(
            Guid entityTypeId,
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);

        Task<PagedResult<Catalog>> GetCatalogPagedResultListAsync(
            List<Guid> entityTypeIds,
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue);

        Task<List<SelectListItem>> GetCatalogTypesSelectListAsync(List<Guid> catalogEntityTypeIds = null, CatalogType type = CatalogType.Product);
        Task<List<SelectListItem>> GetCatalogsSelectListAsync(List<Guid> catalogEntityTypeIds, int? catalogType = null);
    }
}