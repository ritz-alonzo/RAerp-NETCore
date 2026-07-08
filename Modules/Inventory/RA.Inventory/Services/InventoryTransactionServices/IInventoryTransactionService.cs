using RA.Core.Services;
using RA.Inventory.Domain;
using RA.WebFramework.Models.Pagination;

namespace RA.Inventory.Services.InventoryTransactionServices
{
    public interface IInventoryTransactionService : IBaseService<InventoryTransaction>
    {
        Task<InventoryTransaction> GetByInventoryStockId(Guid stockId);
        Task<IEnumerable<InventoryTransaction>> GetTransactionListAsync(Guid? referenceId = null, Guid? stockId = null, Guid? catalogTypeId = null, List<Guid> catalogIds = null,
            List<Guid> warehouseIds = null, string referenceNbr = null, List<int> transactionTypeIds = null, DateTime? searchCreatedOn = null, bool showLowStockOnly = false, bool sortByCatalogName = false, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryTransaction>> GetTransactionListByCatalogIdAndWarehouseIdAsync(Guid catalogId, Guid warehouseId, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryTransaction>> GetTransactionListByCatalogIdAsync(Guid catalogId, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryTransaction>> GetTransactionListByReferenceId(Guid referenceId, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryTransaction>> GetTransactionListByWarehouseIdAsync(Guid warehouseId, int pageNumber = 0, int pageSize = 0);
        Task<PagedResult<InventoryTransaction>> GetTransactionPagedResultListAsync(Guid? referenceId = null,
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
            int pageSize = int.MaxValue);
    }
}