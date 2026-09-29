using FluentMigrator;

namespace RA.Discounts.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    [Migration(20260630101300, "Discount: create table")]
    public class _20260630101300_CreateDiscountTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Discount))
                // base entity type
                .WithColumn(nameof(Discount.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(Discount.EntityTypeId)).AsGuid()
                .WithColumn(nameof(Discount.Code)).AsString().Nullable()
                .WithColumn(nameof(Discount.Name)).AsString().Nullable()
                .WithColumn(nameof(Discount.DiscountTypeId)).AsInt32()
                .WithColumn(nameof(Discount.DiscountScopeId)).AsInt32()
                .WithColumn(nameof(Discount.Value)).AsDecimal()
                .WithColumn(nameof(Discount.BuyQty)).AsInt32().Nullable()
                .WithColumn(nameof(Discount.GetQty)).AsInt32().Nullable()
                .WithColumn(nameof(Discount.MinOrderAmount)).AsDecimal().Nullable()
                .WithColumn(nameof(Discount.MinQty)).AsInt32().Nullable()
                .WithColumn(nameof(Discount.EligibleCatalogIds)).AsString().Nullable()
                .WithColumn(nameof(Discount.ValidFrom)).AsDateTime()
                .WithColumn(nameof(Discount.ValidTo)).AsDateTime()
                .WithColumn(nameof(Discount.UsageLimitTotal)).AsInt32().Nullable()
                .WithColumn(nameof(Discount.UsageLimitPerCustomer)).AsInt32().Nullable()
                .WithColumn(nameof(Discount.UsageCount)).AsInt32()
                .WithColumn(nameof(Discount.IsActive)).AsBoolean()
                // Base Entity Type System Fields
                .WithColumn(nameof(Discount.Description)).AsString().Nullable()
                .WithColumn(nameof(Discount.CreatedById)).AsGuid()
                .WithColumn(nameof(Discount.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(Discount.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Discount.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Discount.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Discount.Deleted)).AsBoolean();

            Create.Table(nameof(DiscountRedemption))
                .WithColumn(nameof(DiscountRedemption.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(DiscountRedemption.DiscountId)).AsGuid()
                .WithColumn(nameof(DiscountRedemption.DiscountCode)).AsString().Nullable()
                .WithColumn(nameof(DiscountRedemption.OrderId)).AsGuid()
                .WithColumn(nameof(DiscountRedemption.OrderNbr)).AsString().Nullable()
                .WithColumn(nameof(DiscountRedemption.CustomerId)).AsGuid()
                .WithColumn(nameof(DiscountRedemption.OriginalAmount)).AsDecimal()
                .WithColumn(nameof(DiscountRedemption.DiscountAmount)).AsDecimal()
                .WithColumn(nameof(DiscountRedemption.DiscountedAmount)).AsDecimal()
                .WithColumn(nameof(DiscountRedemption.RedeemedAt)).AsDateTime()
                .WithColumn(nameof(DiscountRedemption.IsPending)).AsBoolean()
                ;
        }
        public override void Down()
        {
            Delete.Table(nameof(Discount));
            Delete.Table(nameof(DiscountRedemption));
        }
    }
}
