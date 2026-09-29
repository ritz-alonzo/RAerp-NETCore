using FluentMigrator;
using RAerp.Domain.EntityAttributes;

namespace RAerp.Domain.Migrations.FileManager
{
    [Migration(20260717150200)]
    public class _20260717150200_CreateEntityAttributeTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(EntityAttribute))
                .WithColumn(nameof(EntityAttribute.Id)).AsGuid().NotNullable().PrimaryKey()
                .WithColumn(nameof(EntityAttribute.AttributeName)).AsString(200).NotNullable()
                .WithColumn(nameof(EntityAttribute.AttributeDescription)).AsString(255).Nullable()
                .WithColumn(nameof(EntityAttribute.SystemName)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(EntityAttribute.ControlTypeId)).AsInt32().NotNullable()
                .WithColumn(nameof(EntityAttribute.MinValue)).AsInt32().Nullable()
                .WithColumn(nameof(EntityAttribute.MaxValue)).AsInt32().Nullable()
                .WithColumn(nameof(EntityAttribute.RegexPattern)).AsString(Int32.MaxValue).Nullable()
                .WithColumn(nameof(EntityAttribute.IsRequired)).AsBoolean().NotNullable().WithDefaultValue(false)
                .WithColumn(nameof(EntityAttribute.IsActive)).AsBoolean().NotNullable().WithDefaultValue(true)
                // System fields
                .WithColumn(nameof(EntityAttribute.CreatedById)).AsGuid().NotNullable()
                .WithColumn(nameof(EntityAttribute.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(EntityAttribute.CreatedOn)).AsDateTime().NotNullable()
                .WithColumn(nameof(EntityAttribute.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(EntityAttribute.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(EntityAttribute.Deleted)).AsBoolean().NotNullable().WithDefaultValue(false)

                ;

            Create.Table(nameof(EntityAttributeOption))
                .WithColumn(nameof(EntityAttributeOption.Id)).AsGuid().NotNullable().PrimaryKey()
                .WithColumn(nameof(EntityAttributeOption.AttributeId)).AsGuid().NotNullable()
                .WithColumn(nameof(EntityAttributeOption.OptionName)).AsString(255).Nullable()
                .WithColumn(nameof(EntityAttributeOption.OptionValue)).AsString(255).Nullable()
                .WithColumn(nameof(EntityAttributeOption.Description)).AsString(255).Nullable()
                .WithColumn(nameof(EntityAttributeOption.SortOrder)).AsInt32().Nullable()
                .WithColumn(nameof(EntityAttributeOption.IsDefault)).AsBoolean().NotNullable().WithDefaultValue(false)
                ;

            Create.Table(nameof(EntityAttributeValue))
                .WithColumn(nameof(EntityAttributeValue.Id)).AsGuid().NotNullable().PrimaryKey()
                .WithColumn(nameof(EntityAttributeValue.AttributeId)).AsGuid().NotNullable()
                .WithColumn(nameof(EntityAttributeValue.EntityId)).AsGuid().NotNullable()
                .WithColumn(nameof(EntityAttributeValue.Value)).AsString(255).Nullable()
                ;
        }

        public override void Down()
        {
            Delete.Table(nameof(EntityAttribute));
            Delete.Table(nameof(EntityAttributeOption));
            Delete.Table(nameof(EntityAttributeValue));
        }
    }
}