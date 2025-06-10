using FluentMigrator;
using RA.Data.Domain.Users;

namespace RAerp.Domain.Migrations.Users
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20240226151100, "UserAndUserRole: create table")]
    public class _20240226151100_CreateUserAndUserRoleTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(User))
                // base entity
                .WithColumn(nameof(User.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(User.FirstName)).AsString().Nullable()
                .WithColumn(nameof(User.LastName)).AsString().Nullable()
                .WithColumn(nameof(User.Email)).AsString().Nullable()
                .WithColumn(nameof(User.ContactNo)).AsString().Nullable()
                .WithColumn(nameof(User.Username)).AsString().Nullable()
                .WithColumn(nameof(User.Password)).AsString().Nullable()
                .WithColumn(nameof(User.Salt)).AsString().Nullable()
                .WithColumn(nameof(User.CreatedById)).AsGuid().Nullable()
                .WithColumn(nameof(User.CreatedOn)).AsDateTime()
                .WithColumn(nameof(User.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(User.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(User.LastActivityDate)).AsDateTime2().Nullable()
                .WithColumn(nameof(User.LastLoginDate)).AsDateTime2().Nullable()
                .WithColumn(nameof(User.LastPasswordChangedDate)).AsDateTime2().Nullable()
                .WithColumn(nameof(User.Deleted)).AsBoolean()
                .WithColumn(nameof(User.AccountStatusId)).AsInt32();

            Create.Table(nameof(UserRole))
                .WithColumn(nameof(UserRole.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(UserRole.Rolename)).AsString().Nullable()
                .WithColumn(nameof(UserRole.CreatedById)).AsGuid().Nullable()
                .WithColumn(nameof(UserRole.CreatedOn)).AsDateTime()
                .WithColumn(nameof(UserRole.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(UserRole.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(UserRole.Deleted)).AsBoolean();

            Create.Table(nameof(UserUserRoleMapping))
                .WithColumn(nameof(UserUserRoleMapping.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(UserUserRoleMapping.UserId)).AsGuid()
                .WithColumn(nameof(UserUserRoleMapping.UserRoleId)).AsGuid();
        }
        public override void Down()
        {
            Delete.Table(nameof(User));
            Delete.Table(nameof(UserRole));
            Delete.Table(nameof(UserUserRoleMapping));
        }
    }
}
