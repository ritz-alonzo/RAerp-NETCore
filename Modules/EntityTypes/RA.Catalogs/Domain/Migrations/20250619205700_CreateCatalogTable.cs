using FluentMigrator;
using RA.Catalogs.Domain;

namespace RA.EntityTypes.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    [Migration(20250619205700, "Catalog: create table")]
    public class _20250619205700_CreateCatalogTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Catalog))
                // base entity type
                .WithColumn(nameof(Catalog.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(Catalog.EntityTypeId)).AsGuid()
                .WithColumn(nameof(Catalog.Code)).AsString().Nullable()
                .WithColumn(nameof(Catalog.Name)).AsString().Nullable()
                .WithColumn(nameof(Catalog.SKU)).AsString().Nullable()
                .WithColumn(nameof(Catalog.BarcodeValue)).AsString().Nullable()
                .WithColumn(nameof(Catalog.TypeId)).AsInt32()
                .WithColumn(nameof(Catalog.UOMId)).AsGuid().Nullable()
                .WithColumn(nameof(Catalog.Price)).AsDecimal()
                .WithColumn(nameof(Catalog.StatusId)).AsInt32()
                .WithColumn(nameof(Catalog.Description)).AsString().Nullable()
                .WithColumn(nameof(Catalog.ImagePath)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(Catalog.CreatedById)).AsGuid()
                .WithColumn(nameof(Catalog.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(Catalog.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Catalog.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Catalog.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Catalog.Deleted)).AsBoolean();
        }
        public override void Down()
        {
            Delete.Table(nameof(Catalog));
        }
    }
}
