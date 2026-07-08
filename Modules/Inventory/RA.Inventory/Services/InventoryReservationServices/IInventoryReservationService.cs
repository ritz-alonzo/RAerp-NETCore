using RA.Core.Services;
using RA.Inventory.Domain;
using RA.WebFramework.Models.Pagination;

namespace RA.Inventory.Services.InventoryReservationServices
{
    public interface IInventoryReservationService : IBaseService<InventoryReservation>
    {
        Task<InventoryReservation> GetByInventoryStockId(Guid stockId);
        Task<IEnumerable<InventoryReservation>> GetReservationListAsync(Guid? referenceId = null, Guid? catalogTypeId = null, List<Guid> catalogIds = null, List<int> statusIds = null,
            string referenceNbr = null, DateTime? searchReserveOn = null, bool showLowStockOnly = false, bool sortByCatalogName = false, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryReservation>> GetReservationListByCatalogIdAsync(Guid catalogId, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryReservation>> GetReservationListByReferenceId(Guid referenceId, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryReservation>> GetReservationListByWarehouseIdAsync(Guid warehouseId, int pageNumber = 0, int pageSize = 0);
        Task<PagedResult<InventoryReservation>> GetReservationPagedResultListAsync(Guid? referenceId = null,
            Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<int> statusIds = null,
            string referenceNbr = null,
            DateTime? searchReserveOn = null,
            bool showLowStockOnly = false,
            bool sortByCatalogName = false,
            int pageNumber = 0,
            int pageSize = int.MaxValue);
    }
}