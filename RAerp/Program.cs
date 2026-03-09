using RAerp;
using RAerp.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

#region Configure Services Extensions
builder.Services.RegisterFluentMigration();
builder.Services.RegisterDatabase(builder.Configuration);
builder.Services.RegisterDependencyLifetime();
builder.Services.RegisterFluentValidators();
builder.Services.RegisterAutoMapper();
builder.Services.RegisterMiddlewares();
builder.Services.RegisterPluginDependency(builder.Configuration);
#endregion

var app = builder.Build();

#region App Builder Extensions
app.ApplyFluentMigrations();
app.EnvironmentVariables(builder.Environment);
app.ApplyApplicationMiddlewares();
#endregion

app.Run();
