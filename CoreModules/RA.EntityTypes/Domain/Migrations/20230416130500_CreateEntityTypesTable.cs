using FluentMigrator;
using RAerp.Domain.EntityTypes;

namespace RA.EntityTypes.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20230416130500, "EntityTypes: create table")]
    public class _20230416130500_CreateEntityTypesTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(EntityType))
                // base entity
                .WithColumn(nameof(EntityType.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(EntityType.EntityName)).AsString().Nullable()
                .WithColumn(nameof(EntityType.EntitySystemName)).AsString().Nullable()
                .WithColumn(nameof(EntityType.EntityClassificationName)).AsString().Nullable()
                .WithColumn(nameof(EntityType.InstalledOn)).AsDateTime()
                .WithColumn(nameof(EntityType.UnInstalledOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(EntityType.Installed)).AsBoolean()
                .WithColumn(nameof(EntityType.ParentEntityTypeId)).AsGuid().Nullable();
        }
        public override void Down()
        {
            Delete.Table(nameof(EntityType));
        }
    }
}
