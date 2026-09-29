using FluentMigrator;
using RAerp.Domain.Users;

namespace RAerp.Domain.Migrations.Users
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20260911151100, "ExternalLogin: create table")]
    public class _20260911151100_CreateExternalLoginTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(ExternalLogin))
                // base entity
                .WithColumn(nameof(ExternalLogin.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(ExternalLogin.UserId)).AsGuid().NotNullable()
                .WithColumn(nameof(ExternalLogin.Provider)).AsString().Nullable()
                .WithColumn(nameof(ExternalLogin.ProviderSubjectId)).AsString().Nullable()
                .WithColumn(nameof(ExternalLogin.ProviderEmail)).AsString().Nullable()
                .WithColumn(nameof(ExternalLogin.ProviderName)).AsString().Nullable()
                .WithColumn(nameof(ExternalLogin.ProviderPictureUrl)).AsString().Nullable()
                .WithColumn(nameof(ExternalLogin.CreatedOn)).AsDateTime()
                .WithColumn(nameof(ExternalLogin.LastLoginOn)).AsDateTime2().Nullable();
            }
        public override void Down()
        {
            Delete.Table(nameof(ExternalLogin));
        }
    }
}
