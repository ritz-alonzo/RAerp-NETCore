using RA.Core.Services;
using RA.Inventory.Domain;
using RA.WebFramework.Models.Pagination;

namespace RA.Inventory.Services.InventoryStockServices
{
    public interface IInventoryStockService : IBaseService<InventoryStock>
    {
        Task<InventoryStock> GetByCatalogIdAndWarehouseIdAsync(Guid catalogId, Guid warehouseId);
        Task<InventoryStock> GetByCatalogIdAsync(Guid catalogId);
        Task<InventoryStock> GetByWarehouseIdAsync(Guid warehouseId);
        Task<IEnumerable<InventoryStock>> GetStockListAsync(Guid? catalogTypeId = null, List<Guid> catalogIds = null, List<Guid> warehouseIds = null, DateTime? searchCreatedOn = null, bool showLowStockOnly = false, bool sortByCatalogName = false, int pageNumber = 0, int pageSize = 0);
        Task<IEnumerable<InventoryStock>> GetStockListByWarehouseId(Guid warehouseId, Guid catalogTypeId, int pageNumber = 0, int pageSize = 0);
        Task<PagedResult<InventoryStock>> GetStockPagedResultListAsync(Guid? catalogTypeId = null,
            List<Guid> catalogIds = null,
            List<Guid> warehouseIds = null,
            DateTime? searchCreatedOn = null,
            bool showLowStockOnly = false,
            bool sortByCatalogName = false,
            int pageNumber = 0,
            int pageSize = int.MaxValue);
        Task<PagedResult<InventoryStock>> GetStockPagedResultListByWarehouseId(Guid warehouseId,
            Guid catalogTypeId,
            int pageNumber = 0,
            int pageSize = int.MaxValue);
    }
}