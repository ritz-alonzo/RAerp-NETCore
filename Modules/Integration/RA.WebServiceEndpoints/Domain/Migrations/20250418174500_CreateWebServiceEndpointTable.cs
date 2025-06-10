using FluentMigrator;

namespace RA.WebServiceEndpoints.Domain.Migrations
{
    /// <summary>
    /// Migration version format : year, month, day, time : hr : min : sec
    /// </summary>
    ///
    [Migration(20250418174500, "WebServiceEndpoint: create table")]
    public class _20250418174500_CreateWebServiceEndpointTable : Migration
    {
        public override void Up()
        {
            Create.Table(nameof(WebServiceEndpoint))
                // base entity type
                .WithColumn(nameof(WebServiceEndpoint.Id)).AsGuid().PrimaryKey()
                .WithColumn(nameof(WebServiceEndpoint.EndpointName)).AsString().Nullable()
                .WithColumn(nameof(WebServiceEndpoint.EndpointDomain)).AsString().Nullable()
                .WithColumn(nameof(WebServiceEndpoint.EndpointDescription)).AsString().Nullable()
                .WithColumn(nameof(WebServiceEndpoint.CreatedById)).AsGuid()
                .WithColumn(nameof(WebServiceEndpoint.ModifiedById)).AsGuid().Nullable()
                .WithColumn(nameof(WebServiceEndpoint.CreatedOn)).AsDateTime()
                .WithColumn(nameof(WebServiceEndpoint.ModifiedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(WebServiceEndpoint.DeletedOn)).AsDateTime2().Nullable()
                .WithColumn(nameof(WebServiceEndpoint.Deleted)).AsBoolean()
                .WithColumn(nameof(WebServiceEndpoint.EndpointEntityTypeId)).AsGuid().Nullable();
        }
        public override void Down()
        {
            Delete.Table(nameof(WebServiceEndpoint));
        }
    }
}
