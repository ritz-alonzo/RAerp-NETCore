using FluentMigrator;

namespace RA.Inventory.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    [Migration(20260702101300, "Inventory: create table")]
    public class _20260702101300_CreateInventoryTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(InventoryStock))
                .WithColumn(nameof(InventoryStock.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(InventoryStock.CatalogId)).AsGuid()
                .WithColumn(nameof(InventoryStock.WarehouseId)).AsGuid()
                .WithColumn(nameof(InventoryStock.QuantityOnHand)).AsInt32()
                .WithColumn(nameof(InventoryStock.QuantityReserved)).AsInt32()
                .WithColumn(nameof(InventoryStock.QuantityAvailable)).AsInt32()
                .WithColumn(nameof(InventoryStock.LowStockThreshold)).AsInt32()
                .WithColumn(nameof(InventoryStock.IsLowStock)).AsBoolean()
                // System Fields
                .WithColumn(nameof(InventoryStock.CreatedById)).AsGuid()
                .WithColumn(nameof(InventoryStock.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(InventoryStock.CreatedOn)).AsDateTime()
                .WithColumn(nameof(InventoryStock.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(InventoryStock.IsDeleted)).AsBoolean();

            Create.Table(nameof(InventoryTransaction))
                .WithColumn(nameof(InventoryTransaction.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(InventoryTransaction.InventoryStockId)).AsGuid()
                .WithColumn(nameof(InventoryTransaction.CatalogId)).AsGuid()
                .WithColumn(nameof(InventoryTransaction.WarehouseId)).AsGuid()
                .WithColumn(nameof(InventoryTransaction.TransactionTypeId)).AsInt32()
                .WithColumn(nameof(InventoryTransaction.Quantity)).AsInt32()
                .WithColumn(nameof(InventoryTransaction.StockAfter)).AsInt32()
                .WithColumn(nameof(InventoryTransaction.ReferenceId)).AsGuid().Nullable()
                .WithColumn(nameof(InventoryTransaction.ReferenceNbr)).AsString().Nullable()
                .WithColumn(nameof(InventoryTransaction.Note)).AsString().Nullable()
                // System Fields
                .WithColumn(nameof(InventoryTransaction.CreatedById)).AsGuid()
                .WithColumn(nameof(InventoryTransaction.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(InventoryTransaction.CreatedOn)).AsDateTime()
                .WithColumn(nameof(InventoryTransaction.ModifiedOn)).AsDateTime2().Nullable();

            Create.Table(nameof(InventoryReservation))
                .WithColumn(nameof(InventoryReservation.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(InventoryReservation.InventoryStockId)).AsGuid()
                .WithColumn(nameof(InventoryReservation.CatalogId)).AsGuid()
                .WithColumn(nameof(InventoryReservation.ReferenceId)).AsGuid()
                .WithColumn(nameof(InventoryTransaction.ReferenceNbr)).AsString().Nullable()
                .WithColumn(nameof(InventoryReservation.WarehouseId)).AsGuid()
                .WithColumn(nameof(InventoryReservation.Quantity)).AsInt32()
                .WithColumn(nameof(InventoryReservation.StatusId)).AsInt32()
                .WithColumn(nameof(InventoryReservation.ReservedOn)).AsDateTime()
                .WithColumn(nameof(InventoryReservation.ReleasedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(InventoryReservation.ExpiresOn)).AsDateTime()
                // System Fields
                .WithColumn(nameof(InventoryReservation.CreatedById)).AsGuid()
                .WithColumn(nameof(InventoryReservation.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(InventoryReservation.ModifiedOn)).AsDateTime2().Nullable();
        }
        public override void Down()
        {
            Delete.Table(nameof(InventoryStock));
            Delete.Table(nameof(InventoryTransaction));
            Delete.Table(nameof(InventoryReservation));
        }
    }
}
