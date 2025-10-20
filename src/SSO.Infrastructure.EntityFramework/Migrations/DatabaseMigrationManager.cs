using DbUp;
using DbUp.Engine;
using Microsoft.Extensions.Logging;

namespace SSO.Infrastructure.EntityFramework.Migrations;

/// <summary>
/// Database migration manager using DbUp for MySQL
/// </summary>
public class DatabaseMigrationManager
{
    private readonly string _connectionString;
    private readonly ILogger<DatabaseMigrationManager>? _logger;

    public DatabaseMigrationManager(string connectionString, ILogger<DatabaseMigrationManager>? logger = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _logger = logger;
    }

    /// <summary>
    /// Executes database migrations
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public bool MigrateDatabase()
    {
        try
        {
            _logger?.LogInformation("Starting database migration...");

            EnsureDatabase.For.MySqlDatabase(_connectionString);

            var upgrader = DeployChanges.To
                .MySqlDatabase(_connectionString)
                .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrationManager).Assembly)
                .LogToConsole()
                .Build();

            var result = upgrader.PerformUpgrade();

            if (!result.Successful)
            {
                _logger?.LogError(result.Error, "Database migration failed");
                return false;
            }

            _logger?.LogInformation("Database migration completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error during database migration");
            return false;
        }
    }

    /// <summary>
    /// Gets the list of scripts that will be executed
    /// </summary>
    public IEnumerable<string> GetScriptsToExecute()
    {
        var upgrader = DeployChanges.To
            .MySqlDatabase(_connectionString)
            .WithScriptsEmbeddedInAssembly(typeof(DatabaseMigrationManager).Assembly)
            .Build();

        return upgrader.GetScriptsToExecute().Select(s => s.Name);
    }
}
