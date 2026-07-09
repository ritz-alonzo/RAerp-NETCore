using Microsoft.EntityFrameworkCore;
using RA.Catalogs.Services;
using RA.Core.DataCaching.CacheManagement;
using RA.Core.Services;
using RA.Inventory.App_Data;
using RA.Inventory.Domain;
using RA.WebFramework.Extensions;
using RA.WebFramework.Models.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Services.InventoryStockServices
{
    public class InventoryStockService : BaseService<InventoryStock, RAInventoryContext>, IInventoryStockService
    {
        #region Constants
        private readonly RAInventoryContext _context;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Ctor
        public InventoryStockService(RAInventoryContext context,
            ICacheManager<InventoryStock> cacheManager,
            ICatalogService catalogService)
            : base(context, cacheManager)
        {
            _context = context;
            _catalogService = catalogService;
        }
        #endregion

        #region CRUD

        public async Task<InventoryStock> GetByCatalogIdAsync(Guid catalogId)
        {
            return await _context.InventoryStock.FirstOrDefaultAsync(c => c.CatalogId == catalogId);
        }

        public async Task<InventoryStock> GetByWarehouseIdAsync(Guid warehouseId)
        {
            return await _context.InventoryStock.FirstOrDefaultAsync(c => c.WarehouseId == warehouseId);
        }

        public async Task<InventoryStock> GetByCatalogIdAndWarehouseIdAsync(Guid catalogId, Guid warehouseId)
        {
            return await _context.InventoryStock.FirstOrDefaultAsync(c => c.CatalogId == catalogId && c.WarehouseId == warehouseId);
        }

        public async Task<IEnumerable<InventoryStock>> GetStockListAsync(Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<Guid> warehouseIds = null,
            DateTime? searchCreatedOn = null,
            bool showLowStockOnly = false,
            bool sortByCatalogName = false,
            int pageNumber = 0,
            int pageSize = 0)
        {
            var query = await GetListAsync();

            if (catalogIds.HasAny())
                query = query.Where(c => catalogIds.Contains(c.CatalogId));

            if (warehouseIds.HasAny())
                query = query.Where(c => warehouseIds.Contains(c.WarehouseId));

            if (searchCreatedOn.HasValue)
                query = query.Where(c => c.CreatedOn.Date == searchCreatedOn.Value.ConvertToUTC().Date);

            // by default included low stock
            if (showLowStockOnly)
                query = query.Where(c => c.IsLowStock);

            if (catalogTypeId.IsNotNullOrEmpty())
            {
                query = from stock in query
                        join catalog in await _catalogService.GetListAsync(catalogTypeId.Value)
                        on stock.CatalogId equals catalog.Id
                        orderby
                            sortByCatalogName == true ? catalog.Name : catalog.Code
                        select stock;
            }

            query = query.OrderBy(c => c.CreatedOn);

            return query.ToList();
        }

        public async Task<IEnumerable<InventoryStock>> GetStockListByWarehouseId(Guid warehouseId,
            Guid catalogTypeId,
            int pageNumber = 0,
            int pageSize = 0)
        {
            var query = from stock in _context.InventoryStock.Where(c => c.WarehouseId == warehouseId)
                        join catalog in await _catalogService.GetListAsync(catalogTypeId)
                        on stock.CatalogId equals catalog.Id
                        orderby catalog.Code
                        select stock;

            return await ToPagedListAsync(query, pageNumber, pageSize);
        }
        #endregion

        #region Paged List
        public async Task<PagedResult<InventoryStock>> GetStockPagedResultListAsync(Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<Guid> warehouseIds = null,
            DateTime? searchCreatedOn = null,
            bool showLowStockOnly = false,
            bool sortByCatalogName = false,
            int pageNumber = 0,
            int pageSize = int.MaxValue)
        {
            var query = await GetListAsync();

            if (catalogIds.HasAny())
                query = query.Where(c => catalogIds.Contains(c.CatalogId));

            if (warehouseIds.HasAny())
                query = query.Where(c => warehouseIds.Contains(c.WarehouseId));

            if (searchCreatedOn.HasValue)
                query = query.Where(c => c.CreatedOn.Date == searchCreatedOn.Value.ConvertToUTC().Date);

            // by default included low stock
            if (showLowStockOnly)
                query = query.Where(c => c.IsLowStock);

            if (catalogTypeId.IsNotNullOrEmpty())
            {
                query = from stock in query
                        join catalog in await _catalogService.GetListAsync(catalogTypeId.Value)
                        on stock.CatalogId equals catalog.Id
                        orderby
                            sortByCatalogName == true ? catalog.Name : catalog.Code
                        select stock;
            }

            query = query.OrderBy(c => c.CreatedOn);

            return query.ToPagedResult(pageNumber, pageSize);
        }

        public async Task<PagedResult<InventoryStock>> GetStockPagedResultListByWarehouseId(Guid warehouseId,
            Guid catalogTypeId,
            int pageNumber = 0,
            int pageSize = int.MaxValue)
        {
            var query = from stock in _context.InventoryStock.Where(c => c.WarehouseId == warehouseId)
                        join catalog in await _catalogService.GetListAsync(catalogTypeId)
                        on stock.CatalogId equals catalog.Id
                        orderby catalog.Code
                        select stock;

            return query.ToPagedResult(pageNumber, pageSize);
        }
        #endregion
    }
}
