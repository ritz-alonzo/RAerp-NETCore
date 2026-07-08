using FluentMigrator;

namespace RA.WebServiceEndpoints.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20260626143500, "Refresh token: create table")]
    public class _20260626143500_CreateRefreshTokenTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(RefreshToken))
                .WithColumn(nameof(RefreshToken.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(RefreshToken.UserId)).AsGuid()
                .WithColumn(nameof(RefreshToken.Token)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(RefreshToken.AccessToken)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(RefreshToken.Family)).AsString().Nullable()
                .WithColumn(nameof(RefreshToken.CreatedAt)).AsDateTime()
                .WithColumn(nameof(RefreshToken.ExpiresAt)).AsDateTime()
                .WithColumn(nameof(RefreshToken.IsRevoked)).AsBoolean()
                .WithColumn(nameof(RefreshToken.IsUsed)).AsBoolean();
        }
        public override void Down()
        {
            Delete.Table(nameof(RefreshToken));
        }
    }
}
