using Microsoft.EntityFrameworkCore;
using SSO.Domain.Interfaces;
using SSO.Host.Endpoints;
using SSO.Infrastructure.EntityFramework;
using SSO.Infrastructure.EntityFramework.Migrations;
using SSO.Infrastructure.QueryServices.Interfaces;
using SSO.Infrastructure.QueryServices.Services;
using SSO.Infrastructure.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure MySQL connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=sso_db;User=root;Password=password;";

// Configure DbContext with Pomelo MySQL provider
builder.Services.AddDbContext<SsoContext>(options =>
    options.UseMySql(connectionString, 
        ServerVersion.AutoDetect(connectionString),
        mysqlOptions => mysqlOptions.EnableRetryOnFailure()));

// Register repositories
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Register query services
builder.Services.AddScoped<IUserQueryService, UserQueryService>();

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Run database migrations on startup (optional - can be done separately)
if (app.Configuration.GetValue<bool>("RunMigrationsOnStartup", false))
{
    try
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        var migrationManager = new DatabaseMigrationManager(connectionString, logger as ILogger<DatabaseMigrationManager>);
        migrationManager.MigrateDatabase();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "An error occurred while migrating the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors();

// Map endpoints from the Endpoints folder
app.MapUserEndpoints();

app.Run();

