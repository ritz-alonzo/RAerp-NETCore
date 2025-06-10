using FluentMigrator;
using RA.Categories.Domain;

namespace RA.EntityTypes.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20240315205700, "Category: create table")]
    public class _20240315205700_CreateCategoryTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(Category))
                // base entity type
                .WithColumn(nameof(Category.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(Category.EntityTypeId)).AsGuid()
                .WithColumn(nameof(Category.Code)).AsString().Nullable()
                .WithColumn(nameof(Category.Name)).AsString().Nullable()
                .WithColumn(nameof(Category.Description)).AsString().Nullable()
                .WithColumn(nameof(Category.CreatedById)).AsGuid()
                .WithColumn(nameof(Category.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(Category.CreatedOn)).AsDateTime()
                .WithColumn(nameof(Category.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Category.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(Category.Deleted)).AsBoolean()
                .WithColumn(nameof(Category.StatusId)).AsInt32();

            Create.Table(nameof(CategoryEntityMapping))
                .WithColumn(nameof(CategoryEntityMapping.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(CategoryEntityMapping.CategoryId)).AsGuid().Nullable()
                .WithColumn(nameof(CategoryEntityMapping.EntityId)).AsGuid().Nullable();
        }
        public override void Down()
        {
            Delete.Table(nameof(Category));
            Delete.Table(nameof(CategoryEntityMapping));
        }
    }
}
