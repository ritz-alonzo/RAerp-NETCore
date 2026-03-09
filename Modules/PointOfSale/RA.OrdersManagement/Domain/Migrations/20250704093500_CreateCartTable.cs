using FluentMigrator;
using RA.OrdersManagement.Domain.Carts;

namespace RA.OrdersManagement.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20250704093500, "Cart, CartItem: create table")]
    public class _20250704093500_CreateCartTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Cart))
                .WithColumn(nameof(Cart.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(Cart.FormNbr)).AsString().Nullable()
                .WithColumn(nameof(Cart.ServiceId)).AsGuid().Nullable()
                .WithColumn(nameof(Cart.OrderId)).AsGuid().Nullable()
                .WithColumn(nameof(Cart.CustomerName)).AsString().Nullable()
                .WithColumn(nameof(Cart.Description)).AsString().Nullable()
                .WithColumn(nameof(Cart.StatusId)).AsInt32()
                .WithColumn(nameof(Cart.TotalQty)).AsDecimal()
                .WithColumn(nameof(Cart.TotalAmount)).AsDecimal()
                // system fields
                .WithColumn(nameof(Cart.CreatedById)).AsGuid()
                .WithColumn(nameof(Cart.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(Cart.ApprovedById)).AsGuid().Nullable()
                .WithColumn(nameof(Cart.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Cart.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Cart.ApprovedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Cart.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Cart.Deleted)).AsBoolean();

            Create.Table(nameof(CartItem))
                .WithColumn(nameof(CartItem.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(CartItem.FormId)).AsGuid()
                .WithColumn(nameof(CartItem.CatalogId)).AsGuid()
                .WithColumn(nameof(CartItem.LineNbr)).AsInt32()
                .WithColumn(nameof(CartItem.CategoryId)).AsGuid().Nullable()
                .WithColumn(nameof(CartItem.Description)).AsString().Nullable()
                .WithColumn(nameof(CartItem.Qty)).AsDecimal()
                .WithColumn(nameof(CartItem.Price)).AsDecimal()
                .WithColumn(nameof(CartItem.SubTotal)).AsDecimal()
                // system fields
                .WithColumn(nameof(CartItem.CreatedById)).AsGuid()
                .WithColumn(nameof(CartItem.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(CartItem.CreatedOn)).AsDateTime()
                .WithColumn(nameof(CartItem.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(CartItem.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(CartItem.Deleted)).AsBoolean();
        }
        public override void Down()
        {
            Delete.Table(nameof(Cart));
            Delete.Table(nameof(CartItem));
        }
    }
}
