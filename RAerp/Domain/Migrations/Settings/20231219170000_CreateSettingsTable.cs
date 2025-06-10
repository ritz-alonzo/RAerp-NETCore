using FluentMigrator;
using RA.Data.Domain.Settings;

namespace RAerp.Domain.Migrations.Settings
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    [Migration(20231219170000, "Settings: create table")]
    public class _20231219170000_CreateSettingsTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Setting))
                .WithColumn(nameof(Setting.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(Setting.Name)).AsString().Nullable()
                .WithColumn(nameof(Setting.SystemName)).AsString().Nullable()
                .WithColumn(nameof(Setting.Data)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(Setting.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Setting.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Setting.EntityTypeId)).AsGuid().Nullable()
                .WithColumn(nameof(Setting.FormTypeId)).AsGuid().Nullable();
        }

        public override void Down()
        {
            Delete.Table(nameof(Setting));
        }
    }
}
