using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RA.Catalogs.Domain;
using RA.Core.PluginData.Inventory;
using RA.Inventory.Data;
using RA.Inventory.Domain;
using RA.Inventory.Helper;
using RA.Inventory.Services.InventoryReservationServices;
using RA.Inventory.Services.InventoryStockServices;
using RA.Inventory.Services.InventoryTransactionServices;
using RA.WebFramework.Extensions;
using RAerp.App_Data;
using RAerp.Domain.Settings;
using RAerp.Helpers.UserHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RA.Inventory.Services.InventoryServices
{
    public class InventoryManager : IInventoryManager
    {
        #region Constants
        private readonly IInventoryStockService _inventoryStockService;
        private readonly IInventoryTransactionService _inventoryTransactionService;
        private readonly IInventoryReservationService _inventoryReservationService;
        private readonly IUserIdentity _userIdentity;
        private readonly RAerpContext _erpContext;
        #endregion

        #region Ctor
        public InventoryManager(IInventoryStockService inventoryStockService,
            IInventoryTransactionService inventoryTransactionService,
            IInventoryReservationService inventoryReservationService,
            IUserIdentity userIdentity,
            RAerpContext erpContext)
        {
            _inventoryStockService = inventoryStockService;
            _inventoryTransactionService = inventoryTransactionService;
            _inventoryReservationService = inventoryReservationService;
            _userIdentity = userIdentity;
            _erpContext = erpContext;
        }
        #endregion

        public async Task<InventoryStock> InitializeStockAsync(InventoryStock stock)
        {
            InventoryStock existing = await _inventoryStockService.GetByCatalogIdAndWarehouseIdAsync(stock.CatalogId, stock.WarehouseId);
            if (existing is not null)
                throw new InvalidOperationException("Stock record already exists for this item/warehouse combination.");

            if (stock.QuantityAvailable <= stock.LowStockThreshold)
                stock.IsLowStock = true;

            await _inventoryStockService.InsertAsync(stock);

            await GenerateTransactionAsync(stock, null, null, stock.QuantityOnHand, stock.CreatedById, TransactionType.NewlyAdded, "Newly Added Stock");

            return stock;
        }

        // Purchases - add stock
        public async Task ReceiveStockAsync(Guid referenceId,
            string referenceNbr,
            Guid catalogId,
            Guid warehouseId,
            int quantity,
            Guid receivedById,
            string desc,
            TransactionType transactionType)
        {
            InventoryStock stock = await _inventoryStockService.GetByCatalogIdAndWarehouseIdAsync(catalogId, warehouseId)
                ?? throw new KeyNotFoundException($"Stock for catalog: {catalogId} and warehouse: {warehouseId} not found.");

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException("Cannot receive stock when Qty is 0 or negative");

            stock.QuantityOnHand += quantity;
            stock.QuantityAvailable = stock.QuantityOnHand - stock.QuantityReserved;
            stock.ModifiedById = receivedById;
            stock.ModifiedOn = DateTime.UtcNow;
            if (stock.QuantityAvailable <= stock.LowStockThreshold)
                stock.IsLowStock = true;
            else
                stock.IsLowStock = false;
            await _inventoryStockService.UpdateAsync(stock);

            await GenerateTransactionAsync(stock, referenceId, referenceNbr, quantity, receivedById, transactionType, desc);
        }

        // Sales - subtract stock
        public async Task ReleaseStockAsync(Guid referenceId,
            string referenceNbr,
            Guid catalogId,
            Guid warehouseId,
            int quantity,
            Guid releasedById,
            string desc,
            TransactionType transactionType)
        {
            InventoryStock stock = await _inventoryStockService.GetByCatalogIdAndWarehouseIdAsync(catalogId, warehouseId)
                ?? throw new KeyNotFoundException($"Stock for catalog: {catalogId} and warehouse: {warehouseId} not found.");
            if (quantity > stock.QuantityOnHand)
                throw new InvalidOperationException("Cannot deduct more than on-hand quantity.");

            // Update reservation if any
            await FulfillReservationAsync(stock, quantity, releasedById);
            stock.QuantityOnHand -= quantity;
            stock.QuantityAvailable = stock.QuantityOnHand - stock.QuantityReserved;
            stock.ModifiedById = releasedById;
            stock.ModifiedOn = DateTime.UtcNow;
            if (stock.QuantityOnHand <= stock.LowStockThreshold)
                stock.IsLowStock = true;
            else
                stock.IsLowStock = false;
            await _inventoryStockService.UpdateAsync(stock);

            await GenerateTransactionAsync(stock, referenceId, referenceNbr, quantity, releasedById, transactionType, desc);
        }

        public async Task<InventoryReservation> ReserveStockAsync(Guid referenceId,
            string referenceNbr,
            Guid catalogId,
            Guid warehouseId,
            int quantity,
            Guid reservedById)
        {
            InventoryStock stock = await _inventoryStockService.GetByCatalogIdAndWarehouseIdAsync(catalogId, warehouseId)
                ?? throw new KeyNotFoundException($"Stock for catalog: {catalogId} and warehouse: {warehouseId} not found.");

            if (quantity > stock.QuantityOnHand)
                throw new InvalidOperationException("Cannot reserve more than on-hand quantity.");
            if (quantity > stock.QuantityAvailable)
                throw new ArgumentOutOfRangeException($"Insufficient Available Stock: {stock.QuantityAvailable}");

            InventoryReservation existingReservation = await _inventoryReservationService.GetByInventoryStockId(stock.Id);
            if (existingReservation != null &&
                existingReservation?.ReferenceId == referenceId &&
                existingReservation?.Quantity == quantity &&
                existingReservation?.Status == ReservationStatus.Active)
                throw new InvalidOperationException("Stock Reservation for this transaction already exists");

            // Update to expired then create a new reservation
            if (DateTime.UtcNow > existingReservation.ExpiresOn)
            {
                existingReservation.Status = ReservationStatus.Expired;
                existingReservation.ModifiedById = reservedById;
                existingReservation.ModifiedOn = DateTime.UtcNow;
                await _inventoryReservationService.UpdateAsync(existingReservation);
            }

            // Create Reservation
            InventoryReservation reservation = new InventoryReservation();
            reservation.InventoryStockId = stock.Id;
            reservation.CatalogId = catalogId;
            reservation.ReferenceId = referenceId;
            reservation.WarehouseId = warehouseId;
            reservation.Quantity = quantity;
            reservation.Status = ReservationStatus.Active;
            reservation.CreatedById = reservedById;
            reservation.ReservedOn = DateTime.UtcNow;
            reservation.ExpiresOn = DateTime.UtcNow.AddDays(1);
            await _inventoryReservationService.InsertAsync(reservation);

            // Update Stock
            stock.QuantityReserved = quantity;
            stock.QuantityAvailable = stock.QuantityOnHand - stock.QuantityReserved;
            stock.ModifiedById = reservedById;
            stock.ModifiedOn = DateTime.UtcNow;
            await _inventoryStockService.UpdateAsync(stock);

            // Create Reservation Transaction
            await GenerateTransactionAsync(stock, referenceId, referenceNbr, quantity, reservedById, TransactionType.Reserve, "Stock Reserved");

            return reservation;
        }

        public async Task ManualAdjustmentAsync(Guid catalogId,
            Guid warehouseId,
            int quantity,
            Guid adjustedById)
        {
            InventoryStock stock = await _inventoryStockService.GetByCatalogIdAndWarehouseIdAsync(catalogId, warehouseId)
                ?? throw new KeyNotFoundException($"Stock for catalog: {catalogId} and warehouse: {warehouseId} not found.");

            if (quantity <= 0)
                throw new ArgumentOutOfRangeException("Cannot receive stock when Qty is 0 or negative");

            stock.QuantityOnHand += quantity;
            stock.QuantityAvailable = stock.QuantityOnHand - stock.QuantityReserved;
            stock.ModifiedById = adjustedById;
            stock.ModifiedOn = DateTime.UtcNow;
            if (stock.QuantityAvailable <= stock.LowStockThreshold)
                stock.IsLowStock = true;
            else
                stock.IsLowStock = false;
            await _inventoryStockService.UpdateAsync(stock);

            await GenerateTransactionAsync(stock, null, null, quantity, adjustedById, TransactionType.Adjustment, "Manual Adjustment");
        }

        public async Task FulfillReservationAsync(
            InventoryStock stock, int quantity, Guid fulfilledById)
        {
            // Load reservation, stock; deduct, release reserve, record Sold tx
            // (implementation follows same pattern above)
            InventoryReservation existingReservation = await _inventoryReservationService.GetByInventoryStockId(stock.Id);
            if (existingReservation != null)
            {
                existingReservation.ReleasedOn = DateTime.UtcNow;
                existingReservation.ModifiedOn = DateTime.UtcNow;
                existingReservation.ModifiedById = fulfilledById;
                existingReservation.Status = ReservationStatus.Fulfilled;
                await _inventoryReservationService.UpdateAsync(existingReservation);
                stock.QuantityReserved -= quantity;
            }
        }

        public async Task GenerateTransactionAsync(InventoryStock stock, Guid? referenceId, string referenceNbr, int quantity, Guid transactorId, TransactionType transactionType, string desc)
        {
            InventoryTransaction transaction = new InventoryTransaction();
            transaction.InventoryStockId = stock.Id;
            transaction.CatalogId = stock.CatalogId;
            transaction.WarehouseId = stock.WarehouseId;
            transaction.TransactionType = transactionType;
            transaction.Quantity = quantity;
            transaction.StockAfter = stock.QuantityOnHand;
            transaction.Note = desc;
            transaction.ReferenceId = referenceId.IsNotNullOrEmpty() ? referenceId : null;
            transaction.ReferenceNbr = referenceNbr;
            transaction.CreatedById = transactorId;
            transaction.CreatedOn = DateTime.UtcNow;
            await _inventoryTransactionService.InsertAsync(transaction);
        }

        //public async Task AdjustStockAsync(
        //    Guid stockId, int adjustedQty, string adjustedBy,
        //    string reason, CancellationToken  = default)
        //{
        //    var stock = await _inventoryStockService.GetByIdAsync(stockId, )
        //        ?? throw new KeyNotFoundException($"Stock {stockId} not found.");

        //    int delta = adjustedQty - stock.QuantityOnHand;
        //    if (delta > 0) stock.Receive(delta);
        //    else if (delta < 0) stock.Deduct(Math.Abs(delta));

        //    _inventoryStockService.Update(stock);

        //    var tx = InventoryTransaction.Record(
        //        stock.Id, stock.CatalogItemId, stock.WarehouseId,
        //        TransactionType.Adjusted, Math.Abs(delta), stock.QuantityOnHand,
        //        adjustedBy, note: reason);
        //    await _inventoryTransactionService.AddAsync(tx, );
        //    await _inventoryStockService.SaveChangesAsync();
        //}

        //public async Task<IReadOnlyList<InventoryStock>> GetLowStockAlertsAsync(
        //    Guid? warehouseId = null, CancellationToken  = default)
        //    => await _inventoryStockService.GetLowStockAsync(warehouseId, );

        // TransferStockAsync: deduct from source + receive on destination + 2 transactions

        #region Settings

        public async Task<Setting> GetSettingByIdAsync(Guid id)
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.Id == id);
        }

        public virtual async Task<Setting> GetSettingBySystemNameAsync(string systemName)
        {
            return await _erpContext.Setting.FirstOrDefaultAsync(c => c.SystemName.Equals(systemName));
        }

        public virtual async Task InsertSettingAsync()
        {
            var setting = new Setting()
            {
                Id = Guid.NewGuid(),
                Name = InventoryConstants.InventorySettingName,
                SystemName = InventoryConstants.InventorySystemName,
                Data = "{}",
                CreatedOn = DateTime.UtcNow,
            };
            await _erpContext.Setting.AddAsync(setting);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task UpdateSettingDataAsync(InventorySetting settings)
        {
            var settingData = JsonConvert.SerializeObject(settings);

            var setting = await GetSettingBySystemNameAsync(InventoryConstants.InventorySystemName);
            if (setting != null)
            {
                setting.Data = settingData;
                setting.ModifiedOn = DateTime.UtcNow;
            }
            else
            {
                return;
            }

            _erpContext.Setting.Update(setting);
            await _erpContext.SaveChangesAsync();
        }

        public virtual async Task<InventorySetting> GetSettingDataAsync()
        {
            Setting setting = null;

            setting = await GetSettingBySystemNameAsync(InventoryConstants.InventorySystemName);
            if (setting == null)
            {
                await InsertSettingAsync();
                setting = await GetSettingBySystemNameAsync(InventoryConstants.InventorySystemName);
            }

            InventorySetting settingsData = new InventorySetting();
            if (setting.Data != null && setting.Data != "{}")
                settingsData = JsonConvert.DeserializeObject<InventorySetting>(setting.Data);

            return settingsData;

        }

        #endregion
    }
}
