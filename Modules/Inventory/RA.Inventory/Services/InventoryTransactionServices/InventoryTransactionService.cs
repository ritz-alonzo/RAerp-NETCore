using Microsoft.EntityFrameworkCore;
using RA.Catalogs.Domain;
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

namespace RA.Inventory.Services.InventoryTransactionServices
{
    public class InventoryTransactionService : BaseService<InventoryTransaction, RAInventoryContext>, IInventoryTransactionService
    {
        #region Constants
        private readonly RAInventoryContext _context;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Ctor
        public InventoryTransactionService(RAInventoryContext context,
            ICacheManager<InventoryTransaction> cacheManager,
            ICatalogService catalogService)
            : base(context, cacheManager)
        {
            _context = context;
            _catalogService = catalogService;
        }
        #endregion

        #region CRUD

        public async Task<InventoryTransaction> GetByInventoryStockId(Guid stockId)
        {
            return await _context.InventoryTransaction.FirstOrDefaultAsync(c => c.InventoryStockId == stockId);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionListByReferenceId(Guid referenceId, int pageNumber = 0, int pageSize = 0)
        {
            var query = _context.InventoryTransaction.Where(c => c.ReferenceId == referenceId).OrderBy(c => c.CreatedOn);
            return await ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionListByCatalogIdAsync(Guid catalogId, int pageNumber = 0, int pageSize = 0)
        {
            var query = _context.InventoryTransaction.Where(c => c.CatalogId == catalogId).OrderBy(c => c.CreatedOn);
            return await ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionListByWarehouseIdAsync(Guid warehouseId, int pageNumber = 0, int pageSize = 0)
        {
            var query = _context.InventoryTransaction.Where(c => c.WarehouseId == warehouseId).OrderBy(c => c.CreatedOn);
            return await ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionListByCatalogIdAndWarehouseIdAsync(Guid catalogId, Guid warehouseId, int pageNumber = 0, int pageSize = 0)
        {
            var query = _context.InventoryTransaction.Where(c => c.WarehouseId == warehouseId && c.CatalogId == catalogId).OrderBy(c => c.CreatedOn);
            return await ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<InventoryTransaction>> GetTransactionListAsync(Guid? referenceId = null,
            Guid? stockId = null,
            Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<Guid> warehouseIds = null,
            string referenceNbr = null,
            List<int> transactionTypeIds = null,
            DateTime? searchCreatedOn = null,
            bool showLowStockOnly = false,
            bool sortByCatalogName = false,
            int pageNumber = 0,
            int pageSize = 0)
        {
            var query = await GetListAsync();

            if (referenceId.IsNotNullOrEmpty())
                query = query.Where(c => c.ReferenceId == referenceId);

            if (!string.IsNullOrEmpty(referenceNbr))
                query = query.Where(c => c.ReferenceNbr.ToLower().Contains(referenceNbr.ToLower()));

            if (stockId.IsNotNullOrEmpty())
                query = query.Where(c => c.InventoryStockId == stockId);

            if (catalogIds.HasAny())
                query = query.Where(c => catalogIds.Contains(c.CatalogId));

            if (warehouseIds.HasAny())
                query = query.Where(c => warehouseIds.Contains(c.WarehouseId));

            if (searchCreatedOn.HasValue)
                query = query.Where(c => c.CreatedOn.Date == searchCreatedOn.Value.ConvertToUTC().Date);

            if (catalogTypeId.IsNotNullOrEmpty())
            {
                query = from transaction in query
                        join catalog in await _catalogService.GetListAsync(catalogTypeId.Value)
                        on transaction.CatalogId equals catalog.Id
                        orderby
                            sortByCatalogName == true ? catalog.Name : catalog.Code
                        select transaction;
            }

            if (transactionTypeIds.HasAny())
                query = query.Where(c => transactionTypeIds.Contains(c.TransactionTypeId));

            query = query.OrderBy(c => c.CreatedOn);

            return query.ToList();
        }
        #endregion

        #region Paged List
        public async Task<PagedResult<InventoryTransaction>> GetTransactionPagedResultListAsync(Guid? referenceId = null,
            Guid? stockId = null,
            Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<Guid> warehouseIds = null,
            string referenceNbr = null,
            List<int> transactionTypeIds = null,
            DateTime? searchCreatedOn = null,
            bool showLowStockOnly = false,
            bool sortByCatalogName = false,
            int pageNumber = 0,
            int pageSize = int.MaxValue)
        {
            var query = await GetListAsync();

            if (referenceId.IsNotNullOrEmpty())
                query = query.Where(c => c.ReferenceId == referenceId);

            if (!string.IsNullOrEmpty(referenceNbr))
                query = query.Where(c => c.ReferenceNbr != null && c.ReferenceNbr.ToLower().Contains(referenceNbr.ToLower()));

            if (stockId.IsNotNullOrEmpty())
                query = query.Where(c => c.InventoryStockId == stockId);

            if (catalogIds.HasAny())
                query = query.Where(c => catalogIds.Contains(c.CatalogId));

            if (warehouseIds.HasAny())
                query = query.Where(c => warehouseIds.Contains(c.WarehouseId));

            if (searchCreatedOn.HasValue)
                query = query.Where(c => c.CreatedOn.Date == searchCreatedOn.Value.ConvertToUTC().Date);

            if (catalogTypeId.IsNotNullOrEmpty())
            {
                query = from transaction in query
                        join catalog in await _catalogService.GetListAsync(catalogTypeId.Value)
                        on transaction.CatalogId equals catalog.Id
                        orderby
                            sortByCatalogName == true ? catalog.Name : catalog.Code
                        select transaction;
            }

            if (transactionTypeIds.HasAny())
                query = query.Where(c => transactionTypeIds.Contains(c.TransactionTypeId));

            query = query.OrderBy(c => c.CreatedOn);

            return query.ToPagedResult(pageNumber, pageSize);
        }
        #endregion
    }
}
