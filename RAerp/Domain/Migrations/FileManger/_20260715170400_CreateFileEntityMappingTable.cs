using FluentMigrator;
using RAerp.Domain.FileManager;

namespace RAerp.Domain.Migrations.FileManager
{
    [Migration(20260715170400)]
    public class _20260715170400_CreateFileEntityMappingTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(FileEntityMapping))
                .WithColumn(nameof(FileEntityMapping.Id)).AsGuid().NotNullable().PrimaryKey()
                .WithColumn(nameof(FileEntityMapping.EntityId)).AsGuid().NotNullable()
                .WithColumn(nameof(FileEntityMapping.EntitySystemName)).AsString(200).Nullable()
                .WithColumn(nameof(FileEntityMapping.FileName)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(FileEntityMapping.FilePath)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(FileEntityMapping.FileType)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(FileEntityMapping.FileSize)).AsInt64().Nullable()
                .WithColumn(nameof(FileEntityMapping.CreatedAt)).AsDateTime().NotNullable();
        }

        public override void Down()
        {
            Delete.Table(nameof(FileEntityMapping));
        }
    }
}