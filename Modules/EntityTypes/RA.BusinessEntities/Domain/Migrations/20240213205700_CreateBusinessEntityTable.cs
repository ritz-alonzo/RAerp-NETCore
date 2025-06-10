using FluentMigrator;
using RA.BusinessEntities.Domain;

namespace RA.EntityTypes.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20240213205700, "BusinessEntity: create table")]
    public class _20240213205700_CreateBusinessEntityTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(BusinessEntity))
                // base entity type
                .WithColumn(nameof(BusinessEntity.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(BusinessEntity.EntityTypeId)).AsGuid()
                .WithColumn(nameof(BusinessEntity.Code)).AsString().Nullable()
                .WithColumn(nameof(BusinessEntity.Name)).AsString().Nullable()
                .WithColumn(nameof(BusinessEntity.Description)).AsString().Nullable()
                .WithColumn(nameof(BusinessEntity.CreatedById)).AsGuid()
                .WithColumn(nameof(BusinessEntity.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(BusinessEntity.CreatedOn)).AsDateTime()
                .WithColumn(nameof(BusinessEntity.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(BusinessEntity.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(BusinessEntity.Deleted)).AsBoolean()
                .WithColumn(nameof(BusinessEntity.StatusId)).AsInt32()
                .WithColumn(nameof(BusinessEntity.CategoryId)).AsGuid().Nullable()
                .WithColumn(nameof(BusinessEntity.AddressId)).AsGuid().Nullable();
        }
        public override void Down()
        {
            Delete.Table(nameof(BusinessEntity));
        }
    }
}
