using FluentMigrator;
using RA.Data.Domain.AccessRightControl;

namespace RAerp.Domain.Migrations.Users
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20240308231100, "AccessRights: create table")]
    public class _20240308231100_CreateUserAndUserRoleTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(AccessRights))
                // base entity
                .WithColumn(nameof(AccessRights.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(AccessRights.UserRoleId)).AsGuid().Nullable()
                // max length for string
                .WithColumn(nameof(AccessRights.Rolename)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(AccessRights.AccessRecordData)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(AccessRights.CreatedById)).AsGuid().Nullable()
                .WithColumn(nameof(AccessRights.CreatedOn)).AsDateTime()
                .WithColumn(nameof(AccessRights.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(AccessRights.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(AccessRights.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(AccessRights.Deleted)).AsBoolean();
        }
        public override void Down()
        {
            Delete.Table(nameof(AccessRights));
        }
    }
}
