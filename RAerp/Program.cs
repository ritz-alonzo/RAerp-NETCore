using RAerp;
using RAerp.Extensions;
using RAerp.Helpers.DateTimeHelper;
using RAerp.Helpers.PluginHelper;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
// Add services to the container.
var mvcBuilder = builder.Services.AddControllersWithViews();

builder.Services.AddOptions<Microsoft.AspNetCore.Mvc.JsonOptions>()
    .Configure<IHttpContextAccessor>((options, httpContextAccessor) =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonDateTimeConverterHelper(httpContextAccessor)
        );
    });

#region Configure Services Extensions
builder.Services.RegisterFluentMigration();
builder.Services.RegisterDatabase(builder.Configuration);
builder.Services.RegisterDependencyLifetime();
builder.Services.RegisterFluentValidators();
builder.Services.RegisterAutoMapper();
builder.Services.RegisterMiddlewares();
builder.Services.RegisterPluginDependency(builder.Configuration);
builder.Services.RegisterEmailConfiguration(builder.Configuration);
#endregion

var app = builder.Build();

#region App Builder Extensions
app.ApplyFluentMigrations();
app.EnvironmentVariables(builder.Environment);
app.ApplyApplicationMiddlewares();
#endregion

app.MapGet("/debug/views", (Microsoft.AspNetCore.Mvc.ApplicationParts.ApplicationPartManager apm) =>
{
    var feature = new Microsoft.AspNetCore.Mvc.Razor.Compilation.ViewsFeature();
    apm.PopulateFeature(feature);
    return Results.Ok(feature.ViewDescriptors.Select(v => v.RelativePath));
});

app.Run();
