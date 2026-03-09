using FluentMigrator;
using RA.OrdersManagement.Domain.Payments;

namespace RA.OrdersManagement.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20250704094000, "Payment, PaymentItem: create table")]
    public class _20250704094000_CreateCartTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Payment))
                .WithColumn(nameof(Payment.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(Payment.FormNbr)).AsString().Nullable()
                .WithColumn(nameof(Payment.OrderId)).AsGuid()
                .WithColumn(nameof(Payment.PaymentRefNbr)).AsString().Nullable()
                .WithColumn(nameof(Payment.Description)).AsString().Nullable()
                .WithColumn(nameof(Payment.PaymentDate)).AsDateTime()
                .WithColumn(nameof(Payment.PaymentStatusId)).AsInt32()
                .WithColumn(nameof(Payment.StatusId)).AsInt32()
                .WithColumn(nameof(Payment.TotalQty)).AsDecimal()
                .WithColumn(nameof(Payment.TotalDiscountAmount)).AsDecimal()
                .WithColumn(nameof(Payment.TotalVatAmount)).AsDecimal()
                .WithColumn(nameof(Payment.TotalGrossAmount)).AsDecimal()
                .WithColumn(nameof(Payment.TotalNetAmount)).AsDecimal()
                .WithColumn(nameof(Payment.AmountPaid)).AsDecimal()
                .WithColumn(nameof(Payment.ChangeAmount)).AsDecimal()
                // system fields
                .WithColumn(nameof(Payment.CreatedById)).AsGuid()
                .WithColumn(nameof(Payment.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(Payment.ApprovedById)).AsGuid().Nullable()
                .WithColumn(nameof(Payment.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Payment.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Payment.ApprovedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Payment.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Payment.Deleted)).AsBoolean();

            Create.Table(nameof(PaymentItem))
                .WithColumn(nameof(PaymentItem.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(PaymentItem.FormId)).AsGuid()
                .WithColumn(nameof(PaymentItem.CatalogId)).AsGuid()
                .WithColumn(nameof(PaymentItem.LineNbr)).AsInt32()
                .WithColumn(nameof(PaymentItem.CategoryId)).AsGuid().Nullable()
                .WithColumn(nameof(PaymentItem.Description)).AsString().Nullable()
                .WithColumn(nameof(PaymentItem.Qty)).AsDecimal()
                .WithColumn(nameof(PaymentItem.Price)).AsDecimal()
                .WithColumn(nameof(PaymentItem.SubTotal)).AsDecimal()
                .WithColumn(nameof(PaymentItem.DiscountAmount)).AsDecimal()
                // system fields
                .WithColumn(nameof(PaymentItem.CreatedById)).AsGuid()
                .WithColumn(nameof(PaymentItem.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(PaymentItem.CreatedOn)).AsDateTime()
                .WithColumn(nameof(PaymentItem.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(PaymentItem.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(PaymentItem.Deleted)).AsBoolean();
        }
        public override void Down()
        {
            Delete.Table(nameof(Payment));
            Delete.Table(nameof(PaymentItem));
        }
    }
}
