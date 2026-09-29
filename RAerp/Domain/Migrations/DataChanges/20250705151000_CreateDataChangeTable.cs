using FluentMigrator;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using RAerp.Domain.DataChanges;

namespace RAerp.Domain.Migrations.DataChanges
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20250705151000, "DataChange: create table")]
    public class _20250705151000_CreateDataChangeTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(DataChange))
                .WithColumn(nameof(DataChange.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(DataChange.DataId)).AsGuid()
                .WithColumn(nameof(DataChange.ItemDataId)).AsGuid().Nullable()
                .WithColumn(nameof(DataChange.Record)).AsString(int.MaxValue).Nullable()
                .WithColumn(nameof(DataChange.SystemName)).AsString().Nullable()
                .WithColumn(nameof(DataChange.IsApplied)).AsBoolean()
                .WithColumn(nameof(DataChange.StatusId)).AsInt32()
                .WithColumn(nameof(DataChange.CreatedOn)).AsDateTime();
        }
        public override void Down()
        {
            Delete.Table(nameof(DataChange));
        }
    }
}
