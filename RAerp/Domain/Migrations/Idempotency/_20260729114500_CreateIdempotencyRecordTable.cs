using FluentMigrator;
using RAerp.Domain.Application;
using RAerp.Security.Idempontency.Domain;

namespace RAerp.Domain.Migrations.Idempotency
{
    [Migration(20260729114500)]
    public class _20260729114500_CreateIdempotencyRecordTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(IdempotencyRecord))
                .WithColumn(nameof(IdempotencyRecord.Id)).AsGuid().NotNullable().PrimaryKey()
                .WithColumn(nameof(IdempotencyRecord.Key)).AsString(255).NotNullable()
                .WithColumn(nameof(IdempotencyRecord.PayloadHash)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(IdempotencyRecord.Endpoint)).AsString(500).Nullable()
                .WithColumn(nameof(IdempotencyRecord.Status)).AsString(50).Nullable()
                .WithColumn(nameof(IdempotencyRecord.StatusCode)).AsInt32().Nullable()
                .WithColumn(nameof(IdempotencyRecord.Response)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(IdempotencyRecord.Error)).AsString(500).Nullable()
                .WithColumn(nameof(IdempotencyRecord.CreatedUtc)).AsDateTime().NotNullable()
                .WithColumn(nameof(IdempotencyRecord.CompletedUtc)).AsDateTime2().Nullable();

            Create.Index("IX_IdempotencyKey")
                .OnTable(nameof(IdempotencyRecord))
                .OnColumn(nameof(IdempotencyRecord.Key)).Ascending()
                .WithOptions().Unique();
        }

        public override void Down()
        {
            Delete.Table(nameof(IdempotencyRecord));
            Delete.Index("IX_IdempotencyKey").OnTable(nameof(IdempotencyRecord));
        }
    }
}