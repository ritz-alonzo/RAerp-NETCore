using FluentMigrator;
using RAerp.Domain.Application;

namespace RAerp.Domain.Migrations.ApplicationSettings
{
    [Migration(20260505142000)]
    public class _20260505142000_CreateApplicationSettingTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(ApplicationSetting))
                .WithColumn(nameof(ApplicationSetting.Id)).AsGuid().NotNullable().PrimaryKey()
                .WithColumn(nameof(ApplicationSetting.ApplicationName)).AsString(200).Nullable()
                .WithColumn(nameof(ApplicationSetting.CompanyName)).AsString(200).Nullable()
                .WithColumn(nameof(ApplicationSetting.ClientId)).AsString(500).Nullable()
                .WithColumn(nameof(ApplicationSetting.ClientSecret)).AsString(500).Nullable()
                .WithColumn(nameof(ApplicationSetting.Description)).AsString(1000).Nullable()
                .WithColumn(nameof(ApplicationSetting.IsActive)).AsBoolean().NotNullable().WithDefaultValue(true)
                .WithColumn(nameof(ApplicationSetting.CreatedById)).AsGuid().NotNullable()
                .WithColumn(nameof(ApplicationSetting.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(ApplicationSetting.CreatedOn)).AsDateTime().NotNullable()
                .WithColumn(nameof(ApplicationSetting.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(ApplicationSetting.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(ApplicationSetting.LoginAttemptLimit)).AsInt32().NotNullable()
                .WithColumn(nameof(ApplicationSetting.OneTimePINAttemptLimit)).AsInt32().NotNullable()
                .WithColumn(nameof(ApplicationSetting.EmailVerificationAttemptLimit)).AsInt32().NotNullable()
                .WithColumn(nameof(ApplicationSetting.EmailLimit)).AsInt32().NotNullable()
                .WithColumn(nameof(ApplicationSetting.Deleted)).AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn(nameof(ApplicationSetting.IsEmailVerificationEnabled)).AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn(nameof(ApplicationSetting.DefaultTimeZone)).AsString().Nullable();
        }

        public override void Down()
        {
            Delete.Table(nameof(ApplicationSetting));
        }
    }
}