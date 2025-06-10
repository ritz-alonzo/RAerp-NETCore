using FluentMigrator;
using RA.Data.Domain.AccessRightControl;
using RA.Data.Domain.Addresses;

namespace RAerp.Domain.Migrations.Addresses
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20250528190100, "Address: create table")]
    public class _20250528190100_CreateAddressTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Address))
                // base entity
                .WithColumn(nameof(Address.Id)).AsGuid().PrimaryKey()
                // max length for string
                .WithColumn(nameof(Address.AddressLine1)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(Address.AddressLine2)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(Address.AddressLine3)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(Address.RegionCode)).AsString().Nullable()
                .WithColumn(nameof(Address.CityCode)).AsString().Nullable()
                .WithColumn(nameof(Address.BarangayCode)).AsString().Nullable()
                .WithColumn(nameof(Address.ZipCode)).AsString().Nullable()
                .WithColumn(nameof(Address.CreatedById)).AsGuid().Nullable()
                .WithColumn(nameof(Address.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Address.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(Address.ModifiedOn)).AsDateTime2().Nullable();
        }
        public override void Down()
        {
            Delete.Table(nameof(Address));
        }
    }
}
