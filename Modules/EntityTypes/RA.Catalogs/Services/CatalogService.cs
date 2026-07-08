using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RA.Catalogs.App_Data;
using RA.Catalogs.Data;
using RA.Catalogs.Domain;
using RA.Catalogs.Helpers;
using RA.Categories.Data;
using RA.Categories.Domain;
using RA.Categories.Helpers;
using RA.Core.DataCaching.CacheManagement;
using RA.Core.PluginData.EntityTypes.Catalogs;
using RA.EntityTypes.Services;
using RA.WebFramework.Extensions;
using RA.WebFramework.Models.Pagination;

namespace RA.Catalogs.Services
{
    public class CatalogService : EntityTypeService<Catalog, CatalogSetting, RACatalogContext>, ICatalogService
    {
        #region Constants
        private readonly RACatalogContext _context;
        private readonly DbSet<Catalog> _catalog;
        private readonly IEntityTypeManager _entityTypeManager;
        #endregion

        #region Ctor
        public CatalogService(RACatalogContext context, 
            ICacheManager<Catalog> cacheManager, 
            IEntityTypeManager entityTypeManager) 
            : base(context, cacheManager, entityTypeManager)
        {
            _context = context;
            _catalog = _context.Set<Catalog>();
            _entityTypeManager = entityTypeManager;
        }
        #endregion

        #region CRUD
        public async Task<IEnumerable<Catalog>> GetCatalogListAsync(
            Guid entityTypeId, 
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false)
        {
            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException("Catalog type id is null");

            var query = await GetListAsync(entityTypeId);

            if (categoryTypeIds.HasAny())
                query = query.Where(c => c.UOMId.HasValue && categoryTypeIds.Contains(c.UOMId.Value));

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.Code.ToLower().Contains(searchQuery.ToLower()) ||
                c.Name.ToLower().Contains(searchQuery.ToLower()));

            if (catalogTypeIds.HasAny())
                query = query.Where(c => catalogTypeIds.Contains(c.TypeId));

            if (catalogStatusIds.HasAny())
                query = query.Where(c => catalogStatusIds.Contains(c.StatusId));

            if (!showDeleted)
                query = query.Where(c => !c.Deleted);

            return query.ToList();
        }

        public async Task<IEnumerable<Catalog>> GetCatalogListAsync(
            List<Guid> entityTypeIds,
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false)
        {
            if (!entityTypeIds.Any())
                throw new ArgumentNullException("Catalog type id is null");

            var query = await GetListAsync(entityTypeIds);

            if (categoryTypeIds.HasAny())
                query = query.Where(c => c.UOMId.HasValue && categoryTypeIds.Contains(c.UOMId.Value));

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.Code.ToLower().Contains(searchQuery.ToLower()) ||
                c.Name.ToLower().Contains(searchQuery.ToLower()));

            if (catalogTypeIds.HasAny())
                query = query.Where(c => catalogTypeIds.Contains(c.TypeId));

            if (catalogStatusIds.HasAny())
                query = query.Where(c => catalogStatusIds.Contains(c.StatusId));

            if (!showDeleted)
                query = query.Where(c => !c.Deleted);

            return query.ToList();
        }
        #endregion

        #region Paged List
        public async Task<PagedResult<Catalog>> GetCatalogPagedResultListAsync(
            Guid entityTypeId,
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            if (entityTypeId.IsNullOrEmpty())
                throw new ArgumentNullException("Catalog type id is null");

            var query = await GetListAsync(entityTypeId);

            if (categoryTypeIds.HasAny())
                query = query.Where(c => c.UOMId.HasValue && categoryTypeIds.Contains(c.UOMId.Value));

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.Code.ToLower().Contains(searchQuery.ToLower()) ||
                c.Name.ToLower().Contains(searchQuery.ToLower()));

            if (catalogTypeIds.HasAny())
                query = query.Where(c => catalogTypeIds.Contains(c.TypeId));

            if (catalogStatusIds.HasAny())
                query = query.Where(c => catalogStatusIds.Contains(c.StatusId));

            if (!showDeleted)
                query = query.Where(c => !c.Deleted);

            return query.ToPagedResult(pageNumber, pageSize);
        }

        public async Task<PagedResult<Catalog>> GetCatalogPagedResultListAsync(
            List<Guid> entityTypeIds,
            List<Guid> categoryTypeIds = null,
            string searchQuery = null,
            List<int> catalogTypeIds = null,
            List<int> catalogStatusIds = null,
            bool showDeleted = false,
            int? pageNumber = 0,
            int? pageSize = int.MaxValue)
        {
            if (!entityTypeIds.Any())
                throw new ArgumentNullException("Catalog type id is null");

            var query = await GetListAsync(entityTypeIds);

            if (categoryTypeIds.HasAny())
                query = query.Where(c => c.UOMId.HasValue && categoryTypeIds.Contains(c.UOMId.Value));

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(c =>
                c.Code.ToLower().Contains(searchQuery.ToLower()) ||
                c.Name.ToLower().Contains(searchQuery.ToLower()));

            if (catalogTypeIds.HasAny())
                query = query.Where(c => catalogTypeIds.Contains(c.TypeId));

            if (catalogStatusIds.HasAny())
                query = query.Where(c => catalogStatusIds.Contains(c.StatusId));

            if (!showDeleted)
                query = query.Where(c => !c.Deleted);

            return query.ToPagedResult(pageNumber, pageSize);
        }
        #endregion

        #region Select List Items
        public async Task<List<SelectListItem>> GetCatalogTypesSelectListAsync(List<Guid> catalogEntityTypeIds = null, CatalogType type = CatalogType.Product)
        {
            var catalogTypeList = new List<SelectListItem>();

            if (!catalogEntityTypeIds.HasAny())
            {
                var catalogTypes = await _entityTypeManager.GetTypesBySystemNameAsync(typeof(Catalog).FullName);
                if (catalogTypes.Any())
                {
                    foreach (var catalogType in catalogTypes)
                    {
                        var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(catalogType.Id);
                        if (settings == null)
                            continue;

                        if (!settings.Enabled)
                            continue;

                        if (type == CatalogType.Product && !settings.InventoryEnabled)
                            continue;
                        else if (type == CatalogType.Service && settings.InventoryEnabled)
                            continue;

                        catalogTypeList.Add(new SelectListItem()
                        {
                            Value = catalogType.Id.ToString(),
                            Text = catalogType.EntityName
                        });
                    }
                }
            }
            else
            {
                foreach (var catalogTypeId in catalogEntityTypeIds)
                {
                    var settings = await _entityTypeManager.GetSettingDataOfEntityAsync<Catalog, CatalogSetting>(catalogTypeId);
                    if (settings == null)
                        continue;

                    if (!settings.Enabled)
                        continue;

                    catalogTypeList.Add(new SelectListItem()
                    {
                        Value = catalogTypeId.ToString(),
                        Text = _entityTypeManager.GetByIdAsync(catalogTypeId).Result.EntityName
                    });
                }
            }

            return catalogTypeList;
        }

        public async Task<List<SelectListItem>> GetCatalogsSelectListAsync(List<Guid> catalogEntityTypeIds, int? catalogType = null)
        {
            if (!catalogEntityTypeIds.HasAny())
                throw new ArgumentNullException(CatalogMessages.EmptyCatalogTypeIds);

            IEnumerable<Catalog> catalogs = null;

            if (catalogType == null)
                catalogs = await GetCatalogListAsync(catalogEntityTypeIds);
            else
                catalogs = await GetCatalogListAsync(catalogEntityTypeIds,
                                    catalogTypeIds: new List<int> { catalogType.Value });

            var catalogSelectListItems = new List<SelectListItem>
            {
                // show default
                new SelectListItem()
                {
                    Value = Guid.Empty.ToString(),
                    Text = "None"
                }
            };

            foreach (var catalog in catalogs)
            {
                catalogSelectListItems.Add(new SelectListItem()
                {
                    Value = catalog.Id.ToString(),
                    Text = catalog.Name.ToString()
                });
            }

            return catalogSelectListItems;
        }
        #endregion
    }
}
