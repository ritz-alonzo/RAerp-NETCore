using Microsoft.EntityFrameworkCore;
using RA.Catalogs.Services;
using RA.Core.DataCaching.CacheManagement;
using RA.Core.PluginData.Inventory;
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

namespace RA.Inventory.Services.InventoryReservationServices
{
    public class InventoryReservationService : BaseService<InventoryReservation, RAInventoryContext>, IInventoryReservationService
    {
        #region Constants
        private readonly RAInventoryContext _context;
        private readonly ICatalogService _catalogService;
        #endregion

        #region Ctor
        public InventoryReservationService(RAInventoryContext context,
            ICacheManager<InventoryReservation> cacheManager,
            ICatalogService catalogService)
            : base(context, cacheManager)
        {
            _context = context;
            _catalogService = catalogService;
        }
        #endregion

        #region CRUD

        public async Task<InventoryReservation> GetByInventoryStockId(Guid stockId)
        {
            return await _context.InventoryReservation.FirstOrDefaultAsync(c => c.InventoryStockId == stockId);
        }

        public async Task<IEnumerable<InventoryReservation>> GetReservationListByReferenceId(Guid referenceId, int pageNumber = 0, int pageSize = 0)
        {
            var query = _context.InventoryReservation.Where(c => c.ReferenceId == referenceId).OrderBy(c => c.ReservedOn);
            return await ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<InventoryReservation>> GetReservationListByCatalogIdAsync(Guid catalogId, int pageNumber = 0, int pageSize = 0)
        {
            var query = _context.InventoryReservation.Where(c => c.CatalogId == catalogId).OrderBy(c => c.ReservedOn);
            return await ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<InventoryReservation>> GetReservationListByWarehouseIdAsync(Guid warehouseId, int pageNumber = 0, int pageSize = 0)
        {
            var query = _context.InventoryReservation.Where(c => c.WarehouseId == warehouseId).OrderBy(c => c.ReservedOn);
            return await ToPagedListAsync(query, pageNumber, pageSize);
        }

        public async Task<IEnumerable<InventoryReservation>> GetReservationListAsync(Guid? referenceId = null,
            Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<int> statusIds = null,
            string referenceNbr = null,
            DateTime? searchReserveOn = null,
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

            if (catalogIds.HasAny())
                query = query.Where(c => catalogIds.Contains(c.CatalogId));

            if (searchReserveOn.HasValue)
                query = query.Where(c => c.ReservedOn.Date == searchReserveOn.Value.ConvertToUTC().Date);

            if (catalogTypeId.IsNotNullOrEmpty())
            {
                query = from reservation in query
                        join catalog in await _catalogService.GetListAsync(catalogTypeId.Value)
                        on reservation.CatalogId equals catalog.Id
                        orderby
                            sortByCatalogName == true ? catalog.Name : catalog.Code
                        select reservation;
            }

            if (statusIds.HasAny())
                query = query.Where(c => statusIds.Contains(c.StatusId));

            query = query.OrderByDescending(c => c.ReservedOn);

            return query.ToList();
        }
        #endregion

        #region Paged List
        public async Task<PagedResult<InventoryReservation>> GetReservationPagedResultListAsync(Guid? referenceId = null,
            Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<int> statusIds = null,
            string referenceNbr = null,
            DateTime? searchReserveOn = null,
            bool showLowStockOnly = false,
            bool sortByCatalogName = false,
            int pageNumber = 0,
            int pageSize = int.MaxValue)
        {
            var query = await GetListAsync();

            if (referenceId.IsNotNullOrEmpty())
                query = query.Where(c => c.ReferenceId == referenceId);

            if (!string.IsNullOrEmpty(referenceNbr))
                query = query.Where(c => c.ReferenceNbr.ToLower().Contains(referenceNbr.ToLower()));

            if (catalogIds.HasAny())
                query = query.Where(c => catalogIds.Contains(c.CatalogId));

            if (searchReserveOn.HasValue)
                query = query.Where(c => c.ReservedOn.Date == searchReserveOn.Value.ConvertToUTC().Date);

            if (catalogTypeId.IsNotNullOrEmpty())
            {
                query = from reservation in query
                        join catalog in await _catalogService.GetListAsync(catalogTypeId.Value)
                        on reservation.CatalogId equals catalog.Id
                        orderby
                            sortByCatalogName == true ? catalog.Name : catalog.Code
                        select reservation;
            }

            if (statusIds.HasAny())
                query = query.Where(c => statusIds.Contains(c.StatusId));

            query = query.OrderByDescending(c => c.ReservedOn);

            return query.ToPagedResult(pageNumber, pageSize);
        }
        #endregion
    }
}
