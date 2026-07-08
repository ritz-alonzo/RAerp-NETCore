using RA.Core.PluginData.Inventory;
using RA.Data.Domain.Settings;
using RA.Inventory.Data;
using RA.Inventory.Domain;

namespace RA.Inventory.Services.InventoryServices
{
    public interface IInventoryManager
    {
        Task FulfillReservationAsync(InventoryStock stock, int quantity, Guid fulfilledById);
        Task GenerateTransactionAsync(InventoryStock stock, Guid? referenceId, string referenceNbr, int quantity, Guid transactorId, TransactionType transactionType, string desc);
        Task<InventoryStock> InitializeStockAsync(InventoryStock stock);
        Task ManualAdjustmentAsync(Guid catalogId, Guid warehouseId, int quantity, Guid adjustedById);
        Task ReceiveStockAsync(Guid referenceId, string referenceNbr, Guid catalogId, Guid warehouseId, int quantity, Guid receivedById, string desc, TransactionType transactionType);
        Task ReleaseStockAsync(Guid referenceId, string referenceNbr, Guid catalogId, Guid warehouseId, int quantity, Guid releasedById, string desc, TransactionType transactionType);
        Task<InventoryReservation> ReserveStockAsync(Guid referenceId, string referenceNbr, Guid catalogId, Guid warehouseId, int quantity, Guid reservedById);

        #region Settings
        Task<Setting> GetSettingByIdAsync(Guid id);
        Task<Setting> GetSettingBySystemNameAsync(string systemName);
        Task InsertSettingAsync();
        Task UpdateSettingDataAsync(InventorySetting settings);
        Task<InventorySetting> GetSettingDataAsync();
        #endregion
    }
}