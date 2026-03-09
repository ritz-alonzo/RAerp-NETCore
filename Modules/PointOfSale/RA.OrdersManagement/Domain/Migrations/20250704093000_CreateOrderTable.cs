using FluentMigrator;
using RA.OrdersManagement.Domain.Orders;

namespace RA.OrdersManagement.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20250704093000, "Order, OrderItem: create table")]
    public class _20250704093000_CreateOrderTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Order))
                .WithColumn(nameof(Order.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(Order.FormNbr)).AsString().Nullable()
                .WithColumn(nameof(Order.ServiceId)).AsGuid().Nullable()
                .WithColumn(nameof(Order.CustomerName)).AsString().Nullable()
                .WithColumn(nameof(Order.OrderDate)).AsDateTime()
                .WithColumn(nameof(Order.Description)).AsString().Nullable()
                .WithColumn(nameof(Order.StatusId)).AsInt32()
                .WithColumn(nameof(Order.TotalQty)).AsDecimal()
                .WithColumn(nameof(Order.TotalDiscountAmount)).AsDecimal()
                .WithColumn(nameof(Order.TotalVatAmount)).AsDecimal()
                .WithColumn(nameof(Order.TotalGrossAmount)).AsDecimal()
                .WithColumn(nameof(Order.TotalNetAmount)).AsDecimal()
                // system fields
                .WithColumn(nameof(Order.CreatedById)).AsGuid()
                .WithColumn(nameof(Order.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(Order.ApprovedById)).AsGuid().Nullable()
                .WithColumn(nameof(Order.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Order.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Order.ApprovedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Order.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Order.Deleted)).AsBoolean();

            Create.Table(nameof(OrderItem))
                .WithColumn(nameof(OrderItem.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(OrderItem.FormId)).AsGuid()
                .WithColumn(nameof(OrderItem.CatalogId)).AsGuid()
                .WithColumn(nameof(OrderItem.LineNbr)).AsInt32()
                .WithColumn(nameof(OrderItem.CategoryId)).AsGuid().Nullable()
                .WithColumn(nameof(OrderItem.Description)).AsString().Nullable()
                .WithColumn(nameof(OrderItem.Qty)).AsDecimal()
                .WithColumn(nameof(OrderItem.Price)).AsDecimal()
                .WithColumn(nameof(OrderItem.SubTotal)).AsDecimal()
                .WithColumn(nameof(OrderItem.DiscountAmount)).AsDecimal()
                // system fields
                .WithColumn(nameof(OrderItem.CreatedById)).AsGuid()
                .WithColumn(nameof(OrderItem.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(OrderItem.CreatedOn)).AsDateTime()
                .WithColumn(nameof(OrderItem.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(OrderItem.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(OrderItem.Deleted)).AsBoolean();
        }
        public override void Down()
        {
            Delete.Table(nameof(Order));
            Delete.Table(nameof(OrderItem));
        }
    }
}
